using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utilidad para instanciar prefabs (robots, joyas, zona, obstáculos) en posiciones de celda,
/// resolviendo la conversión a coordenadas de mundo y registrando la ocupación.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Llamado por <see cref="SpawnSystem"/> para crear entidades del escenario.
/// - Actualiza <see cref="GridService"/> (ocupación) y devuelve referencias a MB creados.
/// </remarks>
public class GridSpawner : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Renderer boardRenderer;   // Renderer del plano/tablero
    [SerializeField] private GameObject rupeePrefab;   // Tu Rupee con la luz como hijo
    [SerializeField] private Transform parent;         // Contenedor opcional para los instanciados

    [Header("Configuración de grid")]
    [SerializeField] private float cellSize = 1f;      // 1 => coordenadas enteras
    [SerializeField] private int count = 20;           // N a generar
    [SerializeField] private bool randomYRotation = false;
    [SerializeField] private float spawnYOffset = 0f;  // Offset para que se vea más realista
    [SerializeField] private int seed = 0;

    // Para que otros sistemas/Agentes lean dónde quedaron (en enteros)
    public IReadOnlyList<Vector2Int> SpawnedGridPositions => spawnedCells;

    private readonly List<Vector2Int> spawnedCells = new();
    private int gridW, gridH;
    private Vector3 origin;    // Esquina inferior-izquierda (centro de celda)

    private Bounds BoardBounds => boardRenderer.bounds;

    private void Awake()
    {
        if (parent == null) parent = transform;
        if (boardRenderer == null || rupeePrefab == null)
        {
            Debug.LogError("[GridSpawner] Asigna boardRenderer y rupeePrefab en el Inspector.");
            enabled = false;
            return;
        }

        SetupGrid();
        SpawnAll();
    }

    private void SetupGrid()
    {
        var b = BoardBounds;

        // Cantidad de celdas que caben en X y Z
        gridW = Mathf.Max(1, Mathf.FloorToInt(b.size.x / cellSize));
        gridH = Mathf.Max(1, Mathf.FloorToInt(b.size.z / cellSize));

        // Centro de la celda (0,0): media celda adentro desde el mínimo del bounds
        origin = new Vector3(b.min.x + cellSize * 0.5f, b.min.y, b.min.z + cellSize * 0.5f);
    }

    private void SpawnAll()
    {
        int total = gridW * gridH;
        int n = Mathf.Clamp(count, 0, total);

        // Construye todas las celdas y barájalas (Fisher–Yates)
        var cells = new List<Vector2Int>(total);
        for (int y = 0; y < gridH; y++)
            for (int x = 0; x < gridW; x++)
                cells.Add(new Vector2Int(x, y));

        var rng = (seed == 0) ? new System.Random() : new System.Random(seed);
        for (int i = cells.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (cells[i], cells[j]) = (cells[j], cells[i]);
        }

        spawnedCells.Clear();

        for (int i = 0; i < n; i++)
        {
            var cell = cells[i];
            spawnedCells.Add(cell);

            Vector3 pos = CellCenterWorld(cell);
            pos.y = BoardBounds.max.y + spawnYOffset;

            Quaternion rot = randomYRotation
                ? Quaternion.Euler(90f, rng.Next(0, 360), 0f)
                : Quaternion.Euler(90f, 0f, 0f);

            var go = Instantiate(rupeePrefab, pos, rot, parent);
            go.name = $"{rupeePrefab.name}_({cell.x},{cell.y})";
        }
    }

    private Vector3 CellCenterWorld(Vector2Int cell)
        => origin + new Vector3(cell.x * cellSize, 0f, cell.y * cellSize);

    // Útil si un agente te da una posición y quieres su celda entera más cercana
    public Vector2Int WorldToCell(Vector3 world)
    {
        var local = world - origin;
        int x = Mathf.RoundToInt(local.x / cellSize);
        int y = Mathf.RoundToInt(local.z / cellSize);
        return new Vector2Int(Mathf.Clamp(x, 0, gridW - 1), Mathf.Clamp(y, 0, gridH - 1));
    }

    /// <summary>
    /// API pública para que SpawnSystem delegue la colocación de elementos estructurales.
    /// Marca tipos de celda según la configuración proporcionada.
    /// </summary>
    /// <param name="gridService">Servicio de grid donde marcar las celdas</param>
    /// <param name="spawnConfig">Configuración que define qué estructuras crear</param>
    public void PopulateStructuralElements(GridService gridService, SpawnConfig spawnConfig)
    {
        if (gridService?.Map == null)
        {
            Debug.LogError("[GridSpawner] GridService o GridMap es null. No se pueden poblar elementos estructurales.");
            return;
        }

        Debug.Log("[GridSpawner] Poblando elementos estructurales...");

        // 1. Crear perímetro de paredes si está habilitado
        if (spawnConfig.EnableWallPerimeter)
        {
            CreateWallPerimeter(gridService);
        }

        // 2. Crear estantes si están habilitados
        if (spawnConfig.EnableShelves)
        {
            CreateShelves(gridService, spawnConfig.ShelfRows, spawnConfig.ShelfColumns);
        }

        // 3. Crear zonas si están habilitadas
        if (spawnConfig.EnableZones)
        {
            CreateZones(gridService, spawnConfig.NumZones);
        }

        Debug.Log("[GridSpawner] Elementos estructurales poblados exitosamente.");
    }

    /// <summary>
    /// Crea un perímetro de paredes alrededor del borde del mapa.
    /// </summary>
    private void CreateWallPerimeter(GridService gridService)
    {
        var map = gridService.Map;
        int width = map.Width;
        int height = map.Height;

        // Paredes superior e inferior
        for (int x = 0; x < width; x++)
        {
            SetCellType(map, x, 0, CellType.Wall);           // Borde inferior
            SetCellType(map, x, height - 1, CellType.Wall);  // Borde superior
        }

        // Paredes izquierda y derecha
        for (int y = 0; y < height; y++)
        {
            SetCellType(map, 0, y, CellType.Wall);           // Borde izquierdo
            SetCellType(map, width - 1, y, CellType.Wall);   // Borde derecho
        }

        Debug.Log($"[GridSpawner] Created wall perimeter around {width}x{height} map");
    }

    /// <summary>
    /// Crea estantes en patrones de filas y columnas.
    /// </summary>
    private void CreateShelves(GridService gridService, int shelfRows, int shelfColumns)
    {
        var map = gridService.Map;
        int width = map.Width;
        int height = map.Height;

        // Crear filas de estantes (horizontales)
        if (shelfRows > 0)
        {
            int rowSpacing = height / (shelfRows + 1);
            for (int row = 1; row <= shelfRows; row++)
            {
                int y = row * rowSpacing;
                if (y >= height - 1) break; // Evitar borde superior

                for (int x = 1; x < width - 1; x++) // Evitar bordes laterales
                {
                    SetCellType(map, x, y, CellType.Shelf);
                }
            }
        }

        // Crear columnas de estantes (verticales)
        if (shelfColumns > 0)
        {
            int colSpacing = width / (shelfColumns + 1);
            for (int col = 1; col <= shelfColumns; col++)
            {
                int x = col * colSpacing;
                if (x >= width - 1) break; // Evitar borde derecho

                for (int y = 1; y < height - 1; y++) // Evitar bordes superior e inferior
                {
                    SetCellType(map, x, y, CellType.Shelf);
                }
            }
        }

        Debug.Log($"[GridSpawner] Created {shelfRows} shelf rows and {shelfColumns} shelf columns");
    }

    /// <summary>
    /// Crea zonas distribuidas en el mapa.
    /// </summary>
    private void CreateZones(GridService gridService, int numZones)
    {
        var map = gridService.Map;
        int width = map.Width;
        int height = map.Height;

        // Crear zonas en las esquinas disponibles
        var zonePositions = new List<Vector2Int>();

        // Esquinas del mapa (evitando paredes si existen)
        var corners = new Vector2Int[]
        {
            new Vector2Int(1, 1),                    // Esquina inferior-izquierda
            new Vector2Int(width - 2, 1),            // Esquina inferior-derecha
            new Vector2Int(1, height - 2),           // Esquina superior-izquierda
            new Vector2Int(width - 2, height - 2)    // Esquina superior-derecha
        };

        for (int i = 0; i < numZones && i < corners.Length; i++)
        {
            var pos = corners[i];
            if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
            {
                SetCellType(map, pos.x, pos.y, CellType.Zone);
                zonePositions.Add(pos);
            }
        }

        Debug.Log($"[GridSpawner] Created {zonePositions.Count} zones at positions: {string.Join(", ", zonePositions)}");
    }

    /// <summary>
    /// Establece el tipo de una celda si está dentro de los límites del mapa.
    /// </summary>
    private void SetCellType(GridMap map, int x, int y, CellType cellType)
    {
        if (map.InBounds(x, y))
        {
            var cell = map.GetCell(x, y);
            cell.SetType(cellType);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Respawn (Editor)")]
    private void Respawn()
    {
        // Limpia hijos (menos este transform)
        var toDelete = new List<GameObject>();
        foreach (Transform child in parent)
            if (child != transform) toDelete.Add(child.gameObject);
        foreach (var go in toDelete) DestroyImmediate(go);

        SetupGrid();
        SpawnAll();
    }

    private void OnDrawGizmosSelected()
    {
        if (boardRenderer == null) return;
        SetupGrid();

        Gizmos.color = new Color(1, 1, 1, 0.15f);
        for (int y = 0; y < gridH; y++)
        {
            for (int x = 0; x < gridW; x++)
            {
                var p = CellCenterWorld(new Vector2Int(x, y));
                Gizmos.DrawWireSphere(p + Vector3.up * 0.01f, 0.1f);
            }
        }

        Gizmos.color = Color.cyan;
        foreach (var c in spawnedCells)
        {
            var p = CellCenterWorld(c);
            Gizmos.DrawSphere(p + Vector3.up * 0.02f, 0.12f);
        }
    }
#endif
}
