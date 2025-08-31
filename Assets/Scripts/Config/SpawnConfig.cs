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

    [Header("Structural Elements")]
    [SerializeField] private bool _enableWallPerimeter = false;
    [SerializeField] private bool _enableShelves = false;
    [SerializeField] private int _shelfRows = 0;
    [SerializeField] private int _shelfColumns = 0;
    [SerializeField] private bool _enableZones = false;
    [SerializeField] private int _numZones = 1;

    /// <summary>Número de robots a generar</summary>
    public int NumRobots => _numRobots;

    /// <summary>Número de joyas a generar</summary>
    public int NumJewels => _numJewels;

    /// <summary>Colores disponibles para las joyas</summary>
    public string[] JewelColors => _jewelColors;

    /// <summary>Densidad de obstáculos (0.0-1.0)</summary>
    public float ObstacleDensity => _obstacleDensity;

    /// <summary>Indica si se debe crear un perímetro de paredes</summary>
    public bool EnableWallPerimeter => _enableWallPerimeter;

    /// <summary>Indica si se deben crear estantes</summary>
    public bool EnableShelves => _enableShelves;

    /// <summary>Número de filas de estantes a crear</summary>
    public int ShelfRows => _shelfRows;

    /// <summary>Número de columnas de estantes a crear</summary>
    public int ShelfColumns => _shelfColumns;

    /// <summary>Indica si se deben crear zonas</summary>
    public bool EnableZones => _enableZones;

    /// <summary>Número de zonas a crear</summary>
    public int NumZones => _numZones;
}