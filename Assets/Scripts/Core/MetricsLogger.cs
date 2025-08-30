using UnityEngine;

/// <summary>
/// Servicio/MB que acumula KPIs de la simulación (movimientos, tiempo por acción,
/// joyas recogidas/entregadas, eficiencia) y los expone a UI o exporta.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Recibe eventos desde <see cref="RobotController"/> y <see cref="ZoneController"/>.
/// - Consultado por UI y por <see cref="SimulationManager"/> para fin de simulación.
/// </remarks>
public sealed class MetricsLogger : MonoBehaviour
{
        
}