using UnityEngine;

/// <summary>
/// Punto de arranque de la escena. Crea/inyecta servicios (GridService, PathfindingService, etc.),
/// carga configs y expone referencias compartidas.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Inicializa <see cref="SimulationManager"/> y registra sistemas globales.
/// - Provee <see cref="PathfindingService"/> a <see cref="PathfindingComponent"/>.
/// - Usa <see cref="ValidationService"/> para sanity checks.
/// </remarks>
public sealed class GameBootstrap : MonoBehaviour
{
        
}
