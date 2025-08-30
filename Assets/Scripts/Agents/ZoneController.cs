using UnityEngine;

/// <summary>
/// Define una zona de entrega/objetivo del mapa donde los robots depositan joyas.
/// Puede validar drops, sumar puntaje y notificar eventos de entrega.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Consultada por <see cref="GridService"/> para ocupación y tipo de celda.
/// - Recibe eventos desde <see cref="RobotController"/> al hacer "Drop".
/// - Puede informar a <see cref="MetricsLogger"/> y activar condiciones de fin en <see cref="RuleSystem"/>.
/// </remarks>
public sealed class ZoneController : MonoBehaviour
{
    
}