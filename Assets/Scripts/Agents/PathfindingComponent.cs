using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Componente por-agente que solicita y consume rutas del <see cref="PathfindingService"/>.
/// Mantiene el estado local del camino (waypoints, índice actual) y la lógica de replan.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Convierte posiciones entre mundo y celdas via <see cref="GridService"/>.
/// - Pide rutas al <see cref="PathfindingService"/> provisto por <see cref="GameBootstrap"/>.
/// - Notifica a <see cref="RobotController"/> cuando completa o falla una ruta.
/// </remarks>
public sealed class PathfindingComponent : MonoBehaviour
{
    private PathfindingService _pathfindingService;
    private GridService _gridService;
    private List<Vector2Int> _currentPath = new List<Vector2Int>();
    private int _currentWaypointIndex = 0;
    
    /// <summary>Indica si hay un camino activo</summary>
    public bool HasPath => _currentPath.Count > 0 && _currentWaypointIndex < _currentPath.Count;
    
    /// <summary>Siguiente waypoint en el camino, o null si no hay camino</summary>
    public Vector2Int? NextWaypoint => HasPath ? _currentPath[_currentWaypointIndex] : null;

    private void Start()
    {
        // Obtener servicios del registro
        if (!ServiceRegistry.TryResolve<PathfindingService>(out _pathfindingService))
        {
            Debug.LogError("[PathfindingComponent] PathfindingService not found in ServiceRegistry.");
        }
        
        if (!ServiceRegistry.TryResolve<GridService>(out _gridService))
        {
            Debug.LogError("[PathfindingComponent] GridService not found in ServiceRegistry.");
        }
    }

    /// <summary>
    /// Solicita un nuevo camino desde la posición actual hasta el objetivo.
    /// </summary>
    /// <param name="targetWorldPos">Posición objetivo en coordenadas del mundo</param>
    /// <returns>true si se encontró un camino válido</returns>
    public bool RequestPathToWorldPosition(Vector3 targetWorldPos)
    {
        if (_pathfindingService == null || _gridService == null)
        {
            Debug.LogWarning("[PathfindingComponent] Services not available for pathfinding.");
            return false;
        }

        // Convertir posiciones del mundo a celdas del grid
        var startCell = _gridService.WorldToCell(transform.position);
        var goalCell = _gridService.WorldToCell(targetWorldPos);

        return RequestPathToCellPosition(startCell, goalCell);
    }

    /// <summary>
    /// Solicita un nuevo camino entre dos celdas del grid.
    /// </summary>
    /// <param name="startCell">Celda de inicio</param>
    /// <param name="goalCell">Celda de destino</param>
    /// <returns>true si se encontró un camino válido</returns>
    public bool RequestPathToCellPosition(Vector2Int startCell, Vector2Int goalCell)
    {
        if (_pathfindingService == null)
        {
            Debug.LogWarning("[PathfindingComponent] PathfindingService not available.");
            return false;
        }

        // Limpiar camino anterior
        ClearPath();

        // Solicitar nuevo camino
        if (_pathfindingService.TryFindPath(startCell, goalCell, out var newPath))
        {
            _currentPath = newPath;
            _currentWaypointIndex = 0;
            
            Debug.Log($"[PathfindingComponent] Path found with {newPath.Count} waypoints.");
            return true;
        }
        else
        {
            Debug.LogWarning($"[PathfindingComponent] No path found from {startCell} to {goalCell}.");
            return false;
        }
    }

    /// <summary>
    /// Avanza al siguiente waypoint en el camino.
    /// </summary>
    /// <returns>true si aún hay waypoints restantes</returns>
    public bool AdvanceToNextWaypoint()
    {
        if (!HasPath)
            return false;

        _currentWaypointIndex++;
        return HasPath;
    }

    /// <summary>
    /// Limpia el camino actual.
    /// </summary>
    public void ClearPath()
    {
        _currentPath.Clear();
        _currentWaypointIndex = 0;
    }

    /// <summary>
    /// Obtiene la posición del mundo del siguiente waypoint.
    /// </summary>
    /// <returns>Posición del mundo, o null si no hay waypoint</returns>
    public Vector3? GetNextWaypointWorldPosition()
    {
        if (!HasPath || _gridService == null)
            return null;

        return _gridService.CellToWorld(_currentPath[_currentWaypointIndex]);
    }

    /// <summary>
    /// Verifica si el siguiente waypoint sigue siendo walkable y replana si es necesario.
    /// </summary>
    /// <param name="currentPosition">Posición actual del agente en el mundo</param>
    /// <param name="finalGoal">Objetivo final del movimiento</param>
    /// <returns>true si el path es válido o se pudo replanear</returns>
    public bool ValidateAndReplanIfNeeded(Vector3 currentPosition, Vector2Int finalGoal)
    {
        if (!HasPath || _gridService == null || _pathfindingService == null)
            return false;

        var nextWaypoint = NextWaypoint;
        if (!nextWaypoint.HasValue)
            return false;

        // Verificar si el siguiente waypoint sigue siendo walkable
        if (!_gridService.IsWalkable(nextWaypoint.Value))
        {
            Debug.LogWarning($"[PathfindingComponent] Next waypoint {nextWaypoint.Value} is no longer walkable. Replanning...");
            
            // Intentar replanear desde la posición actual
            var currentCell = _gridService.WorldToCell(currentPosition);
            return RequestPathToCellPosition(currentCell, finalGoal);
        }

        return true; // Path válido
    }
}