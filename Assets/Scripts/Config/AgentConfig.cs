using UnityEngine;

[CreateAssetMenu(fileName = "AgentConfig", menuName = "Scriptable Objects/AgentConfig")]
/// <summary>
/// Configuración por agente (velocidad, capacidad, FOV, tiempos de acción).
/// Editable en el inspector como ScriptableObject para tuning sin recompilar.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Consumida por <see cref="RobotController"/> y componentes de navegación/percepción.
/// - Cargada por <see cref="GameBootstrap"/>.
/// </remarks>
public sealed class AgentConfig : ScriptableObject
{
    
}