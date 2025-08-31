using UnityEngine;

/// <summary>
/// Demostración del GridRenderer con diferentes tipos de celdas.
/// Crear un GameObject con este script para probar la visualización del grid.
/// </summary>
/// <remarks>
/// Este script crea un GridService de ejemplo con diferentes tipos de celdas
/// para mostrar las capacidades de visualización del GridRenderer.
/// </remarks>
public sealed class GridRendererDemo : MonoBehaviour
{
    [Header("Demo Settings")]
    [SerializeField] private int _demoWidth = 10;
    [SerializeField] private int _demoHeight = 8;
    [SerializeField] private float _demoCellSize = 1.0f;
    [SerializeField] private bool _createGridService = true;
    [SerializeField] private bool _addSampleOccupants = true;

    private GridService _demoGridService;

    /// <summary>
    /// Crea un GridService de demostración y lo registra en el ServiceRegistry.
    /// </summary>
    private void Start()
    {
        if (_createGridService)
        {
            CreateDemoGridService();
        }
    }

    /// <summary>
    /// Limpia el ServiceRegistry al destruir el objeto.
    /// </summary>
    private void OnDestroy()
    {
        if (_demoGridService != null)
        {
            ServiceRegistry.Clear();
        }
    }

    /// <summary>
    /// Crea un GridService con datos de ejemplo para demostración.
    /// </summary>
    private void CreateDemoGridService()
    {
        // Crear GridMap
        var demoMap = new GridMap(_demoWidth, _demoHeight);
        
        // Llenar con patrones de ejemplo
        FillDemoPattern(demoMap);
        
        // Crear GridService
        Vector3 origin = transform.position;
        _demoGridService = new GridService(demoMap, origin, _demoCellSize);
        
        // Registrar en ServiceRegistry
        ServiceRegistry.Register<GridService>(_demoGridService);
        
        // Agregar ocupantes de ejemplo si está habilitado
        if (_addSampleOccupants)
        {
            AddSampleOccupants();
        }
        
        Debug.Log($"[GridRendererDemo] Created demo GridService ({_demoWidth}x{_demoHeight}) at {origin}");
    }

    /// <summary>
    /// Llena el mapa con un patrón de demostración que incluye todos los tipos de celdas.
    /// </summary>
    /// <param name="map">El GridMap a llenar</param>
    private void FillDemoPattern(GridMap map)
    {
        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                CellType cellType = GetDemoCellType(x, y, map.Width, map.Height);
                map.SetCell(x, y, new GridCell(cellType));
            }
        }
    }

    /// <summary>
    /// Determina el tipo de celda para crear un patrón de demostración interesante.
    /// </summary>
    /// <param name="x">Coordenada X</param>
    /// <param name="y">Coordenada Y</param>
    /// <param name="width">Ancho del mapa</param>
    /// <param name="height">Alto del mapa</param>
    /// <returns>Tipo de celda para la posición dada</returns>
    private CellType GetDemoCellType(int x, int y, int width, int height)
    {
        // Bordes como muros
        if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
        {
            return CellType.Wall;
        }
        
        // Esquinas interiores como spawn de robots
        if ((x == 1 && y == 1) || (x == width - 2 && y == 1) || 
            (x == 1 && y == height - 2) || (x == width - 2 && y == height - 2))
        {
            return CellType.RobotSpawn;
        }
        
        // Zonas en el centro de cada cuadrante
        int centerX = width / 2;
        int centerY = height / 2;
        
        if ((x == centerX / 2 && y == centerY / 2) || 
            (x == centerX + centerX / 2 && y == centerY / 2) ||
            (x == centerX / 2 && y == centerY + centerY / 2) || 
            (x == centerX + centerX / 2 && y == centerY + centerY / 2))
        {
            return CellType.Zone;
        }
        
        // Algunos estantes dispersos
        if ((x + y) % 5 == 0 && x > 1 && x < width - 2 && y > 1 && y < height - 2)
        {
            return CellType.Shelf;
        }
        
        // Algunas celdas de joyas
        if ((x * 2 + y) % 7 == 0 && x > 2 && x < width - 3 && y > 2 && y < height - 3)
        {
            return CellType.Jewel;
        }
        
        // El resto como celdas vacías
        return CellType.Empty;
    }

    /// <summary>
    /// Agrega ocupantes de ejemplo a algunas celdas para demostrar la visualización de ocupación.
    /// </summary>
    private void AddSampleOccupants()
    {
        if (_demoGridService == null) return;
        
        // Agregar algunos robots
        _demoGridService.AddOccupant(new Vector2Int(2, 2), CellOccupant.Robot);
        _demoGridService.AddOccupant(new Vector2Int(_demoWidth - 3, 2), CellOccupant.Robot);
        
        // Agregar algunas joyas
        _demoGridService.AddOccupant(new Vector2Int(3, 3), CellOccupant.Jewel);
        _demoGridService.AddOccupant(new Vector2Int(4, 4), CellOccupant.Jewel);
        _demoGridService.AddOccupant(new Vector2Int(5, 3), CellOccupant.Jewel);
        
        // Agregar una reserva
        _demoGridService.AddOccupant(new Vector2Int(6, 5), CellOccupant.Reserved);
        
        Debug.Log("[GridRendererDemo] Added sample occupants to demonstrate visualization");
    }

    /// <summary>
    /// Método para recrear el grid de demostración desde el Inspector.
    /// </summary>
    [ContextMenu("Recreate Demo Grid")]
    private void RecreateDemoGrid()
    {
        if (_demoGridService != null)
        {
            ServiceRegistry.Clear();
            _demoGridService = null;
        }
        
        if (_createGridService)
        {
            CreateDemoGridService();
        }
    }

    /// <summary>
    /// Dibuja información de debug en el SceneView.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Mostrar el área de demostración cuando está seleccionado
        if (_createGridService)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position + new Vector3(_demoWidth * _demoCellSize * 0.5f, 0, _demoHeight * _demoCellSize * 0.5f);
            Vector3 size = new Vector3(_demoWidth * _demoCellSize, 0.1f, _demoHeight * _demoCellSize);
            Gizmos.DrawWireCube(center, size);
        }
    }
}