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
    
    [Header("Jewel Color Distribution")]
    [SerializeField] private int _numRedJewels = 5;
    [SerializeField] private int _numBlueJewels = 2;
    [SerializeField] private int _numGreenJewels = 2;
    [SerializeField] private int _numYellowJewels = 1;

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

    /// <summary>Número específico de joyas rojas a generar</summary>
    public int NumRedJewels => _numRedJewels;

    /// <summary>Número específico de joyas azules a generar</summary>
    public int NumBlueJewels => _numBlueJewels;

    /// <summary>Número específico de joyas verdes a generar</summary>
    public int NumGreenJewels => _numGreenJewels;

    /// <summary>Número específico de joyas amarillas a generar</summary>
    public int NumYellowJewels => _numYellowJewels;

    /// <summary>Número total de joyas calculado desde la distribución por colores</summary>
    public int TotalJewelsFromDistribution => _numRedJewels + _numBlueJewels + _numGreenJewels + _numYellowJewels;

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

    /// <summary>
    /// Valida la configuración de joyas y ajusta el total si es necesario.
    /// </summary>
    private void OnValidate()
    {
        // Asegurar que los valores sean no negativos
        _numRedJewels = Mathf.Max(0, _numRedJewels);
        _numBlueJewels = Mathf.Max(0, _numBlueJewels);
        _numGreenJewels = Mathf.Max(0, _numGreenJewels);
        _numYellowJewels = Mathf.Max(0, _numYellowJewels);

        // Sincronizar el total con la distribución por colores
        int calculatedTotal = _numRedJewels + _numBlueJewels + _numGreenJewels + _numYellowJewels;
        if (_numJewels != calculatedTotal)
        {
            _numJewels = calculatedTotal;
        }
    }
}