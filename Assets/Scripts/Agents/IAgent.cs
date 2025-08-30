using UnityEngine;

/// <summary>
/// Contrato mínimo para cualquier agente controlado por la simulación.
/// Se espera que represente un robot u otro actor con posición en celda,
/// capaz de percibir y planear acciones en cada tick/turno.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Usa <see cref="GridService"/> para conocer el mundo lógico.
/// - Pide rutas vía <see cref="PathfindingComponent"/> / <see cref="PathfindingService"/>.
/// - Reporta métricas a <see cref="MetricsLogger"/> y sigue reglas de <see cref="RuleSystem"/>.
/// - Es coordinado por <see cref="SimulationManager"/>.
/// </remarks>
public interface IAgent
{
    int Id { get; }
    Vector2Int Cell { get; }
    void Tick();           // Percepción + avance del plan corto.
    void PlanNextAction(); // Decide la siguiente acción cuando no tiene plan.
}
