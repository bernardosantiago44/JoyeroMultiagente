using UnityEngine;

/// <summary>
/// Test standalone GridSpawner structural element creation functionality.
/// Verifies wall perimeters, shelves, and zones are created correctly.
/// </summary>
public class GridSpawnerStructuralTest : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private int _testWidth = 10;
    [SerializeField] private int _testHeight = 8;
    [SerializeField] private float _testCellSize = 1.0f;

    [ContextMenu("Test Wall Perimeter")]
    public void TestWallPerimeter()
    {
        Debug.Log("=== TESTING WALL PERIMETER ===");

        // Create test grid
        var gridMap = new GridMap(_testWidth, _testHeight);
        var gridService = new GridService(gridMap, Vector3.zero, _testCellSize);

        // Create test spawn config with wall perimeter enabled
        var spawnConfig = ScriptableObject.CreateInstance<SpawnConfig>();
        SetPrivateField(spawnConfig, "_enableWallPerimeter", true);

        // Find GridSpawner
        var gridSpawner = FindObjectOfType<GridSpawner>();
        if (gridSpawner == null)
        {
            Debug.LogError("GridSpawner not found in scene!");
            return;
        }

        // Test wall perimeter creation
        gridSpawner.PopulateStructuralElements(gridService, spawnConfig);

        // Verify perimeter walls
        VerifyWallPerimeter(gridService);

        Debug.Log("Wall perimeter test completed!");
    }

    [ContextMenu("Test Shelves")]
    public void TestShelves()
    {
        Debug.Log("=== TESTING SHELVES ===");

        // Create test grid
        var gridMap = new GridMap(_testWidth, _testHeight);
        var gridService = new GridService(gridMap, Vector3.zero, _testCellSize);

        // Create test spawn config with shelves enabled
        var spawnConfig = ScriptableObject.CreateInstance<SpawnConfig>();
        SetPrivateField(spawnConfig, "_enableShelves", true);
        SetPrivateField(spawnConfig, "_shelfRows", 2);
        SetPrivateField(spawnConfig, "_shelfColumns", 1);

        // Find GridSpawner
        var gridSpawner = FindObjectOfType<GridSpawner>();
        if (gridSpawner == null)
        {
            Debug.LogError("GridSpawner not found in scene!");
            return;
        }

        // Test shelf creation
        gridSpawner.PopulateStructuralElements(gridService, spawnConfig);

        // Verify shelves
        VerifyShelves(gridService, 2, 1);

        Debug.Log("Shelves test completed!");
    }

    [ContextMenu("Test Zones")]
    public void TestZones()
    {
        Debug.Log("=== TESTING ZONES ===");

        // Create test grid
        var gridMap = new GridMap(_testWidth, _testHeight);
        var gridService = new GridService(gridMap, Vector3.zero, _testCellSize);

        // Create test spawn config with zones enabled
        var spawnConfig = ScriptableObject.CreateInstance<SpawnConfig>();
        SetPrivateField(spawnConfig, "_enableZones", true);
        SetPrivateField(spawnConfig, "_numZones", 3);

        // Find GridSpawner
        var gridSpawner = FindObjectOfType<GridSpawner>();
        if (gridSpawner == null)
        {
            Debug.LogError("GridSpawner not found in scene!");
            return;
        }

        // Test zone creation
        gridSpawner.PopulateStructuralElements(gridService, spawnConfig);

        // Verify zones
        VerifyZones(gridService, 3);

        Debug.Log("Zones test completed!");
    }

    private void VerifyWallPerimeter(GridService gridService)
    {
        int wallCount = 0;
        int width = gridService.Width;
        int height = gridService.Height;

        // Check all perimeter cells
        for (int x = 0; x < width; x++)
        {
            // Top and bottom edges
            if (gridService.Map.GetCell(x, 0).Type == CellType.Wall) wallCount++;
            if (gridService.Map.GetCell(x, height - 1).Type == CellType.Wall) wallCount++;
        }

        for (int y = 0; y < height; y++)
        {
            // Left and right edges
            if (gridService.Map.GetCell(0, y).Type == CellType.Wall) wallCount++;
            if (gridService.Map.GetCell(width - 1, y).Type == CellType.Wall) wallCount++;
        }

        int expectedWalls = 2 * width + 2 * height - 4; // Perimeter cells minus corners counted twice
        Debug.Log($"Wall perimeter verification: {wallCount} walls found, {expectedWalls} expected");
        
        if (wallCount == expectedWalls)
        {
            Debug.Log("✅ Wall perimeter created correctly!");
        }
        else
        {
            Debug.LogWarning($"⚠️ Wall count mismatch: expected {expectedWalls}, found {wallCount}");
        }
    }

    private void VerifyShelves(GridService gridService, int expectedRows, int expectedColumns)
    {
        int shelfCount = 0;
        
        for (int x = 0; x < gridService.Width; x++)
        {
            for (int y = 0; y < gridService.Height; y++)
            {
                if (gridService.Map.GetCell(x, y).Type == CellType.Shelf)
                {
                    shelfCount++;
                }
            }
        }

        Debug.Log($"Shelf verification: {shelfCount} shelf cells found");
        Debug.Log($"Expected {expectedRows} rows and {expectedColumns} columns of shelves");
        
        if (shelfCount > 0)
        {
            Debug.Log("✅ Shelves created successfully!");
        }
        else
        {
            Debug.LogWarning("⚠️ No shelf cells found");
        }
    }

    private void VerifyZones(GridService gridService, int expectedZones)
    {
        int zoneCount = 0;
        
        for (int x = 0; x < gridService.Width; x++)
        {
            for (int y = 0; y < gridService.Height; y++)
            {
                if (gridService.Map.GetCell(x, y).Type == CellType.Zone)
                {
                    zoneCount++;
                }
            }
        }

        Debug.Log($"Zone verification: {zoneCount} zone cells found, {expectedZones} expected");
        
        if (zoneCount == expectedZones)
        {
            Debug.Log("✅ Zones created correctly!");
        }
        else
        {
            Debug.LogWarning($"⚠️ Zone count mismatch: expected {expectedZones}, found {zoneCount}");
        }
    }

    private void SetPrivateField(object obj, string fieldName, object value)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(obj, value);
    }
}