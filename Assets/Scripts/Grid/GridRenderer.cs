using UnityEngine;

/// <summary>
/// Encargado de visualizar el <see cref="GridMap"/> en la escena (tiles, gizmos).
/// No modifica la lógica; solo refleja el estado actual del mundo.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Lee datos desde <see cref="GridService"/>.
/// - Opcionalmente muestra paths del <see cref="PathfindingService"/> para debug.
/// </remarks>
public sealed class GridRenderer : MonoBehaviour
{
    [Header("Preview Mode Settings")]
    [SerializeField] private int _previewWidth = 20;
    [SerializeField] private int _previewHeight = 15;
    [SerializeField] private float _previewCellSize = 1.0f;
    [SerializeField] private Vector3 _previewOrigin = Vector3.zero;
    
    [Header("Rendering Settings")]
    [SerializeField] private bool _showGrid = true;
    [SerializeField] private bool _showCellTypes = true;
    [SerializeField] private bool _showOccupancy = true;
    [SerializeField] private bool _useTileMode = false; // Future extensibility
    
    [Header("Colors")]
    [SerializeField] private Color _emptyColor = new Color(0.8f, 0.8f, 0.8f, 0.3f);
    [SerializeField] private Color _wallColor = new Color(0.5f, 0.3f, 0.2f, 0.8f);
    [SerializeField] private Color _shelfColor = new Color(0.7f, 0.5f, 0.3f, 0.8f);
    [SerializeField] private Color _jewelColor = new Color(1.0f, 0.8f, 0.2f, 0.6f);
    [SerializeField] private Color _zoneColor = new Color(0.2f, 0.8f, 0.3f, 0.6f);
    [SerializeField] private Color _robotSpawnColor = new Color(0.3f, 0.6f, 1.0f, 0.6f);
    [SerializeField] private Color _gridLineColor = new Color(0.6f, 0.6f, 0.6f, 0.5f);
    
    [Header("Occupancy Colors")]
    [SerializeField] private Color _robotOccupantColor = new Color(1.0f, 0.2f, 0.2f, 0.7f);
    [SerializeField] private Color _jewelOccupantColor = new Color(1.0f, 1.0f, 0.2f, 0.5f);
    [SerializeField] private Color _zoneOccupantColor = new Color(0.2f, 1.0f, 0.2f, 0.3f);
    [SerializeField] private Color _reservedColor = new Color(0.8f, 0.4f, 0.8f, 0.4f);

    private GridService _gridService;
    private bool _hasGridService;

    /// <summary>
    /// Intenta obtener el GridService del ServiceRegistry al inicializar.
    /// Si no está disponible, usa el modo preview.
    /// </summary>
    public void Start()
    {
        _hasGridService = ServiceRegistry.TryResolve<GridService>(out _gridService);
        
        if (!_hasGridService)
        {
            Debug.Log("[GridRenderer] GridService not found. Using preview mode.");
        }
        else
        {
            Debug.Log($"[GridRenderer] Connected to GridService. Grid size: {_gridService.Width}x{_gridService.Height}");
        }
    }

    /// <summary>
    /// Renderiza el grid usando Gizmos en el SceneView.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (!_showGrid) return;

        if (_hasGridService && _gridService != null)
        {
            DrawGridFromService();
        }
        else
        {
            DrawPreviewGrid();
        }
    }

    /// <summary>
    /// Dibuja el grid real usando datos del GridService.
    /// </summary>
    private void DrawGridFromService()
    {
        var gridService = _gridService;
        if (gridService?.Map == null) return;

        int width = gridService.Width;
        int height = gridService.Height;
        float cellSize = gridService.CellSize;
        Vector3 origin = gridService.OriginWorld;

        // Dibujar celdas
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var cell = gridService.GetCell(new Vector2Int(x, y));
                if (cell == null) continue;

                Vector3 worldPos = gridService.CellToWorld(new Vector2Int(x, y));
                DrawCell(worldPos, cellSize, cell);
            }
        }

        // Dibujar líneas del grid
        DrawGridLines(origin, width, height, cellSize);
    }

    /// <summary>
    /// Dibuja un grid de preview cuando no hay GridService disponible.
    /// </summary>
    private void DrawPreviewGrid()
    {
        int width = _previewWidth;
        int height = _previewHeight;
        float cellSize = _previewCellSize;
        Vector3 origin = _previewOrigin;

        // Dibujar celdas vacías de preview
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 worldPos = new Vector3(
                    origin.x + (x + 0.5f) * cellSize,
                    origin.y,
                    origin.z + (y + 0.5f) * cellSize
                );

                // Solo mostrar celdas vacías en modo preview
                Gizmos.color = _emptyColor;
                Gizmos.DrawCube(worldPos, new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f));
            }
        }

        // Dibujar líneas del grid
        DrawGridLines(origin, width, height, cellSize);
    }

    /// <summary>
    /// Dibuja una celda individual con su tipo y ocupación.
    /// </summary>
    /// <param name="worldPos">Posición del mundo del centro de la celda</param>
    /// <param name="cellSize">Tamaño de la celda</param>
    /// <param name="cell">Datos de la celda</param>
    private void DrawCell(Vector3 worldPos, float cellSize, GridCell cell)
    {
        if (!_showCellTypes) return;

        // Dibujar tipo de celda
        Color cellColor = GetCellTypeColor(cell.Type);
        Gizmos.color = cellColor;
        
        Vector3 cubeSize = new Vector3(cellSize * 0.9f, 0.1f, cellSize * 0.9f);
        Gizmos.DrawCube(worldPos, cubeSize);

        // Dibujar ocupación si está habilitada
        if (_showOccupancy && cell.Occupants != CellOccupant.None)
        {
            DrawOccupancy(worldPos, cellSize, cell.Occupants);
        }
    }

    /// <summary>
    /// Dibuja la ocupación de una celda (robots, joyas, etc).
    /// </summary>
    /// <param name="worldPos">Posición del mundo del centro de la celda</param>
    /// <param name="cellSize">Tamaño de la celda</param>
    /// <param name="occupants">Flags de ocupantes</param>
    private void DrawOccupancy(Vector3 worldPos, float cellSize, CellOccupant occupants)
    {
        float yOffset = 0.15f; // Elevar ocupantes por encima del tipo de celda
        Vector3 occupantPos = worldPos + Vector3.up * yOffset;
        
        if (occupants.HasFlag(CellOccupant.Robot))
        {
            Gizmos.color = _robotOccupantColor;
            Gizmos.DrawSphere(occupantPos, cellSize * 0.2f);
        }
        
        if (occupants.HasFlag(CellOccupant.Jewel))
        {
            Gizmos.color = _jewelOccupantColor;
            Gizmos.DrawWireSphere(occupantPos + Vector3.right * cellSize * 0.1f, cellSize * 0.15f);
        }
        
        if (occupants.HasFlag(CellOccupant.Zone))
        {
            Gizmos.color = _zoneOccupantColor;
            Gizmos.DrawWireCube(occupantPos, Vector3.one * cellSize * 0.3f);
        }
        
        if (occupants.HasFlag(CellOccupant.Reserved))
        {
            Gizmos.color = _reservedColor;
            Gizmos.DrawWireCube(occupantPos + Vector3.forward * cellSize * 0.1f, Vector3.one * cellSize * 0.25f);
        }
    }

    /// <summary>
    /// Obtiene el color correspondiente a un tipo de celda.
    /// </summary>
    /// <param name="cellType">Tipo de celda</param>
    /// <returns>Color asignado al tipo</returns>
    private Color GetCellTypeColor(CellType cellType)
    {
        return cellType switch
        {
            CellType.Empty => _emptyColor,
            CellType.Wall => _wallColor,
            CellType.Shelf => _shelfColor,
            CellType.Jewel => _jewelColor,
            CellType.Zone => _zoneColor,
            CellType.RobotSpawn => _robotSpawnColor,
            _ => _emptyColor
        };
    }

    /// <summary>
    /// Dibuja las líneas del grid.
    /// </summary>
    /// <param name="origin">Origen del grid</param>
    /// <param name="width">Ancho en celdas</param>
    /// <param name="height">Alto en celdas</param>
    /// <param name="cellSize">Tamaño de celda</param>
    private void DrawGridLines(Vector3 origin, int width, int height, float cellSize)
    {
        Gizmos.color = _gridLineColor;

        // Líneas verticales
        for (int x = 0; x <= width; x++)
        {
            Vector3 start = new Vector3(origin.x + x * cellSize, origin.y, origin.z);
            Vector3 end = new Vector3(origin.x + x * cellSize, origin.y, origin.z + height * cellSize);
            Gizmos.DrawLine(start, end);
        }

        // Líneas horizontales
        for (int y = 0; y <= height; y++)
        {
            Vector3 start = new Vector3(origin.x, origin.y, origin.z + y * cellSize);
            Vector3 end = new Vector3(origin.x + width * cellSize, origin.y, origin.z + y * cellSize);
            Gizmos.DrawLine(start, end);
        }
    }

    /// <summary>
    /// Intenta reconectar al GridService si no estaba disponible antes.
    /// Útil para casos donde el GridService se registra después del Start.
    /// </summary>
    [ContextMenu("Reconnect to GridService")]
    private void TryReconnectGridService()
    {
        _hasGridService = ServiceRegistry.TryResolve<GridService>(out _gridService);
        
        if (_hasGridService)
        {
            Debug.Log($"[GridRenderer] Reconnected to GridService. Grid size: {_gridService.Width}x{_gridService.Height}");
        }
        else
        {
            Debug.Log("[GridRenderer] GridService still not available.");
        }
    }
}