using System.Collections.Generic;
using UnityEngine;

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
