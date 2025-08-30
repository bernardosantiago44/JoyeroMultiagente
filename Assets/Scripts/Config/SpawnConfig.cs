using UnityEngine;

[CreateAssetMenu(fileName = "SpawnConfig", menuName = "Scriptable Objects/SpawnConfig")]
/// <summary>
/// Reglas y cantidades de spawning (joyas, paredes, estantes, zonas).
/// Permite definir restricciones y probabilidades por tipo.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Consumida por <see cref="SpawnSystem"/> / <see cref="GridSpawner"/>.
/// - Puede ser validada por <see cref="ValidationService"/>.
/// </remarks>
public sealed class SpawnConfig : ScriptableObject
{
        
}