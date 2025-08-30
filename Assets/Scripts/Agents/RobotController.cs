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
    public int Id => 0;
    public Vector2Int Cell => default;
    public void Tick() { }
    public void PlanNextAction() { }
}