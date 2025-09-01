using UnityEngine;

/// <summary>
/// Demo script que muestra cómo usar el RobotController para movimiento básico.
/// Coloca este script en una escena con un robot configurado para ver el movimiento en acción.
/// </summary>
public class RobotMovementDemo : MonoBehaviour
{
    [Header("Demo Configuration")]
    [SerializeField] private RobotController _robot;
    [SerializeField] private Vector2Int _targetCell = new Vector2Int(5, 5);
    [SerializeField] private bool _autoStart = true;
    [SerializeField] private float _moveDelay = 2.0f; // Delay before starting movement

    private void Start()
    {
        if (_autoStart && _robot != null)
        {
            Invoke(nameof(StartMovementDemo), _moveDelay);
        }
    }

    /// <summary>
    /// Inicia la demostración de movimiento del robot.
    /// </summary>
    public void StartMovementDemo()
    {
        if (_robot == null)
        {
            Debug.LogError("[RobotMovementDemo] No robot assigned.");
            return;
        }

        Debug.Log($"[RobotMovementDemo] Starting movement demo from {_robot.Cell} to {_targetCell}");
        
        bool success = _robot.MoveToCell(_targetCell);
        if (success)
        {
            Debug.Log($"[RobotMovementDemo] Movement command sent successfully.");
        }
        else
        {
            Debug.LogWarning($"[RobotMovementDemo] Failed to start movement.");
        }
    }

    /// <summary>
    /// Mueve el robot a una nueva posición aleatoria.
    /// </summary>
    [ContextMenu("Move to Random Position")]
    public void MoveToRandomPosition()
    {
        if (_robot == null) return;

        // Get grid service to find valid positions
        if (!ServiceRegistry.TryResolve<GridService>(out var gridService))
        {
            Debug.LogError("[RobotMovementDemo] GridService not available.");
            return;
        }

        // Try to find a random walkable position
        int attempts = 0;
        Vector2Int randomTarget;
        
        do
        {
            randomTarget = new Vector2Int(
                Random.Range(0, gridService.Width),
                Random.Range(0, gridService.Height)
            );
            attempts++;
        }
        while (!gridService.IsWalkable(randomTarget) && attempts < 20);

        if (attempts >= 20)
        {
            Debug.LogWarning("[RobotMovementDemo] Could not find walkable random position.");
            return;
        }

        Debug.Log($"[RobotMovementDemo] Moving robot to random position {randomTarget}");
        _robot.MoveToCell(randomTarget);
    }

    /// <summary>
    /// Para el movimiento actual del robot.
    /// </summary>
    [ContextMenu("Stop Movement")]
    public void StopMovement()
    {
        if (_robot == null) return;

        // Access PathfindingComponent and clear path
        var pathfindingComponent = _robot.GetComponent<PathfindingComponent>();
        if (pathfindingComponent != null)
        {
            pathfindingComponent.ClearPath();
            Debug.Log("[RobotMovementDemo] Movement stopped.");
        }
    }

    private void OnGUI()
    {
        if (_robot == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label("Robot Movement Demo", GUI.skin.box);
        
        GUILayout.Label($"Robot ID: {_robot.Id}");
        GUILayout.Label($"Current Cell: {_robot.Cell}");
        GUILayout.Label($"Target Cell: {_targetCell}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Start Movement Demo"))
        {
            StartMovementDemo();
        }
        
        if (GUILayout.Button("Move to Random Position"))
        {
            MoveToRandomPosition();
        }
        
        if (GUILayout.Button("Stop Movement"))
        {
            StopMovement();
        }

        GUILayout.EndArea();
    }
}