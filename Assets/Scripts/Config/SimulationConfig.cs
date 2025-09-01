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
    [Header("Grid Settings")]
    [SerializeField] private int _width = 20;
    [SerializeField] private int _height = 15;
    [SerializeField] private float _cellSize = 1.0f;

    [Header("Simulation Settings")]
    [SerializeField] private float _maxTime = 300.0f; // 5 minutes default

    [Header("Validation Thresholds")]
    [SerializeField] private float _obstacleDensityMax = 0.40f;
    [SerializeField] private int _minWidth = 2;
    [SerializeField] private int _minHeight = 2;

    /// <summary>Ancho del mapa en celdas</summary>
    public int Width => _width;

    /// <summary>Alto del mapa en celdas</summary>
    public int Height => _height;

    /// <summary>Tamaño de cada celda en unidades del mundo</summary>
    public float CellSize => _cellSize;

    /// <summary>Tiempo máximo de simulación en segundos</summary>
    public float MaxTime => _maxTime;

    /// <summary>Densidad máxima recomendada de obstáculos (0.0-1.0)</summary>
    public float ObstacleDensityMax => _obstacleDensityMax;

    /// <summary>Ancho mínimo permitido del mapa</summary>
    public int MinWidth => _minWidth;

    /// <summary>Alto mínimo permitido del mapa</summary>
    public int MinHeight => _minHeight;

    public GridMap GridMap => new GridMap(_width, _height);
}