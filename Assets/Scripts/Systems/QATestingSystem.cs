using UnityEngine;

public static class QATestingSystem
{
    public static void RunAllTests()
    {
        TestGridCell();
        TestGridMap();
    }

    public static void TestGridCell()
    {
        Debug.Log("[QATestingSystem] Starting tests for GridCell.");

        // Prueba de creación de celdas
        var cell = new GridCell(CellType.Empty);
        Debug.Assert(cell.Type == CellType.Empty, "[QATestingSystem] Error: Tipo de celda no coincide.");

        // Prueba de ocupantes
        cell.AddOccupant(CellOccupant.Robot);
        Debug.Assert(cell.HasOccupant(CellOccupant.Robot), "[QATestingSystem] Error: Robot no encontrado como ocupante.");

        UnityEngine.Debug.Log(cell.IsWalkableNow); // true

        cell.AddOccupant(CellOccupant.Robot);
        UnityEngine.Debug.Log(cell.IsWalkableNow); // false

        cell.RemoveOccupant(CellOccupant.Robot);
        cell.SetType(CellType.Wall);
        UnityEngine.Debug.Log(cell.IsWalkableNow); // false por tipo (muro)

        Debug.Log("[QATestingSystem] All tests for GridCell completed.");
    }

    public static void TestGridMap()
    {
        Debug.Log("[QATestingSystem] Starting tests for GridMap.");

        // Test 1: Creación de GridMap
        var map = new GridMap(5, 3);
        Debug.Assert(map.Width == 5, "[QATestingSystem] Error: Ancho del mapa incorrecto.");
        Debug.Assert(map.Height == 3, "[QATestingSystem] Error: Alto del mapa incorrecto.");
        Debug.Log("[QATestingSystem] ✓ GridMap creation test passed.");

        // Test 2: InBounds
        Debug.Assert(map.InBounds(0, 0), "[QATestingSystem] Error: (0,0) debería estar dentro del mapa.");
        Debug.Assert(map.InBounds(4, 2), "[QATestingSystem] Error: (4,2) debería estar dentro del mapa.");
        Debug.Assert(!map.InBounds(-1, 0), "[QATestingSystem] Error: (-1,0) no debería estar dentro del mapa.");
        Debug.Assert(!map.InBounds(5, 0), "[QATestingSystem] Error: (5,0) no debería estar dentro del mapa.");
        Debug.Assert(!map.InBounds(0, 3), "[QATestingSystem] Error: (0,3) no debería estar dentro del mapa.");
        Debug.Assert(map.InBounds(new Vector2Int(2, 1)), "[QATestingSystem] Error: Vector2Int(2,1) debería estar dentro del mapa.");
        Debug.Log("[QATestingSystem] ✓ InBounds test passed.");

        // Test 3: GetCell y SetCell
        var originalCell = map.GetCell(2, 1);
        Debug.Assert(originalCell != null, "[QATestingSystem] Error: GetCell devolvió null.");
        Debug.Assert(originalCell.Type == CellType.Empty, "[QATestingSystem] Error: Celda inicial no es Empty.");

        var wallCell = new GridCell(CellType.Wall);
        map.SetCell(2, 1, wallCell);
        var retrievedCell = map.GetCell(2, 1);
        Debug.Assert(retrievedCell.Type == CellType.Wall, "[QATestingSystem] Error: SetCell no funcionó correctamente.");

        // Test con Vector2Int
        map.SetCell(new Vector2Int(3, 0), new GridCell(CellType.Shelf));
        Debug.Assert(map.GetCell(new Vector2Int(3, 0)).Type == CellType.Shelf, "[QATestingSystem] Error: SetCell/GetCell con Vector2Int falló.");
        Debug.Log("[QATestingSystem] ✓ GetCell/SetCell test passed.");

        // Test 4: GetNeighbors4 - centro del mapa
        var neighbors = map.GetNeighbors4(2, 1);
        Debug.Assert(neighbors.Count == 4, "[QATestingSystem] Error: Centro del mapa debería tener 4 vecinos.");
        Debug.Assert(neighbors.Contains(new Vector2Int(2, 2)), "[QATestingSystem] Error: Falta vecino arriba.");
        Debug.Assert(neighbors.Contains(new Vector2Int(3, 1)), "[QATestingSystem] Error: Falta vecino derecha.");
        Debug.Assert(neighbors.Contains(new Vector2Int(2, 0)), "[QATestingSystem] Error: Falta vecino abajo.");
        Debug.Assert(neighbors.Contains(new Vector2Int(1, 1)), "[QATestingSystem] Error: Falta vecino izquierda.");
        Debug.Log("[QATestingSystem] ✓ GetNeighbors4 center test passed.");

        // Test 5: GetNeighbors4 - esquina superior izquierda
        var cornerNeighbors = map.GetNeighbors4(0, 2);
        Debug.Assert(cornerNeighbors.Count == 2, "[QATestingSystem] Error: Esquina superior izquierda debería tener 2 vecinos.");
        Debug.Assert(cornerNeighbors.Contains(new Vector2Int(1, 2)), "[QATestingSystem] Error: Falta vecino derecha en esquina.");
        Debug.Assert(cornerNeighbors.Contains(new Vector2Int(0, 1)), "[QATestingSystem] Error: Falta vecino abajo en esquina.");
        Debug.Log("[QATestingSystem] ✓ GetNeighbors4 corner test passed.");

        // Test 6: GetNeighbors4 - borde lateral
        var sideNeighbors = map.GetNeighbors4(4, 1);
        Debug.Assert(sideNeighbors.Count == 3, "[QATestingSystem] Error: Borde lateral debería tener 3 vecinos.");
        Debug.Assert(sideNeighbors.Contains(new Vector2Int(4, 2)), "[QATestingSystem] Error: Falta vecino arriba en borde.");
        Debug.Assert(sideNeighbors.Contains(new Vector2Int(4, 0)), "[QATestingSystem] Error: Falta vecino abajo en borde.");
        Debug.Assert(sideNeighbors.Contains(new Vector2Int(3, 1)), "[QATestingSystem] Error: Falta vecino izquierda en borde.");
        Debug.Log("[QATestingSystem] ✓ GetNeighbors4 edge test passed.");

        // Test 7: GetNeighbors4 con Vector2Int
        var vectorNeighbors = map.GetNeighbors4(new Vector2Int(1, 0));
        Debug.Assert(vectorNeighbors.Count == 3, "[QATestingSystem] Error: GetNeighbors4 con Vector2Int falló.");
        Debug.Log("[QATestingSystem] ✓ GetNeighbors4 Vector2Int test passed.");

        // Test 8: Manejo de excepciones
        try
        {
            map.GetCell(10, 10);
            Debug.Assert(false, "[QATestingSystem] Error: GetCell debería lanzar excepción para coordenadas fuera del mapa.");
        }
        catch (System.ArgumentOutOfRangeException)
        {
            // Esperado
        }

        try
        {
            map.SetCell(0, 0, null);
            Debug.Assert(false, "[QATestingSystem] Error: SetCell debería lanzar excepción para celda null.");
        }
        catch (System.ArgumentNullException)
        {
            // Esperado
        }
        Debug.Log("[QATestingSystem] ✓ Exception handling test passed.");

        Debug.Log("[QATestingSystem] All tests for GridMap completed successfully.");
    }
}
