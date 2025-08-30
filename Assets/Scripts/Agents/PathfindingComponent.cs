using UnityEngine;

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
    
}