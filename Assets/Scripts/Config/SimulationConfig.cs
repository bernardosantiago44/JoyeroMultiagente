using UnityEngine;

[CreateAssetMenu(fileName = "SimulationConfig", menuName = "Scriptable Objects/SimulationConfig")]
/// <summary>
/// Parámetros globales de la simulación (tamaño de grid, número de robots,
/// densidad de joyas, seed, flags de reglas).
/// </summary>
/// <remarks>
/// Interacciones:
/// - Leída por <see cref="GameBootstrap"/> para inicializar servicios y sistemas.
/// - Referenciada por <see cref="RuleSystem"/> y <see cref="SpawnSystem"/>.
/// </remarks>
public sealed class SimulationConfig : ScriptableObject
{
    
}