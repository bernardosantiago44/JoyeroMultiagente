using UnityEngine;

/// <summary>
/// Controlador del robot. Implementa la máquina de estados (Idle/Explore/GoToJewel/…),
/// coordina percepción, pathfinding y acciones atómicas (mover, recoger, dejar).
/// </summary>
/// <remarks>
/// Interacciones:
/// - Implementa <see cref="IAgent"/> y es orquestado por <see cref="SimulationManager"/>.
/// - Usa <see cref="PathfindingComponent"/> y consulta <see cref="GridService"/>.
/// - Respeta reglas globales de <see cref="RuleSystem"/> y reporta a <see cref="MetricsLogger"/>.
/// </remarks>
public sealed class RobotController : MonoBehaviour, IAgent
{
    [SerializeField] private int _robotId;
    [SerializeField] private float _moveSpeed = 2.0f; // cells per second
    
    private GridService _gridService;
    private PathfindingComponent _pathfindingComponent;
    private MetricsLogger _metricsLogger;
    private RuleSystem _ruleSystem;
    private Vector2Int _currentCell;
    private Vector2Int _finalGoal; // Store final destination for replanning
    private RobotState _currentState = RobotState.Idle;
    private Vector3? _moveTarget;
    private float _moveStartTime;
    private Vector3 _moveStartPosition;
    
    // Jewel carrying state
    private Jewel _carriedJewel;
    
    // Anti-loop system
    private Vector2Int _lastTargetCell = Vector2Int.one * -1; // Invalid position initially
    private int _sameTargetAttempts = 0;
    private const int MAX_SAME_TARGET_ATTEMPTS = 4;
    
    /// <summary>Estados básicos del robot para el movimiento y acciones</summary>
    private enum RobotState
    {
        Idle,
        MovingToTarget,
        SeekingJewel,
        MovingToJewel,
        MovingToZone
    }
    
    /// <summary>ID único del robot</summary>
    public int Id => _robotId;
    
    /// <summary>Posición actual del robot en el grid</summary>
    public Vector2Int Cell => _currentCell;
    
    /// <summary>True si el robot está cargando una joya</summary>
    public bool IsCarryingJewel => _carriedJewel != null;
    
    /// <summary>Joya que está cargando el robot, null si no tiene ninguna</summary>
    public Jewel CarriedJewel => _carriedJewel;

    private void Awake()
    {
        // Obtener PathfindingComponent en el mismo GameObject
        _pathfindingComponent = GetComponent<PathfindingComponent>();
        if (_pathfindingComponent == null)
        {
            Debug.LogError($"[RobotController] PathfindingComponent not found on {gameObject.name}");
        }
    }

    private void Start()
    {
        // Obtener GridService del registro
        if (!ServiceRegistry.TryResolve<GridService>(out _gridService))
        {
            Debug.LogError("[RobotController] GridService not found in ServiceRegistry.");
            return;
        }

        // Obtener servicios opcionales
        ServiceRegistry.TryResolve<MetricsLogger>(out _metricsLogger);
        ServiceRegistry.TryResolve<RuleSystem>(out _ruleSystem);

        // Validar posición inicial en el grid
        _currentCell = _gridService.WorldToCell(transform.position);
        if (!_gridService.IsInside(_currentCell) || !_gridService.IsWalkable(_currentCell))
        {
            Debug.LogError($"[RobotController] Initial position {_currentCell} is invalid or not walkable.");
            return;
        }

        // Alinear la posición del robot al centro de la celda del grid
        Vector3 alignedPosition = _gridService.CellToWorld(_currentCell);
        transform.position = alignedPosition;
        Debug.Log($"[RobotController] Robot {Id} position aligned to grid at {alignedPosition} (cell {_currentCell})");

        // Marcar la celda como ocupada por este robot
        _gridService.AddOccupant(_currentCell, CellOccupant.Robot);

        Debug.Log($"[RobotController] Robot {Id} initialized at cell {_currentCell}");
    }

    /// <summary>
    /// Comando principal: mover el robot a una celda objetivo.
    /// </summary>
    /// <param name="targetCell">Celda de destino</param>
    /// <returns>true si se pudo iniciar el movimiento</returns>
    public bool MoveToCell(Vector2Int targetCell)
    {
        if (_pathfindingComponent == null || _gridService == null)
        {
            Debug.LogWarning($"[RobotController] Cannot move - missing dependencies");
            return false;
        }
        
        if (_currentState == RobotState.MovingToTarget || _currentState == RobotState.MovingToJewel || _currentState == RobotState.MovingToZone)
        {
            Debug.LogWarning($"[RobotController] Robot {Id} is already moving");
            return false;
        }
        
        // Validar que la celda objetivo esté dentro del grid
        if (!_gridService.IsInside(targetCell))
        {
            Debug.LogWarning($"[RobotController] Target cell {targetCell} is outside grid bounds");
            return false;
        }
        
        // Validar que la celda objetivo sea walkable
        if (!_gridService.IsWalkable(targetCell))
        {
            Debug.LogWarning($"[RobotController] Target cell {targetCell} is not walkable");
            return false;
        }
        
        // Solicitar ruta al PathfindingComponent
        if (_pathfindingComponent.RequestPathToCellPosition(_currentCell, targetCell))
        {
            _currentState = RobotState.MovingToTarget;
            _finalGoal = targetCell; // Store for potential replanning
            Debug.Log($"[RobotController] Robot {Id} starting movement from {_currentCell} to {targetCell}");
            return true;
        }
        else
        {
            Debug.LogWarning($"[RobotController] No path found from {_currentCell} to {targetCell}");
            return false;
        }
    }

    /// <summary>
    /// Update de Unity - llama al Tick del agente cada frame.
    /// </summary>
    private void Update()
    {
        Tick();
    }

    /// <summary>
    /// Tick del agente - ejecuta lógica de movimiento y percepción.
    /// </summary>
    public void Tick()
    {
        if (_gridService == null) return;
        
        switch (_currentState)
        {
            case RobotState.Idle:
                // En idle, planificar siguiente acción
                PlanNextAction();
                break;
                
            case RobotState.MovingToTarget:
            case RobotState.MovingToJewel:
            case RobotState.MovingToZone:
                UpdateMovement();
                break;
                
            case RobotState.SeekingJewel:
                // Buscar joya y empezar movimiento hacia ella
                SeekNearestJewel();
                break;
        }
    }
    
    /// <summary>
    /// Planifica la siguiente acción cuando no tiene plan activo.
    /// Lógica inteligente: buscar joya -> recogerla -> entregarla en zona apropiada.
    /// </summary>
    public void PlanNextAction()
    {
        if (_gridService == null) return;
        
        // Si ya está cargando una joya, buscar zona apropiada
        if (IsCarryingJewel)
        {
            var targetZone = FindNearestCompatibleZone(_carriedJewel.Color);
            if (targetZone != null)
            {
                Debug.Log($"[RobotController] Robot {Id} planning to deliver {_carriedJewel.Color} jewel to zone at {targetZone.Cell}");
                
                // Anti-loop check
                if (CheckAndHandleLoop(targetZone.Cell)) return;
                
                if (MoveToCell(targetZone.Cell))
                {
                    _currentState = RobotState.MovingToZone;
                }
            }
            else
            {
                Debug.LogWarning($"[RobotController] Robot {Id} couldn't find compatible zone for {_carriedJewel.Color} jewel");
                // Si no encuentra zona, moverse aleatoriamente
                MoveToRandomPosition();
            }
        }
        // Si no tiene joya, buscar una para recoger
        else
        {
            _currentState = RobotState.SeekingJewel;
        }
    }
    
    /// <summary>
    /// Actualiza el movimiento del robot siguiendo el path.
    /// </summary>
    private void UpdateMovement()
    {
        if (_pathfindingComponent == null || !_pathfindingComponent.HasPath)
        {
            // No hay path, manejar según el estado actual
            HandleMovementCompleted();
            return;
        }
        
        // Si no estamos moviéndonos a un waypoint, iniciar movimiento al siguiente
        if (_moveTarget == null)
        {
            StartMoveToNextWaypoint();
        }
        
        // Actualizar movimiento hacia el waypoint actual
        if (_moveTarget.HasValue)
        {
            UpdateMoveToWaypoint();
        }
    }

    /// <summary>
    /// Maneja el comportamiento cuando se completa un movimiento según el estado actual.
    /// </summary>
    private void HandleMovementCompleted()
    {
        switch (_currentState)
        {
            case RobotState.MovingToJewel:
                // Intentar recoger la joya
                if (TryPickupJewel())
                {
                    Debug.Log($"[RobotController] Robot {Id} successfully picked up jewel. Planning delivery...");
                    _currentState = RobotState.Idle; // Triggera PlanNextAction para buscar zona
                }
                else
                {
                    Debug.LogWarning($"[RobotController] Robot {Id} failed to pick up jewel. Seeking another...");
                    _currentState = RobotState.SeekingJewel;
                }
                break;
                
            case RobotState.MovingToZone:
                // Intentar entregar la joya
                if (TryDropJewel())
                {
                    Debug.Log($"[RobotController] Robot {Id} successfully delivered jewel. Seeking next task...");
                    _currentState = RobotState.Idle; // Triggera PlanNextAction para buscar nueva joya
                }
                else
                {
                    Debug.LogWarning($"[RobotController] Robot {Id} failed to deliver jewel. Trying again...");
                    _currentState = RobotState.Idle; // Replaneará
                }
                break;
                
            default:
                _currentState = RobotState.Idle;
                break;
        }
    }

    /// <summary>
    /// Busca la joya más cercana disponible y se mueve hacia ella.
    /// </summary>
    private void SeekNearestJewel()
    {
        var nearestJewel = FindNearestAvailableJewel();
        if (nearestJewel != null)
        {
            Debug.Log($"[RobotController] Robot {Id} found nearest jewel ({nearestJewel.Color}) at {nearestJewel.Cell}");
            
            // Anti-loop check
            if (CheckAndHandleLoop(nearestJewel.Cell)) return;
            
            if (MoveToCell(nearestJewel.Cell))
            {
                _currentState = RobotState.MovingToJewel;
            }
            else
            {
                Debug.LogWarning($"[RobotController] Robot {Id} couldn't find path to jewel at {nearestJewel.Cell}");
                _currentState = RobotState.Idle; // Reintentará en el próximo tick
            }
        }
        else
        {
            Debug.Log($"[RobotController] Robot {Id} couldn't find any available jewels. Moving randomly to explore.");
            MoveToRandomPosition();
        }
    }
    
    /// <summary>
    /// Inicia el movimiento hacia el siguiente waypoint.
    /// </summary>
    private void StartMoveToNextWaypoint()
    {
        var nextWaypointWorld = _pathfindingComponent.GetNextWaypointWorldPosition();
        if (nextWaypointWorld.HasValue)
        {
            Vector3 targetPosition = nextWaypointWorld.Value;
            
            // Validar que la posición objetivo sea válida
            if (float.IsNaN(targetPosition.x) || float.IsNaN(targetPosition.y) || float.IsNaN(targetPosition.z) ||
                float.IsInfinity(targetPosition.x) || float.IsInfinity(targetPosition.y) || float.IsInfinity(targetPosition.z))
            {
                Debug.LogError($"[RobotController] Invalid target position: {targetPosition}. Stopping movement.");
                _currentState = RobotState.Idle;
                return;
            }
            
            // Validar que la posición inicial sea válida
            if (float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y) || float.IsNaN(transform.position.z) ||
                float.IsInfinity(transform.position.x) || float.IsInfinity(transform.position.y) || float.IsInfinity(transform.position.z))
            {
                Debug.LogError($"[RobotController] Invalid start position: {transform.position}. Stopping movement.");
                _currentState = RobotState.Idle;
                return;
            }

            _moveTarget = targetPosition;
            _moveStartTime = Time.time;
            _moveStartPosition = transform.position;

            var nextWaypoint = _pathfindingComponent.NextWaypoint.Value;
            Debug.Log($"[RobotController] Robot {Id} moving to waypoint {nextWaypoint} at world position {_moveTarget.Value}");
        }
        else
        {
            Debug.LogWarning($"[RobotController] No valid waypoint found for Robot {Id}.");
            _currentState = RobotState.Idle;
        }
    }
    
    /// <summary>
    /// Actualiza el movimiento hacia el waypoint actual.
    /// </summary>
    private void UpdateMoveToWaypoint()
    {
        if (!_moveTarget.HasValue) return;

        float distance = Vector3.Distance(_moveStartPosition, _moveTarget.Value);
        
        // Si ya estamos en la posición objetivo (distancia muy pequeña), completar inmediatamente
        if (distance < 0.01f)
        {
            Debug.Log($"[RobotController] Robot {Id} already at target position. Completing waypoint.");
            transform.position = _moveTarget.Value;
            OnWaypointReached();
            return;
        }
        
        // Validar que la distancia no sea NaN o infinita
        if (float.IsNaN(distance) || float.IsInfinity(distance))
        {
            Debug.LogError($"[RobotController] Invalid distance calculation: {distance}. StartPos: {_moveStartPosition}, Target: {_moveTarget.Value}");
            _currentState = RobotState.Idle;
            return;
        }
        
        float moveTime = distance / _moveSpeed;
        float elapsed = Time.time - _moveStartTime;
        float t = Mathf.Clamp01(elapsed / moveTime);

        // Validar que los valores de interpolación sean válidos
        if (float.IsNaN(t) || float.IsInfinity(t))
        {
            Debug.LogError($"[RobotController] Invalid interpolation value t: {t}. Distance: {distance}, MoveTime: {moveTime}, Elapsed: {elapsed}");
            _currentState = RobotState.Idle;
            return;
        }

        // Interpolar posición con validación adicional
        Vector3 newPosition = Vector3.Lerp(_moveStartPosition, _moveTarget.Value, t);
        
        // Validar que la nueva posición sea válida antes de asignarla
        if (float.IsNaN(newPosition.x) || float.IsNaN(newPosition.y) || float.IsNaN(newPosition.z) ||
            float.IsInfinity(newPosition.x) || float.IsInfinity(newPosition.y) || float.IsInfinity(newPosition.z))
        {
            Debug.LogError($"[RobotController] Invalid interpolated position: {newPosition}. t: {t}, StartPos: {_moveStartPosition}, Target: {_moveTarget.Value}");
            _currentState = RobotState.Idle;
            return;
        }
        
        transform.position = newPosition;

        // ¿Llegamos al waypoint?
        if (t >= 1.0f)
        {
            transform.position = _moveTarget.Value;
            OnWaypointReached();
        }
    }
    
    /// <summary>
    /// Llamado cuando el robot llega a un waypoint.
    /// </summary>
    private void OnWaypointReached()
    {
        if (_pathfindingComponent == null) return;
        
        // Actualizar posición en el grid
        var oldCell = _currentCell;
        _currentCell = _gridService.WorldToCell(transform.position);
        
        // Alinear la posición final al centro de la celda
        Vector3 alignedPosition = _gridService.CellToWorld(_currentCell);
        transform.position = alignedPosition;
        
        // Actualizar ocupación del grid
        _gridService.RemoveOccupant(oldCell, CellOccupant.Robot);
        _gridService.AddOccupant(_currentCell, CellOccupant.Robot);
        
        // Resetear contador anti-loop al completar movimiento exitosamente
        _sameTargetAttempts = 0;
        _lastTargetCell = Vector2Int.one * -1; // Reset to invalid position
        
        // Registrar paso en métricas
        if (_metricsLogger != null)
        {
            _metricsLogger.RecordRobotStep(Id);
        }
        
        Debug.Log($"[RobotController] Robot {Id} reached waypoint at cell {_currentCell} (world pos: {alignedPosition})");
        
        // Avanzar al siguiente waypoint
        if (_pathfindingComponent.AdvanceToNextWaypoint())
        {
            // Hay más waypoints, continuar movimiento
            _moveTarget = null; // Se establecerá en el próximo frame
        }
        else
        {
            // Path completado
            _moveTarget = null;
            
            // Verificar si llegamos a una joya que debemos recoger
            if (_currentState == RobotState.MovingToJewel)
            {
                if (!IsCarryingJewel)
                {
                    Debug.Log($"[RobotController] Robot {Id} arrived at jewel location. Attempting to pick up jewel.");
                    if (TryPickupJewel())
                    {
                        _currentState = RobotState.Idle; // Será cambiado a MovingToZone en el próximo tick
                    }
                    else
                    {
                        Debug.LogWarning($"[RobotController] Robot {Id} failed to pick up jewel at {_currentCell}");
                        _currentState = RobotState.Idle;
                    }
                }
                else
                {
                    Debug.LogWarning($"[RobotController] Robot {Id} arrived at jewel location but already carrying one.");
                    _currentState = RobotState.Idle;
                }
            }
            else if (_currentState == RobotState.MovingToZone)
            {
                Debug.Log($"[RobotController] Robot {Id} arrived at zone location. Attempting to drop jewel.");
                if (TryDropJewel())
                {
                    _currentState = RobotState.Idle;
                }
                else
                {
                    Debug.LogWarning($"[RobotController] Robot {Id} failed to drop jewel at {_currentCell}");
                    _currentState = RobotState.Idle;
                }
            }
            else
            {
                _currentState = RobotState.Idle;
            }
            
            Debug.Log($"[RobotController] Robot {Id} completed movement to {_currentCell}");
        }
    }
    
    /// <summary>
    /// Simula recoger una joya en la celda actual.
    /// </summary>
    /// <returns>True si se pudo recoger una joya</returns>
    public bool TryPickupJewel()
    {
        if (_carriedJewel != null)
        {
            Debug.LogWarning($"[RobotController] Robot {Id} is already carrying a jewel");
            return false;
        }
        
        // Buscar joya en la celda actual
        var jewelsInCell = Object.FindObjectsByType<Jewel>(FindObjectsSortMode.None);
        Jewel targetJewel = null;
        
        foreach (var jewel in jewelsInCell)
        {
            if (jewel.Cell == _currentCell && jewel.IsAvailable)
            {
                targetJewel = jewel;
                break;
            }
        }
        
        if (targetJewel == null)
        {
            Debug.LogWarning($"[RobotController] No available jewel found at cell {_currentCell}");
            return false;
        }
        
        // Validar con RuleSystem
        if (_ruleSystem != null && !_ruleSystem.ValidatePickup(Id, targetJewel.Color))
        {
            Debug.LogWarning($"[RobotController] Pickup not allowed by RuleSystem");
            return false;
        }
        
        // Recoger la joya
        if (targetJewel.TryPickUp())
        {
            _carriedJewel = targetJewel;
            Debug.Log($"[RobotController] Robot {Id} picked up {targetJewel.Color} jewel at {_currentCell}");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Simula entregar la joya cargada a una zona en la celda actual.
    /// </summary>
    /// <returns>True si se pudo entregar la joya</returns>
    public bool TryDropJewel()
    {
        if (_carriedJewel == null)
        {
            Debug.LogWarning($"[RobotController] Robot {Id} is not carrying any jewel");
            return false;
        }
        
        Debug.Log($"[RobotController] Robot {Id} attempting to drop {_carriedJewel.Color} jewel at cell {_currentCell}");
        
        // Buscar zona en la celda actual
        var zonesInCell = Object.FindObjectsByType<ZoneController>(FindObjectsSortMode.None);
        ZoneController targetZone = null;
        
        Debug.Log($"[RobotController] Found {zonesInCell.Length} zones in scene");
        
        foreach (var zone in zonesInCell)
        {
            Debug.Log($"[RobotController] Checking zone at cell {zone.Cell} (accepts {zone.AcceptedColor}), robot at {_currentCell}");
            if (zone.Cell == _currentCell)
            {
                targetZone = zone;
                Debug.Log($"[RobotController] Found matching zone at {_currentCell} that accepts {zone.AcceptedColor}");
                break;
            }
        }
        
        if (targetZone == null)
        {
            Debug.LogWarning($"[RobotController] No zone found at cell {_currentCell}");
            return false;
        }
        
        // Validar con RuleSystem
        if (_ruleSystem != null && !_ruleSystem.ValidateDelivery(Id, _carriedJewel.Color, targetZone.AcceptedColor))
        {
            Debug.LogWarning($"[RobotController] Delivery not allowed by RuleSystem. Jewel: {_carriedJewel.Color}, Zone accepts: {targetZone.AcceptedColor}");
            return false;
        }
        
        // Intentar entregar la joya
        Debug.Log($"[RobotController] Attempting delivery: {_carriedJewel.Color} jewel to {targetZone.AcceptedColor} zone");
        if (targetZone.TryDeliverJewel(_carriedJewel.Color, _carriedJewel.Value))
        {
            Debug.Log($"[RobotController] Robot {Id} successfully delivered {_carriedJewel.Color} jewel to zone at {_currentCell}");
            _carriedJewel = null;
            return true;
        }
        else
        {
            Debug.LogWarning($"[RobotController] Zone rejected the jewel delivery. Jewel: {_carriedJewel.Color}, Zone accepts: {targetZone.AcceptedColor}");
            return false;
        }
    }
    
    /// <summary>
    /// Verifica si el robot está intentando moverse repetidamente al mismo lugar y maneja el loop.
    /// </summary>
    /// <param name="targetCell">Celda objetivo a la que se quiere mover</param>
    /// <returns>True si se detectó un loop y se manejó, False si puede continuar normalmente</returns>
    private bool CheckAndHandleLoop(Vector2Int targetCell)
    {
        // Si es el mismo objetivo que la vez anterior
        if (targetCell == _lastTargetCell)
        {
            _sameTargetAttempts++;
            Debug.Log($"[RobotController] Robot {Id} attempting same target {targetCell} for the {_sameTargetAttempts} time");
            
            // Si ha intentado demasiadas veces
            if (_sameTargetAttempts >= MAX_SAME_TARGET_ATTEMPTS)
            {
                Debug.LogWarning($"[RobotController] Robot {Id} stuck in loop trying to reach {targetCell}. Moving to random position.");
                MoveToRandomPosition();
                return true; // Loop detected and handled
            }
        }
        else
        {
            // Nuevo objetivo, resetear contador
            _sameTargetAttempts = 1;
            _lastTargetCell = targetCell;
        }
        
        return false; // No loop, continuar normalmente
    }
    
    /// <summary>
    /// Mueve el robot a una posición aleatoria válida para romper loops.
    /// </summary>
    private void MoveToRandomPosition()
    {
        // Resetear el sistema anti-loop
        _sameTargetAttempts = 0;
        _lastTargetCell = Vector2Int.one * -1; // Invalid position
        
        // Buscar una celda aleatoria válida
        var randomCell = FindRandomWalkableCell();
        if (randomCell.HasValue)
        {
            Debug.Log($"[RobotController] Robot {Id} moving to random position {randomCell.Value} to break loop");
            if (MoveToCell(randomCell.Value))
            {
                _currentState = RobotState.MovingToTarget; // Movimiento genérico
            }
            else
            {
                Debug.LogWarning($"[RobotController] Robot {Id} couldn't move to random position {randomCell.Value}");
                _currentState = RobotState.Idle;
            }
        }
        else
        {
            Debug.LogWarning($"[RobotController] Robot {Id} couldn't find any valid random position");
            _currentState = RobotState.Idle;
        }
    }
    
    /// <summary>
    /// Encuentra una celda aleatoria transitable en el grid.
    /// </summary>
    /// <returns>Una celda válida aleatoria o null si no encuentra ninguna</returns>
    private Vector2Int? FindRandomWalkableCell()
    {
        var availableCells = new System.Collections.Generic.List<Vector2Int>();
        
        // Recopilar todas las celdas walkable
        for (int x = 0; x < _gridService.Width; x++)
        {
            for (int y = 0; y < _gridService.Height; y++)
            {
                var cell = new Vector2Int(x, y);
                if (_gridService.IsWalkable(cell) && cell != _currentCell)
                {
                    availableCells.Add(cell);
                }
            }
        }
        
        // Seleccionar una aleatoria
        if (availableCells.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, availableCells.Count);
            return availableCells[randomIndex];
        }
        
        return null;
    }
    
    /// <summary>
    /// Encuentra la joya disponible más cercana al robot.
    /// </summary>
    /// <returns>La joya más cercana o null si no hay ninguna disponible</returns>
    private Jewel FindNearestAvailableJewel()
    {
        var allJewels = Object.FindObjectsByType<Jewel>(FindObjectsSortMode.None);
        Jewel nearestJewel = null;
        float nearestDistance = float.MaxValue;
        
        foreach (var jewel in allJewels)
        {
            if (!jewel.IsAvailable) continue;
            
            // Calcular distancia Manhattan (más apropiada para grid)
            float distance = Vector2Int.Distance(_currentCell, jewel.Cell);
            
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestJewel = jewel;
            }
        }
        
        return nearestJewel;
    }
    
    /// <summary>
    /// Encuentra la zona más cercana que acepta el color de joya especificado.
    /// </summary>
    /// <param name="jewelColor">Color de la joya a entregar</param>
    /// <returns>La zona compatible más cercana o null si no hay ninguna</returns>
    private ZoneController FindNearestCompatibleZone(JewelColor jewelColor)
    {
        var allZones = Object.FindObjectsByType<ZoneController>(FindObjectsSortMode.None);
        ZoneController nearestZone = null;
        float nearestDistance = float.MaxValue;
        
        Debug.Log($"[RobotController] Searching for zone that accepts {jewelColor}. Found {allZones.Length} zones total.");
        
        foreach (var zone in allZones)
        {
            Debug.Log($"[RobotController] Zone at {zone.Cell} accepts {zone.AcceptedColor}");
            
            // Verificar si la zona acepta este color de joya
            if (zone.AcceptedColor != jewelColor) 
            {
                Debug.Log($"[RobotController] Zone at {zone.Cell} doesn't accept {jewelColor} (accepts {zone.AcceptedColor})");
                continue;
            }
            
            // Calcular distancia Manhattan
            float distance = Vector2Int.Distance(_currentCell, zone.Cell);
            Debug.Log($"[RobotController] Compatible zone found at {zone.Cell}, distance: {distance}");
            
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestZone = zone;
                Debug.Log($"[RobotController] New nearest compatible zone: {zone.Cell}");
            }
        }
        
        if (nearestZone != null)
        {
            Debug.Log($"[RobotController] Found nearest compatible zone at {nearestZone.Cell} for {jewelColor} jewel");
        }
        else
        {
            Debug.LogWarning($"[RobotController] No compatible zone found for {jewelColor} jewel");
        }
        
        return nearestZone;
    }
    
    /// <summary>
    /// Llamado cuando el objeto es destruido.
    /// </summary>
    private void OnDestroy()
    {
        // Limpiar ocupación del grid
        if (_gridService != null)
        {
            _gridService.RemoveOccupant(_currentCell, CellOccupant.Robot);
        }
    }
}