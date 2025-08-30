using UnityEngine;

public static class QATestingSystem
{
    public static void RunAllTests()
    {
        TestGridCell();
        TestGridMap();
        TestGridService();
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

    public static void TestGridService()
    {
        Debug.Log("[QATestingSystem] Starting tests for GridService.");

        // Test 1: Constructor and properties
        var map = new GridMap(5, 3);
        var originWorld = new Vector3(10f, 0f, 20f);
        var cellSize = 2.0f;
        var service = new GridService(map, originWorld, cellSize);

        Debug.Assert(service.Map == map, "[QATestingSystem] Error: GridService.Map property incorrect.");
        Debug.Assert(service.Width == 5, "[QATestingSystem] Error: GridService.Width property incorrect.");
        Debug.Assert(service.Height == 3, "[QATestingSystem] Error: GridService.Height property incorrect.");
        Debug.Assert(service.OriginWorld == originWorld, "[QATestingSystem] Error: GridService.OriginWorld property incorrect.");
        Debug.Assert(service.CellSize == cellSize, "[QATestingSystem] Error: GridService.CellSize property incorrect.");
        Debug.Log("[QATestingSystem] ✓ GridService constructor and properties test passed.");

        // Test 2: IsInside
        Debug.Assert(service.IsInside(new Vector2Int(0, 0)), "[QATestingSystem] Error: (0,0) should be inside.");
        Debug.Assert(service.IsInside(new Vector2Int(4, 2)), "[QATestingSystem] Error: (4,2) should be inside.");
        Debug.Assert(!service.IsInside(new Vector2Int(-1, 0)), "[QATestingSystem] Error: (-1,0) should not be inside.");
        Debug.Assert(!service.IsInside(new Vector2Int(5, 0)), "[QATestingSystem] Error: (5,0) should not be inside.");
        Debug.Assert(!service.IsInside(new Vector2Int(0, 3)), "[QATestingSystem] Error: (0,3) should not be inside.");
        Debug.Log("[QATestingSystem] ✓ GridService.IsInside test passed.");

        // Test 3: GetCell safe wrapper
        var cell = service.GetCell(new Vector2Int(2, 1));
        Debug.Assert(cell != null, "[QATestingSystem] Error: GetCell should return valid cell for valid coordinates.");
        Debug.Assert(cell.Type == CellType.Empty, "[QATestingSystem] Error: Default cell should be Empty.");

        var outOfBoundsCell = service.GetCell(new Vector2Int(10, 10));
        Debug.Assert(outOfBoundsCell == null, "[QATestingSystem] Error: GetCell should return null for out-of-bounds coordinates.");
        Debug.Log("[QATestingSystem] ✓ GridService.GetCell test passed.");

        // Test 4: IsWalkable
        Debug.Assert(service.IsWalkable(new Vector2Int(1, 1)), "[QATestingSystem] Error: Empty cell should be walkable.");
        Debug.Assert(!service.IsWalkable(new Vector2Int(10, 10)), "[QATestingSystem] Error: Out-of-bounds cell should not be walkable.");

        // Add a wall and test
        map.SetCell(2, 1, new GridCell(CellType.Wall));
        Debug.Assert(!service.IsWalkable(new Vector2Int(2, 1)), "[QATestingSystem] Error: Wall cell should not be walkable.");

        // Add a robot and test
        var robotCell = new GridCell(CellType.Empty);
        robotCell.AddOccupant(CellOccupant.Robot);
        map.SetCell(3, 1, robotCell);
        Debug.Assert(!service.IsWalkable(new Vector2Int(3, 1)), "[QATestingSystem] Error: Cell with robot should not be walkable.");
        Debug.Log("[QATestingSystem] ✓ GridService.IsWalkable test passed.");

        // Test 5: GetNeighbors4
        var neighbors = service.GetNeighbors4(new Vector2Int(1, 1));
        Debug.Assert(neighbors != null, "[QATestingSystem] Error: GetNeighbors4 should not return null.");
        var neighborsList = new System.Collections.Generic.List<Vector2Int>(neighbors);
        Debug.Assert(neighborsList.Count == 4, "[QATestingSystem] Error: Center cell should have 4 neighbors.");
        Debug.Log("[QATestingSystem] ✓ GridService.GetNeighbors4 test passed.");

        // Test 6: CellToWorld conversion
        var worldPos = service.CellToWorld(new Vector2Int(0, 0));
        var expectedPos = new Vector3(originWorld.x + 0.5f * cellSize, originWorld.y, originWorld.z + 0.5f * cellSize);
        Debug.Assert(Vector3.Distance(worldPos, expectedPos) < 0.001f, "[QATestingSystem] Error: CellToWorld conversion incorrect for (0,0).");

        var worldPos2 = service.CellToWorld(new Vector2Int(2, 1));
        var expectedPos2 = new Vector3(originWorld.x + 2.5f * cellSize, originWorld.y, originWorld.z + 1.5f * cellSize);
        Debug.Assert(Vector3.Distance(worldPos2, expectedPos2) < 0.001f, "[QATestingSystem] Error: CellToWorld conversion incorrect for (2,1).");
        Debug.Log("[QATestingSystem] ✓ GridService.CellToWorld test passed.");

        // Test 7: WorldToCell conversion
        var cellPos = service.WorldToCell(expectedPos);
        Debug.Assert(cellPos == new Vector2Int(0, 0), "[QATestingSystem] Error: WorldToCell conversion incorrect for center of (0,0).");

        var cellPos2 = service.WorldToCell(expectedPos2);
        Debug.Assert(cellPos2 == new Vector2Int(2, 1), "[QATestingSystem] Error: WorldToCell conversion incorrect for center of (2,1).");

        // Test edge case
        var edgeWorldPos = new Vector3(originWorld.x, originWorld.y, originWorld.z);
        var edgeCellPos = service.WorldToCell(edgeWorldPos);
        Debug.Assert(edgeCellPos == new Vector2Int(0, 0), "[QATestingSystem] Error: WorldToCell conversion incorrect for origin edge.");
        Debug.Log("[QATestingSystem] ✓ GridService.WorldToCell test passed.");

        // Test 8: Round-trip conversion consistency
        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                var originalCell = new Vector2Int(x, y);
                var worldPosition = service.CellToWorld(originalCell);
                var convertedCell = service.WorldToCell(worldPosition);
                Debug.Assert(convertedCell == originalCell, 
                    $"[QATestingSystem] Error: Round-trip conversion failed for cell ({x},{y}). Got {convertedCell}.");
            }
        }
        Debug.Log("[QATestingSystem] ✓ GridService round-trip conversion test passed.");

        // Test 9: Occupant utilities
        var testCell = new Vector2Int(1, 0);
        Debug.Assert(service.AddOccupant(testCell, CellOccupant.Jewel), "[QATestingSystem] Error: AddOccupant should succeed for valid cell.");
        Debug.Assert(service.HasOccupant(testCell, CellOccupant.Jewel), "[QATestingSystem] Error: HasOccupant should return true after adding.");
        Debug.Assert(service.RemoveOccupant(testCell, CellOccupant.Jewel), "[QATestingSystem] Error: RemoveOccupant should succeed for valid cell.");
        Debug.Assert(!service.HasOccupant(testCell, CellOccupant.Jewel), "[QATestingSystem] Error: HasOccupant should return false after removing.");

        // Test with invalid cell
        var invalidCell = new Vector2Int(10, 10);
        Debug.Assert(!service.AddOccupant(invalidCell, CellOccupant.Jewel), "[QATestingSystem] Error: AddOccupant should fail for invalid cell.");
        Debug.Assert(!service.HasOccupant(invalidCell, CellOccupant.Jewel), "[QATestingSystem] Error: HasOccupant should return false for invalid cell.");
        Debug.Assert(!service.RemoveOccupant(invalidCell, CellOccupant.Jewel), "[QATestingSystem] Error: RemoveOccupant should fail for invalid cell.");
        Debug.Log("[QATestingSystem] ✓ GridService occupant utilities test passed.");

        // Test 10: Reservation system
        var reserveCell = new Vector2Int(0, 1);
        Debug.Assert(service.TryReserve(reserveCell), "[QATestingSystem] Error: TryReserve should succeed for empty cell.");
        Debug.Assert(service.HasOccupant(reserveCell, CellOccupant.Reserved), "[QATestingSystem] Error: Cell should have Reserved occupant after reservation.");
        Debug.Assert(!service.IsWalkable(reserveCell), "[QATestingSystem] Error: Reserved cell should not be walkable.");

        // Try to reserve again - should fail
        Debug.Assert(!service.TryReserve(reserveCell), "[QATestingSystem] Error: TryReserve should fail for already reserved cell.");

        // Release reservation
        Debug.Assert(service.ReleaseReserve(reserveCell), "[QATestingSystem] Error: ReleaseReserve should succeed for reserved cell.");
        Debug.Assert(!service.HasOccupant(reserveCell, CellOccupant.Reserved), "[QATestingSystem] Error: Cell should not have Reserved occupant after release.");
        Debug.Assert(service.IsWalkable(reserveCell), "[QATestingSystem] Error: Cell should be walkable again after releasing reservation.");

        // Test reservation with invalid cell
        Debug.Assert(!service.TryReserve(invalidCell), "[QATestingSystem] Error: TryReserve should fail for invalid cell.");
        Debug.Assert(!service.ReleaseReserve(invalidCell), "[QATestingSystem] Error: ReleaseReserve should fail for invalid cell.");
        Debug.Log("[QATestingSystem] ✓ GridService reservation system test passed.");

        // Test 11: Default cell size constructor
        var serviceDefault = new GridService(map, Vector3.zero);
        Debug.Assert(serviceDefault.CellSize == 1.0f, "[QATestingSystem] Error: Default cell size should be 1.0f.");
        
        var worldPosDefault = serviceDefault.CellToWorld(new Vector2Int(1, 1));
        var expectedPosDefault = new Vector3(1.5f, 0f, 1.5f);
        Debug.Assert(Vector3.Distance(worldPosDefault, expectedPosDefault) < 0.001f, "[QATestingSystem] Error: Default cell size conversion incorrect.");
        Debug.Log("[QATestingSystem] ✓ GridService default constructor test passed.");

        // Test 12: Constructor validation
        try
        {
            var invalidService = new GridService(null, Vector3.zero);
            Debug.Assert(false, "[QATestingSystem] Error: Constructor should throw exception for null GridMap.");
        }
        catch (System.ArgumentNullException)
        {
            // Expected
        }
        Debug.Log("[QATestingSystem] ✓ GridService constructor validation test passed.");

        Debug.Log("[QATestingSystem] All tests for GridService completed successfully.");
    }
}
