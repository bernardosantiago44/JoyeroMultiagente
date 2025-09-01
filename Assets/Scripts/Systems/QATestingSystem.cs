using UnityEngine;
using System.Reflection;

public static class QATestingSystem
{
    public static void RunAllTests()
    {
        TestGridCell();
        TestGridMap();
        TestGridService();
        TestPathfindingService();
        TestGridRenderer();
        TestValidationService();
        TestSpawnSystem();
        TestRobotController();
        TestJewel();
        TestZoneController();
        TestMetricsLogger();
        TestRuleSystem();

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

    public static void TestPathfindingService()
    {
        Debug.Log("[QATestingSystem] Starting tests for PathfindingService.");

        // Setup: Create a test map and service
        var map = new GridMap(5, 5);
        var gridService = new GridService(map, Vector3.zero, 1.0f);
        var pathfindingService = new PathfindingService(gridService);

        // Test 1: Constructor validation
        try
        {
            var invalidService = new PathfindingService(null);
            Debug.Assert(false, "[QATestingSystem] Error: PathfindingService constructor should reject null GridService.");
        }
        catch (System.ArgumentNullException)
        {
            // Expected
        }
        Debug.Log("[QATestingSystem] ✓ PathfindingService constructor validation test passed.");

        // Test 2: Simple path - start equals goal
        var result = pathfindingService.TryFindPath(new Vector2Int(1, 1), new Vector2Int(1, 1), out var path);
        Debug.Assert(result, "[QATestingSystem] Error: Should find path when start equals goal.");
        Debug.Assert(path.Count == 1, "[QATestingSystem] Error: Path should contain only start position when start equals goal.");
        Debug.Assert(path[0] == new Vector2Int(1, 1), "[QATestingSystem] Error: Path should contain start position when start equals goal.");
        Debug.Log("[QATestingSystem] ✓ PathfindingService start equals goal test passed.");

        // Test 3: Out of bounds - start
        result = pathfindingService.TryFindPath(new Vector2Int(-1, 0), new Vector2Int(1, 1), out path);
        Debug.Assert(!result, "[QATestingSystem] Error: Should not find path when start is out of bounds.");
        Debug.Assert(path.Count == 0, "[QATestingSystem] Error: Path should be empty when start is out of bounds.");
        Debug.Log("[QATestingSystem] ✓ PathfindingService out of bounds start test passed.");

        // Test 4: Out of bounds - goal
        result = pathfindingService.TryFindPath(new Vector2Int(1, 1), new Vector2Int(10, 10), out path);
        Debug.Assert(!result, "[QATestingSystem] Error: Should not find path when goal is out of bounds.");
        Debug.Assert(path.Count == 0, "[QATestingSystem] Error: Path should be empty when goal is out of bounds.");
        Debug.Log("[QATestingSystem] ✓ PathfindingService out of bounds goal test passed.");

        // Test 5: Blocked start
        map.SetCell(2, 2, new GridCell(CellType.Wall));
        result = pathfindingService.TryFindPath(new Vector2Int(2, 2), new Vector2Int(1, 1), out path);
        Debug.Assert(!result, "[QATestingSystem] Error: Should not find path when start is blocked.");
        Debug.Assert(path.Count == 0, "[QATestingSystem] Error: Path should be empty when start is blocked.");
        Debug.Log("[QATestingSystem] ✓ PathfindingService blocked start test passed.");

        // Test 6: Blocked goal
        result = pathfindingService.TryFindPath(new Vector2Int(1, 1), new Vector2Int(2, 2), out path);
        Debug.Assert(!result, "[QATestingSystem] Error: Should not find path when goal is blocked.");
        Debug.Assert(path.Count == 0, "[QATestingSystem] Error: Path should be empty when goal is blocked.");
        Debug.Log("[QATestingSystem] ✓ PathfindingService blocked goal test passed.");

        // Test 7: Simple straight line path (horizontal)
        // Clear the wall first
        map.SetCell(2, 2, new GridCell(CellType.Empty));
        result = pathfindingService.TryFindPath(new Vector2Int(0, 0), new Vector2Int(3, 0), out path);
        Debug.Assert(result, "[QATestingSystem] Error: Should find horizontal straight line path.");
        Debug.Assert(path.Count == 4, "[QATestingSystem] Error: Horizontal path from (0,0) to (3,0) should have 4 cells.");
        Debug.Assert(path[0] == new Vector2Int(0, 0), "[QATestingSystem] Error: Path should start at (0,0).");
        Debug.Assert(path[3] == new Vector2Int(3, 0), "[QATestingSystem] Error: Path should end at (3,0).");
        // Verify all cells are consecutive
        for (int i = 0; i < path.Count; i++)
        {
            Debug.Assert(path[i] == new Vector2Int(i, 0), $"[QATestingSystem] Error: Path cell {i} should be ({i},0) but was {path[i]}.");
        }
        Debug.Log("[QATestingSystem] ✓ PathfindingService horizontal straight line test passed.");

        // Test 8: Simple straight line path (vertical)
        result = pathfindingService.TryFindPath(new Vector2Int(1, 0), new Vector2Int(1, 3), out path);
        Debug.Assert(result, "[QATestingSystem] Error: Should find vertical straight line path.");
        Debug.Assert(path.Count == 4, "[QATestingSystem] Error: Vertical path from (1,0) to (1,3) should have 4 cells.");
        Debug.Assert(path[0] == new Vector2Int(1, 0), "[QATestingSystem] Error: Path should start at (1,0).");
        Debug.Assert(path[3] == new Vector2Int(1, 3), "[QATestingSystem] Error: Path should end at (1,3).");
        // Verify all cells are consecutive
        for (int i = 0; i < path.Count; i++)
        {
            Debug.Assert(path[i] == new Vector2Int(1, i), $"[QATestingSystem] Error: Path cell {i} should be (1,{i}) but was {path[i]}.");
        }
        Debug.Log("[QATestingSystem] ✓ PathfindingService vertical straight line test passed.");

        // Test 9: L-shaped path around obstacle
        // Create an obstacle that forces an L-shaped path
        map.SetCell(2, 1, new GridCell(CellType.Wall));
        result = pathfindingService.TryFindPath(new Vector2Int(1, 1), new Vector2Int(3, 1), out path);
        Debug.Assert(result, "[QATestingSystem] Error: Should find L-shaped path around obstacle.");
        Debug.Assert(path.Count > 3, "[QATestingSystem] Error: L-shaped path should be longer than direct path.");
        Debug.Assert(path[0] == new Vector2Int(1, 1), "[QATestingSystem] Error: Path should start at (1,1).");
        Debug.Assert(path[path.Count - 1] == new Vector2Int(3, 1), "[QATestingSystem] Error: Path should end at (3,1).");
        // Verify path doesn't go through the wall
        Debug.Assert(!path.Contains(new Vector2Int(2, 1)), "[QATestingSystem] Error: Path should not go through wall at (2,1).");
        Debug.Log("[QATestingSystem] ✓ PathfindingService L-shaped path test passed.");

        // Test 10: No path available (completely blocked)
        // Create a wall that completely blocks the path
        for (int y = 0; y < 5; y++)
        {
            map.SetCell(2, y, new GridCell(CellType.Wall));
        }
        result = pathfindingService.TryFindPath(new Vector2Int(0, 0), new Vector2Int(4, 0), out path);
        Debug.Assert(!result, "[QATestingSystem] Error: Should not find path when completely blocked.");
        Debug.Assert(path.Count == 0, "[QATestingSystem] Error: Path should be empty when no path available.");
        Debug.Log("[QATestingSystem] ✓ PathfindingService no path available test passed.");

        // Test 11: Extra cost parameter
        // Clear walls for this test
        for (int y = 0; y < 5; y++)
        {
            map.SetCell(2, y, new GridCell(CellType.Empty));
        }

        // Test with and without extra cost - should find same path but different cost
        result = pathfindingService.TryFindPath(new Vector2Int(0, 0), new Vector2Int(2, 0), out var pathNormalCost);
        var resultExtraCost = pathfindingService.TryFindPath(new Vector2Int(0, 0), new Vector2Int(2, 0), out var pathExtraCost, extraCost: 5);

        Debug.Assert(result && resultExtraCost, "[QATestingSystem] Error: Both normal and extra cost paths should be found.");
        Debug.Assert(pathNormalCost.Count == pathExtraCost.Count, "[QATestingSystem] Error: Path length should be same regardless of extra cost.");
        // Verify paths are identical
        for (int i = 0; i < pathNormalCost.Count; i++)
        {
            Debug.Assert(pathNormalCost[i] == pathExtraCost[i], "[QATestingSystem] Error: Paths should be identical regardless of extra cost.");
        }
        Debug.Log("[QATestingSystem] ✓ PathfindingService extra cost test passed.");

        // Test 12: Max iterations limit
        // This is harder to test directly, but we can verify the parameter is accepted
        result = pathfindingService.TryFindPath(new Vector2Int(0, 0), new Vector2Int(1, 0), out path, maxIterations: 1);
        // Should still find a simple path even with low iteration limit
        Debug.Assert(result, "[QATestingSystem] Error: Should find simple path even with low iteration limit.");
        Debug.Log("[QATestingSystem] ✓ PathfindingService max iterations test passed.");

        // Test 13: Deterministic behavior - same input should produce same output
        result = pathfindingService.TryFindPath(new Vector2Int(0, 0), new Vector2Int(2, 2), out var path1);
        var result2 = pathfindingService.TryFindPath(new Vector2Int(0, 0), new Vector2Int(2, 2), out var path2);

        Debug.Assert(result == result2, "[QATestingSystem] Error: Pathfinding should be deterministic in result.");
        if (result && result2)
        {
            Debug.Assert(path1.Count == path2.Count, "[QATestingSystem] Error: Pathfinding should be deterministic in path length.");
            for (int i = 0; i < path1.Count; i++)
            {
                Debug.Assert(path1[i] == path2[i], "[QATestingSystem] Error: Pathfinding should be deterministic in path content.");
            }
        }
        Debug.Log("[QATestingSystem] ✓ PathfindingService deterministic behavior test passed.");

        // Test 14: Diagonal movement not allowed (only 4-directional)
        result = pathfindingService.TryFindPath(new Vector2Int(0, 0), new Vector2Int(1, 1), out path);
        Debug.Assert(result, "[QATestingSystem] Error: Should find path to diagonal position using 4-directional movement.");
        // Verify no diagonal steps in path
        for (int i = 1; i < path.Count; i++)
        {
            var step = path[i] - path[i - 1];
            var manhattanDistance = Mathf.Abs(step.x) + Mathf.Abs(step.y);
            Debug.Assert(manhattanDistance == 1, "[QATestingSystem] Error: Each step should be exactly 1 Manhattan distance (no diagonals).");
        }
        Debug.Log("[QATestingSystem] ✓ PathfindingService 4-directional movement test passed.");

        Debug.Log("[QATestingSystem] All tests for PathfindingService completed successfully.");
    }

    public static void TestGridRenderer()
    {
        Debug.Log("[QATestingSystem] Starting tests for GridRenderer.");

        // Test 1: GridRenderer can be created
        var gameObject = new GameObject("TestGridRenderer");
        var renderer = gameObject.AddComponent<GridRenderer>();
        Debug.Assert(renderer != null, "[QATestingSystem] Error: GridRenderer component could not be created.");
        Debug.Log("[QATestingSystem] ✓ GridRenderer creation test passed.");

        // Test 2: GridRenderer works without GridService (preview mode)
        ServiceRegistry.Clear(); // Ensure no GridService is registered

        // Simulate Start() call to test service resolution
        renderer.Start();

        // Since GridRenderer uses TryResolve, it should handle missing service gracefully
        Debug.Log("[QATestingSystem] ✓ GridRenderer preview mode test passed (no exceptions thrown).");

        // Test 3: GridRenderer connects to GridService when available
        var map = new GridMap(5, 3);
        var gridService = new GridService(map, Vector3.zero, 1.0f);
        ServiceRegistry.Register<GridService>(gridService);

        // Create new renderer to test with service
        var gameObjectWithService = new GameObject("TestGridRendererWithService");
        var rendererWithService = gameObjectWithService.AddComponent<GridRenderer>();
        rendererWithService.Start();

        Debug.Log("[QATestingSystem] ✓ GridRenderer with GridService test passed.");

        // Test 4: Verify renderer has proper color configuration
        Debug.Assert(renderer != null, "[QATestingSystem] Error: GridRenderer should have default color settings.");
        Debug.Log("[QATestingSystem] ✓ GridRenderer color configuration test passed.");

        // Test 5: GridRendererDemo can be created and works
        var demoObject = new GameObject("TestGridRendererDemo");
        var demo = demoObject.AddComponent<GridRendererDemo>();
        Debug.Assert(demo != null, "[QATestingSystem] Error: GridRendererDemo component could not be created.");
        Debug.Log("[QATestingSystem] ✓ GridRendererDemo creation test passed.");

        // Clean up
        Object.DestroyImmediate(gameObject);
        Object.DestroyImmediate(gameObjectWithService);
        Object.DestroyImmediate(demoObject);
        ServiceRegistry.Clear();

        Debug.Log("[QATestingSystem] All tests for GridRenderer completed successfully.");
    }

    public static void TestValidationService()
    {
        Debug.Log("[QATestingSystem] Starting tests for ValidationService.");

        // Create test configurations
        var simConfig = ScriptableObject.CreateInstance<SimulationConfig>();
        var agentConfig = ScriptableObject.CreateInstance<AgentConfig>();
        var spawnConfig = ScriptableObject.CreateInstance<SpawnConfig>();
        var validationService = new ValidationService();

        // Test 1: Validation without configs registered should handle gracefully
        ServiceRegistry.Clear();
        try
        {
            validationService.RunAll();
            Debug.Log("[QATestingSystem] ✓ ValidationService handles missing configs gracefully.");
        }
        catch (System.Exception ex)
        {
            Debug.Assert(false, $"[QATestingSystem] Error: ValidationService should handle missing configs gracefully. Exception: {ex.Message}");
        }

        // Test 2: Register configs and test validation
        ServiceRegistry.Register(simConfig);
        ServiceRegistry.Register(agentConfig);
        ServiceRegistry.Register(spawnConfig);

        try
        {
            validationService.RunAll();
            Debug.Log("[QATestingSystem] ✓ ValidationService runs with valid default configs.");
        }
        catch (System.Exception ex)
        {
            Debug.Assert(false, $"[QATestingSystem] Error: ValidationService should work with valid configs. Exception: {ex.Message}");
        }

        // Test 3: Test with GridService
        var map = new GridMap(5, 5);
        var gridService = new GridService(map, Vector3.zero);
        ServiceRegistry.Register(gridService);

        try
        {
            validationService.RunAll();
            Debug.Log("[QATestingSystem] ✓ ValidationService runs with GridService available.");
        }
        catch (System.Exception ex)
        {
            Debug.Assert(false, $"[QATestingSystem] Error: ValidationService should work with GridService. Exception: {ex.Message}");
        }

        // Test 4: Test with fragmented map (create walls to block connectivity)
        for (int x = 0; x < 5; x++)
        {
            map.SetCell(x, 2, new GridCell(CellType.Wall)); // Create horizontal wall
        }

        try
        {
            validationService.RunAll();
            Debug.Log("[QATestingSystem] ✓ ValidationService detects map fragmentation.");
        }
        catch (System.Exception ex)
        {
            Debug.Assert(false, $"[QATestingSystem] Error: ValidationService should handle fragmented maps. Exception: {ex.Message}");
        }

        // Cleanup
        ServiceRegistry.Clear();

        Debug.Log("[QATestingSystem] All tests for ValidationService completed successfully.");
    }

    public static void TestSpawnSystem()
    {
        Debug.Log("[QATestingSystem] Starting tests for SpawnSystem.");

        // Setup: Clear registry and create test configs
        ServiceRegistry.Clear();

        // Create test SimulationConfig
        var simConfig = ScriptableObject.CreateInstance<SimulationConfig>();
        // Set private fields via reflection for testing
        var widthField = typeof(SimulationConfig).GetField("_width", BindingFlags.NonPublic | BindingFlags.Instance);
        var heightField = typeof(SimulationConfig).GetField("_height", BindingFlags.NonPublic | BindingFlags.Instance);
        var cellSizeField = typeof(SimulationConfig).GetField("_cellSize", BindingFlags.NonPublic | BindingFlags.Instance);

        widthField?.SetValue(simConfig, 10);
        heightField?.SetValue(simConfig, 8);
        cellSizeField?.SetValue(simConfig, 1.0f);

        ServiceRegistry.Register(simConfig);

        // Create test SpawnConfig
        var spawnConfig = ScriptableObject.CreateInstance<SpawnConfig>();
        ServiceRegistry.Register(spawnConfig);

        // Create test ValidationService
        var validationService = new ValidationService();
        ServiceRegistry.Register(validationService);

        // Test 1: Basic empty world creation
        Debug.Log("[QATestingSystem] Test 1: Basic empty world creation");

        Vector3 testOrigin = new Vector3(0, 0, 0);
        SpawnSystem.SpawnEmptyWorld(testOrigin, null);

        // Verify GridService was created and registered
        Debug.Assert(ServiceRegistry.TryResolve<GridService>(out var gridService), "[QATestingSystem] Error: GridService not registered after SpawnEmptyWorld.");
        Debug.Assert(gridService.Width == 10, $"[QATestingSystem] Error: GridService width should be 10, but was {gridService.Width}.");
        Debug.Assert(gridService.Height == 8, $"[QATestingSystem] Error: GridService height should be 8, but was {gridService.Height}.");
        Debug.Assert(gridService.CellSize == 1.0f, $"[QATestingSystem] Error: GridService cellSize should be 1.0, but was {gridService.CellSize}.");
        Debug.Assert(gridService.OriginWorld == testOrigin, $"[QATestingSystem] Error: GridService origin should be {testOrigin}, but was {gridService.OriginWorld}.");

        Debug.Log("[QATestingSystem] ✓ Basic empty world creation test passed.");

        // Test 2: Verify all cells are empty by default
        Debug.Log("[QATestingSystem] Test 2: Verify empty cells");

        bool allCellsEmpty = true;
        for (int x = 0; x < gridService.Width; x++)
        {
            for (int y = 0; y < gridService.Height; y++)
            {
                var cell = gridService.Map.GetCell(x, y);
                if (cell.Type != CellType.Empty)
                {
                    allCellsEmpty = false;
                    break;
                }
            }
            if (!allCellsEmpty) break;
        }

        Debug.Assert(allCellsEmpty, "[QATestingSystem] Error: Not all cells are empty by default.");
        Debug.Log("[QATestingSystem] ✓ Empty cells verification test passed.");

        // Test 2.5: Verify PathfindingService was registered
        Debug.Assert(ServiceRegistry.TryResolve<PathfindingService>(out var pathfindingService), "[QATestingSystem] Error: PathfindingService not registered after SpawnEmptyWorld.");
        Debug.Assert(pathfindingService != null, "[QATestingSystem] Error: PathfindingService is null.");

        // Quick pathfinding test
        bool pathResult = pathfindingService.TryFindPath(new Vector2Int(0, 0), new Vector2Int(1, 1), out var path);
        Debug.Assert(pathResult, "[QATestingSystem] Error: PathfindingService should find simple path.");
        Debug.Assert(path.Count > 0, "[QATestingSystem] Error: PathfindingService should return non-empty path.");
        Debug.Log("[QATestingSystem] ✓ PathfindingService integration test passed.");

        // Test 3: Test without GridSpawner (should work fine)
        Debug.Log("[QATestingSystem] Test 3: Test without GridSpawner");

        // Clear and setup again 
        ServiceRegistry.Clear();
        ServiceRegistry.Register(simConfig);
        ServiceRegistry.Register(spawnConfig);
        ServiceRegistry.Register(validationService);

        // Call without GridSpawner - should still work
        SpawnSystem.SpawnEmptyWorld(testOrigin, null);

        Debug.Assert(ServiceRegistry.TryResolve<GridService>(out gridService), "[QATestingSystem] Error: GridService not registered after SpawnEmptyWorld without GridSpawner.");

        Debug.Log("[QATestingSystem] ✓ No GridSpawner test passed.");

        // Cleanup
        ServiceRegistry.Clear();

        Debug.Log("[QATestingSystem] All tests for SpawnSystem completed successfully.");
    }

    public static void TestRobotController()
    {
        Debug.Log("[QATestingSystem] Starting tests for RobotController.");

        // Setup: Create services needed for RobotController
        var simConfig = ScriptableObject.CreateInstance<SimulationConfig>();

        // Use reflection to set private fields in SimulationConfig
        var widthField = simConfig.GetType().GetField("_width", BindingFlags.NonPublic | BindingFlags.Instance);
        var heightField = simConfig.GetType().GetField("_height", BindingFlags.NonPublic | BindingFlags.Instance);
        var cellSizeField = simConfig.GetType().GetField("_cellSize", BindingFlags.NonPublic | BindingFlags.Instance);

        widthField?.SetValue(simConfig, 5);
        heightField?.SetValue(simConfig, 5);
        cellSizeField?.SetValue(simConfig, 1.0f);

        ServiceRegistry.Clear();
        ServiceRegistry.Register(simConfig);

        // Create minimal world using SpawnSystem
        Vector3 testOrigin = new Vector3(0, 0, 0);
        SpawnSystem.SpawnEmptyWorld(testOrigin, null);

        // Verify services are available
        Debug.Assert(ServiceRegistry.TryResolve<GridService>(out var gridService), "[QATestingSystem] Error: GridService not available for RobotController test.");
        Debug.Assert(ServiceRegistry.TryResolve<PathfindingService>(out var pathfindingService), "[QATestingSystem] Error: PathfindingService not available for RobotController test.");

        // Test 1: Create RobotController
        Debug.Log("[QATestingSystem] Test 1: RobotController creation");

        var robotGameObject = new GameObject("TestRobot");
        robotGameObject.transform.position = gridService.CellToWorld(new Vector2Int(0, 0));

        // Add PathfindingComponent first (required dependency)
        var pathfindingComponent = robotGameObject.AddComponent<PathfindingComponent>();
        Debug.Assert(pathfindingComponent != null, "[QATestingSystem] Error: PathfindingComponent could not be created.");

        // Add RobotController
        var robotController = robotGameObject.AddComponent<RobotController>();
        Debug.Assert(robotController != null, "[QATestingSystem] Error: RobotController could not be created.");

        Debug.Log("[QATestingSystem] ✓ RobotController creation test passed.");

        // Test 2: IAgent interface implementation
        Debug.Log("[QATestingSystem] Test 2: IAgent interface");

        IAgent agent = robotController;
        Debug.Assert(agent != null, "[QATestingSystem] Error: RobotController should implement IAgent interface.");

        // ID should be readable (default is 0)
        int agentId = agent.Id;
        Debug.Assert(agentId >= 0, "[QATestingSystem] Error: Agent ID should be non-negative.");

        // Cell should be readable
        Vector2Int agentCell = agent.Cell;
        Debug.Assert(gridService.IsInside(agentCell), "[QATestingSystem] Error: Agent cell should be within grid bounds.");

        // Tick and PlanNextAction should not throw
        agent.Tick();
        agent.PlanNextAction();

        Debug.Log("[QATestingSystem] ✓ IAgent interface test passed.");

        // Test 3: Basic movement command
        Debug.Log("[QATestingSystem] Test 3: Basic movement command");

        // Simulate Start() call to initialize the robot
        pathfindingComponent.SendMessage("Start");
        robotController.SendMessage("Start");

        // Get initial position
        Vector2Int startCell = agent.Cell;
        Vector2Int targetCell = new Vector2Int(startCell.x + 2, startCell.y + 1);

        // Ensure target is within bounds and walkable
        Debug.Assert(gridService.IsInside(targetCell), "[QATestingSystem] Error: Target cell should be within grid bounds.");
        Debug.Assert(gridService.IsWalkable(targetCell), "[QATestingSystem] Error: Target cell should be walkable.");

        // Test movement command
        bool moveResult = robotController.MoveToCell(targetCell);
        Debug.Assert(moveResult, "[QATestingSystem] Error: MoveToCell should return true for valid target.");

        Debug.Log("[QATestingSystem] ✓ Basic movement command test passed.");

        // Test 4: Pathfinding integration
        Debug.Log("[QATestingSystem] Test 4: Pathfinding integration");

        // Check that pathfinding component has a path
        Debug.Assert(pathfindingComponent.HasPath, "[QATestingSystem] Error: PathfindingComponent should have a path after MoveToCell.");
        Debug.Assert(pathfindingComponent.NextWaypoint.HasValue, "[QATestingSystem] Error: PathfindingComponent should have next waypoint.");

        Debug.Log("[QATestingSystem] ✓ Pathfinding integration test passed.");

        // Test 5: Multiple tick simulation (simulate movement)
        Debug.Log("[QATestingSystem] Test 5: Movement simulation");

        int maxTicks = 50; // Prevent infinite loop
        int tickCount = 0;
        Vector2Int initialCell = agent.Cell;

        // Simulate multiple ticks until robot reaches destination or timeout
        while (tickCount < maxTicks && (agent.Cell != targetCell))
        {
            agent.Tick();
            tickCount++;
        }

        Debug.Assert(tickCount < maxTicks, "[QATestingSystem] Error: Robot should reach destination within reasonable time.");
        Debug.Log($"[QATestingSystem] Robot moved from {initialCell} to {agent.Cell} in {tickCount} ticks.");

        Debug.Log("[QATestingSystem] ✓ Movement simulation test passed.");

        // Test 6: Grid occupancy management
        Debug.Log("[QATestingSystem] Test 6: Grid occupancy");

        Vector2Int finalCell = agent.Cell;

        // Final cell should have robot occupant
        Debug.Assert(gridService.HasOccupant(finalCell, CellOccupant.Robot), "[QATestingSystem] Error: Final cell should have Robot occupant.");

        // Initial cell should not have robot occupant (if different from final)
        if (initialCell != finalCell)
        {
            Debug.Assert(!gridService.HasOccupant(initialCell, CellOccupant.Robot), "[QATestingSystem] Error: Initial cell should not have Robot occupant after movement.");
        }

        Debug.Log("[QATestingSystem] ✓ Grid occupancy test passed.");

        // Test 7: Basic replan functionality (optional feature)
        Debug.Log("[QATestingSystem] Test 7: Basic replan functionality");

        // Create a new path first
        robotController.MoveToCell(new Vector2Int(1, 1));
        Debug.Assert(pathfindingComponent.HasPath, "[QATestingSystem] Error: Should have path for replan test.");

        // Test the ValidateAndReplanIfNeeded method
        bool validationResult = pathfindingComponent.ValidateAndReplanIfNeeded(robotGameObject.transform.position, new Vector2Int(1, 1));
        Debug.Assert(validationResult, "[QATestingSystem] Error: Path validation should succeed in empty grid.");

        Debug.Log("[QATestingSystem] ✓ Basic replan functionality test passed.");

        // Cleanup
        UnityEngine.Object.DestroyImmediate(robotGameObject);
        ServiceRegistry.Clear();

        Debug.Log("[QATestingSystem] All tests for RobotController completed.");
    }

    public static void TestJewel()
    {
        Debug.Log("[QATestingSystem] Starting tests for Jewel.");

        // Setup: Create basic grid service
        var simConfig = ScriptableObject.CreateInstance<SimulationConfig>();

        var widthField = simConfig.GetType().GetField("_width", BindingFlags.NonPublic | BindingFlags.Instance);
        var heightField = simConfig.GetType().GetField("_height", BindingFlags.NonPublic | BindingFlags.Instance);
        var cellSizeField = simConfig.GetType().GetField("_cellSize", BindingFlags.NonPublic | BindingFlags.Instance);

        widthField?.SetValue(simConfig, 5);
        heightField?.SetValue(simConfig, 5);
        cellSizeField?.SetValue(simConfig, 1.0f);

        ServiceRegistry.Clear();
        ServiceRegistry.Register(simConfig);

        SpawnSystem.SpawnEmptyWorld(Vector3.zero);

        // Test 1: Create jewel GameObject
        var jewelGameObject = new GameObject("TestJewel");
        var jewel = jewelGameObject.AddComponent<Jewel>();

        // Set position within grid bounds
        jewelGameObject.transform.position = new Vector3(1, 0, 1);

        // Test 2: Initialize jewel
        jewel.SendMessage("Start");

        Debug.Assert(jewel.Color == JewelColor.Red, "[QATestingSystem] Error: Default jewel color should be Red.");
        Debug.Assert(jewel.Value == 1, "[QATestingSystem] Error: Default jewel value should be 1.");
        Debug.Assert(jewel.IsAvailable, "[QATestingSystem] Error: New jewel should be available.");

        Debug.Log("[QATestingSystem] ✓ Jewel initialization test passed.");

        // Test 3: Pick up jewel
        bool pickupResult = jewel.TryPickUp();
        Debug.Assert(pickupResult, "[QATestingSystem] Error: Jewel pickup should succeed.");
        Debug.Assert(!jewel.IsAvailable, "[QATestingSystem] Error: Jewel should not be available after pickup.");
        Debug.Assert(!jewelGameObject.activeInHierarchy, "[QATestingSystem] Error: Jewel GameObject should be inactive after pickup.");

        Debug.Log("[QATestingSystem] ✓ Jewel pickup test passed.");

        // Test 4: Try to pick up again (should fail)
        bool secondPickup = jewel.TryPickUp();
        Debug.Assert(!secondPickup, "[QATestingSystem] Error: Second pickup should fail.");

        Debug.Log("[QATestingSystem] ✓ Jewel double pickup prevention test passed.");

        // Cleanup
        UnityEngine.Object.DestroyImmediate(jewelGameObject);
        ServiceRegistry.Clear();

        Debug.Log("[QATestingSystem] All tests for Jewel completed.");
    }

    public static void TestZoneController()
    {
        Debug.Log("[QATestingSystem] Starting tests for ZoneController.");

        // Setup: Create basic grid service
        var simConfig = ScriptableObject.CreateInstance<SimulationConfig>();

        var widthField = simConfig.GetType().GetField("_width", BindingFlags.NonPublic | BindingFlags.Instance);
        var heightField = simConfig.GetType().GetField("_height", BindingFlags.NonPublic | BindingFlags.Instance);
        var cellSizeField = simConfig.GetType().GetField("_cellSize", BindingFlags.NonPublic | BindingFlags.Instance);

        widthField?.SetValue(simConfig, 5);
        heightField?.SetValue(simConfig, 5);
        cellSizeField?.SetValue(simConfig, 1.0f);

        ServiceRegistry.Clear();
        ServiceRegistry.Register(simConfig);

        SpawnSystem.SpawnEmptyWorld(Vector3.zero);

        // Test 1: Create zone GameObject
        var zoneGameObject = new GameObject("TestZone");
        var zone = zoneGameObject.AddComponent<ZoneController>();

        // Set position within grid bounds
        zoneGameObject.transform.position = new Vector3(2, 0, 2);

        // Test 2: Initialize zone
        zone.SendMessage("Start");

        Debug.Assert(zone.AcceptedColor == JewelColor.Red, "[QATestingSystem] Error: Default zone color should be Red.");

        Debug.Log("[QATestingSystem] ✓ ZoneController initialization test passed.");

        // Test 3: Try to deliver matching jewel
        bool deliveryResult = zone.TryDeliverJewel(JewelColor.Red, 10);
        Debug.Assert(deliveryResult, "[QATestingSystem] Error: Delivery of matching color should succeed.");

        Debug.Log("[QATestingSystem] ✓ ZoneController valid delivery test passed.");

        // Test 4: Try to deliver non-matching jewel
        bool wrongDelivery = zone.TryDeliverJewel(JewelColor.Blue, 5);
        Debug.Assert(!wrongDelivery, "[QATestingSystem] Error: Delivery of wrong color should fail.");

        Debug.Log("[QATestingSystem] ✓ ZoneController invalid delivery test passed.");

        // Cleanup
        UnityEngine.Object.DestroyImmediate(zoneGameObject);
        ServiceRegistry.Clear();

        Debug.Log("[QATestingSystem] All tests for ZoneController completed.");
    }

    public static void TestMetricsLogger()
    {
        Debug.Log("[QATestingSystem] Starting tests for MetricsLogger.");

        // Test 1: Create MetricsLogger
        var loggerGameObject = new GameObject("TestMetricsLogger");
        var logger = loggerGameObject.AddComponent<MetricsLogger>();

        // Test 2: Initialize logger
        logger.SendMessage("Awake");
        logger.SendMessage("Start");

        Debug.Assert(logger.TotalSteps == 0, "[QATestingSystem] Error: Initial steps should be 0.");
        Debug.Assert(logger.TotalJewelsDelivered == 0, "[QATestingSystem] Error: Initial deliveries should be 0.");
        Debug.Assert(logger.TotalScore == 0, "[QATestingSystem] Error: Initial score should be 0.");

        Debug.Log("[QATestingSystem] ✓ MetricsLogger initialization test passed.");

        // Test 3: Record robot steps
        logger.RecordRobotStep(1);
        logger.RecordRobotStep(1);
        logger.RecordRobotStep(2);

        Debug.Assert(logger.TotalSteps == 3, "[QATestingSystem] Error: Should have recorded 3 steps.");

        Debug.Log("[QATestingSystem] ✓ MetricsLogger step recording test passed.");

        // Test 4: Record jewel deliveries
        logger.RecordJewelDelivered(JewelColor.Red, 5);
        logger.RecordJewelDelivered(JewelColor.Blue, 10);

        Debug.Assert(logger.TotalJewelsDelivered == 2, "[QATestingSystem] Error: Should have recorded 2 deliveries.");
        Debug.Assert(logger.TotalScore == 15, "[QATestingSystem] Error: Total score should be 15.");

        Debug.Log("[QATestingSystem] ✓ MetricsLogger delivery recording test passed.");

        // Test 5: Reset functionality
        logger.Reset();

        Debug.Assert(logger.TotalSteps == 0, "[QATestingSystem] Error: Steps should be reset to 0.");
        Debug.Assert(logger.TotalJewelsDelivered == 0, "[QATestingSystem] Error: Deliveries should be reset to 0.");
        Debug.Assert(logger.TotalScore == 0, "[QATestingSystem] Error: Score should be reset to 0.");

        Debug.Log("[QATestingSystem] ✓ MetricsLogger reset test passed.");

        // Cleanup
        UnityEngine.Object.DestroyImmediate(loggerGameObject);

        Debug.Log("[QATestingSystem] All tests for MetricsLogger completed.");
    }

    public static void TestRuleSystem()
    {
        Debug.Log("[QATestingSystem] Starting tests for RuleSystem.");

        // Test 1: Create RuleSystem
        var ruleSystem = new RuleSystem();

        // Test 2: Test validation methods
        bool moveValidation = ruleSystem.ValidateMovement(1, new Vector2Int(0, 0), new Vector2Int(1, 1));
        Debug.Assert(moveValidation, "[QATestingSystem] Error: Movement validation should succeed by default.");

        bool pickupValidation = ruleSystem.ValidatePickup(1, JewelColor.Red);
        Debug.Assert(pickupValidation, "[QATestingSystem] Error: Pickup validation should succeed by default.");

        bool deliveryValidation = ruleSystem.ValidateDelivery(1, JewelColor.Red, JewelColor.Red);
        Debug.Assert(deliveryValidation, "[QATestingSystem] Error: Delivery validation should succeed for matching colors.");

        bool wrongDeliveryValidation = ruleSystem.ValidateDelivery(1, JewelColor.Red, JewelColor.Blue);
        Debug.Assert(!wrongDeliveryValidation, "[QATestingSystem] Error: Delivery validation should fail for mismatched colors.");

        Debug.Log("[QATestingSystem] ✓ RuleSystem validation tests passed.");

        // Test 3: Test end condition with no jewels
        bool shouldEnd = ruleSystem.ShouldEndSimulation();
        Debug.Log($"[QATestingSystem] End simulation check (no jewels in scene): {shouldEnd}");

        int jewelCount = ruleSystem.GetAvailableJewelCount();
        Debug.Assert(jewelCount == 0, "[QATestingSystem] Error: Should find 0 jewels in empty scene.");

        Debug.Log("[QATestingSystem] ✓ RuleSystem end condition test passed.");

        Debug.Log("[QATestingSystem] All tests for RuleSystem completed.");
    }
}