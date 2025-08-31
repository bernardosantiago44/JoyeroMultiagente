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
    [Header("Agent Spawn Settings")]
    [SerializeField] private int _numRobots = 3;

    [Header("Jewel Spawn Settings")]
    [SerializeField] private int _numJewels = 10;
    [SerializeField] private string[] _jewelColors = { "Red", "Blue", "Green", "Yellow" };

    [Header("Environment Settings")]
    [SerializeField] private float _obstacleDensity = 0.2f;

    /// <summary>Número de robots a generar</summary>
    public int NumRobots => _numRobots;

    /// <summary>Número de joyas a generar</summary>
    public int NumJewels => _numJewels;

    /// <summary>Colores disponibles para las joyas</summary>
    public string[] JewelColors => _jewelColors;

    /// <summary>Densidad de obstáculos (0.0-1.0)</summary>
    public float ObstacleDensity => _obstacleDensity;
}