using UnityEngine;

/// <summary>
/// Orquestador principal de estados de la simulación (Init/Running/Paused/Finished),
/// controla el loop de ticks y los reseteos/replays.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Coordina <see cref="SpawnSystem"/>, <see cref="RuleSystem"/> y notifica a UI.
/// - Itera <see cref="IAgent.Tick"/> y decide cuándo pedir <see cref="IAgent.PlanNextAction"/>.
/// </remarks>
public sealed class SimulationManager : MonoBehaviour
{
        
    }