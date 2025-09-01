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
    private Vector2Int _currentCell;
    private Vector2Int _finalGoal; // Store final destination for replanning
    private RobotState _currentState = RobotState.Idle;
    private Vector3? _moveTarget;
    private float _moveStartTime;
    private Vector3 _moveStartPosition;
    
    /// <summary>Estados básicos del robot para el movimiento</summary>
    private enum RobotState
    {
        Idle,
        MovingToTarget
    }
    
    /// <summary>ID único del robot</summary>
    public int Id => _robotId;
    
    /// <summary>Posición actual del robot en el grid</summary>
    public Vector2Int Cell => _currentCell;

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
        
        // Inicializar posición en el grid
        _currentCell = _gridService.WorldToCell(transform.position);
        
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
        
        if (_currentState == RobotState.MovingToTarget)
        {
            Debug.LogWarning($"[RobotController] Robot {Id} is already moving");
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
    /// Tick del agente - ejecuta lógica de movimiento y percepción.
    /// </summary>
    public void Tick()
    {
        if (_gridService == null) return;
        
        switch (_currentState)
        {
            case RobotState.Idle:
                // En idle no hay acción automática
                break;
                
            case RobotState.MovingToTarget:
                UpdateMovement();
                break;
        }
    }
    
    /// <summary>
    /// Planifica la siguiente acción cuando no tiene plan activo.
    /// Implementación mínima - por ahora no hace nada.
    /// </summary>
    public void PlanNextAction()
    {
        // Implementación mínima - el robot solo responde a comandos externos
        // En el futuro aquí iría la lógica de exploración, búsqueda de joyas, etc.
    }
    
    /// <summary>
    /// Actualiza el movimiento del robot siguiendo el path.
    /// </summary>
    private void UpdateMovement()
    {
        if (_pathfindingComponent == null || !_pathfindingComponent.HasPath)
        {
            // No hay path, cambiar a idle
            _currentState = RobotState.Idle;
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
    /// Inicia el movimiento hacia el siguiente waypoint.
    /// </summary>
    private void StartMoveToNextWaypoint()
    {
        // Verificar y replanear si es necesario (feature opcional)
        if (!_pathfindingComponent.ValidateAndReplanIfNeeded(transform.position, _finalGoal))
        {
            Debug.LogWarning($"[RobotController] Robot {Id} could not validate or replan path. Stopping movement.");
            _currentState = RobotState.Idle;
            return;
        }

        var nextWaypointWorld = _pathfindingComponent.GetNextWaypointWorldPosition();
        if (nextWaypointWorld.HasValue)
        {
            _moveTarget = nextWaypointWorld.Value;
            _moveStartTime = Time.time;
            _moveStartPosition = transform.position;
            
            var nextWaypoint = _pathfindingComponent.NextWaypoint.Value;
            Debug.Log($"[RobotController] Robot {Id} moving to waypoint {nextWaypoint}");
        }
    }
    
    /// <summary>
    /// Actualiza el movimiento hacia el waypoint actual.
    /// </summary>
    private void UpdateMoveToWaypoint()
    {
        if (!_moveTarget.HasValue) return;
        
        float distance = Vector3.Distance(_moveStartPosition, _moveTarget.Value);
        float moveTime = distance / _moveSpeed;
        float elapsed = Time.time - _moveStartTime;
        float t = Mathf.Clamp01(elapsed / moveTime);
        
        // Interpolar posición
        transform.position = Vector3.Lerp(_moveStartPosition, _moveTarget.Value, t);
        
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
        
        // Actualizar ocupación del grid
        _gridService.RemoveOccupant(oldCell, CellOccupant.Robot);
        _gridService.AddOccupant(_currentCell, CellOccupant.Robot);
        
        Debug.Log($"[RobotController] Robot {Id} reached waypoint at cell {_currentCell}");
        
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
            _currentState = RobotState.Idle;
            Debug.Log($"[RobotController] Robot {Id} completed movement to {_currentCell}");
        }
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