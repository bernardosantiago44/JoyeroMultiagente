using UnityEngine;

/// <summary>
/// Responsable de poblar el mundo al inicio/reset: crea grid, paredes/estantes,
/// zona(s), joyas y robots, conforme a <see cref="SpawnConfig"/> y <see cref="SimulationConfig"/>.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Escribe en <see cref="GridService"/> y usa <see cref="GridSpawner"/> para instanciar prefabs.
/// - Invocado por <see cref="SimulationManager"/> / <see cref="GameBootstrap"/>.
/// </remarks>
public sealed class SpawnSystem
{
    private const string LOG_PREFIX = "[SpawnSystem]";

    /// <summary>
    /// Construye el mundo base de la simulación: crea GridMap, registra GridService
    /// y opcionalmente poblarlo con elementos estructurales.
    /// </summary>
    /// <param name="origin">Posición del mundo para la celda (0,0)</param>
    /// <param name="gridSpawner">GridSpawner opcional para colocar elementos estructurales</param>
    public static void SpawnEmptyWorld(Vector3 origin, GridSpawner gridSpawner = null)
    {
        Debug.Log($"{LOG_PREFIX} Starting empty world spawn...");

        // 1. Obtener configuraciones necesarias
        if (!ServiceRegistry.TryResolve<SimulationConfig>(out var simConfig))
        {
            Debug.LogError($"{LOG_PREFIX} SimulationConfig not found in ServiceRegistry. Cannot create world.");
            return;
        }

        ServiceRegistry.TryResolve<SpawnConfig>(out var spawnConfig); // Opcional

        // 2. Crear GridMap con dimensiones de SimulationConfig
        var gridMap = new GridMap(simConfig.Width, simConfig.Height);
        Debug.Log($"{LOG_PREFIX} Created GridMap with dimensions {simConfig.Width}x{simConfig.Height}");

        // 3. Crear y registrar GridService
        var gridService = new GridService(gridMap, origin, simConfig.CellSize);
        ServiceRegistry.Register(gridService);
        Debug.Log($"{LOG_PREFIX} GridService registered with origin {origin} and cellSize {simConfig.CellSize}");

        // 4. Crear y registrar PathfindingService
        var pathfindingService = new PathfindingService(gridService);
        ServiceRegistry.Register(pathfindingService);
        Debug.Log($"{LOG_PREFIX} PathfindingService registered and ready for use");

        // 5. Poblar con elementos estructurales si hay GridSpawner disponible
        if (gridSpawner != null && spawnConfig != null)
        {
            PopulateStructuralElements(gridService, spawnConfig, gridSpawner);
        }
        else
        {
            Debug.Log($"{LOG_PREFIX} No GridSpawner or SpawnConfig provided. World remains empty (all cells CellType.Empty)");
        }

        // 6. Ejecutar validaciones Fase B si ValidationService está disponible
        if (ServiceRegistry.TryResolve<ValidationService>(out var validationService))
        {
            Debug.Log($"{LOG_PREFIX} Running Phase B validations...");
            try
            {
                validationService.RunAll();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"{LOG_PREFIX} Phase B validation failed: {ex.Message}");
            }
        }

        // 6. Log de resumen
        LogWorldSummary(gridService);
    }

    /// <summary>
    /// Pobla el mapa con elementos estructurales usando GridSpawner.
    /// </summary>
    private static void PopulateStructuralElements(GridService gridService, SpawnConfig spawnConfig, GridSpawner gridSpawner)
    {
        Debug.Log($"{LOG_PREFIX} Populating structural elements...");

        // Delegar a GridSpawner para la colocación de elementos estructurales
        gridSpawner.PopulateStructuralElements(gridService, spawnConfig);
        
        Debug.Log($"{LOG_PREFIX} Structural elements population completed");
    }

    /// <summary>
    /// Registra un resumen del mundo creado.
    /// </summary>
    private static void LogWorldSummary(GridService gridService)
    {
        int totalCells = gridService.Width * gridService.Height;
        int emptyCells = 0;
        int wallCells = 0;
        int shelfCells = 0;
        int zoneCells = 0;
        int robotSpawnCells = 0;

        // Contar tipos de celdas
        for (int x = 0; x < gridService.Width; x++)
        {
            for (int y = 0; y < gridService.Height; y++)
            {
                var cell = gridService.Map.GetCell(x, y);
                switch (cell.Type)
                {
                    case CellType.Empty:
                        emptyCells++;
                        break;
                    case CellType.Wall:
                        wallCells++;
                        break;
                    case CellType.Shelf:
                        shelfCells++;
                        break;
                    case CellType.Zone:
                        zoneCells++;
                        break;
                    case CellType.RobotSpawn:
                        robotSpawnCells++;
                        break;
                }
            }
        }

        int walkableCells = emptyCells + zoneCells + robotSpawnCells;

        Debug.Log($"{LOG_PREFIX} === WORLD SUMMARY ===");
        Debug.Log($"{LOG_PREFIX} Dimensions: {gridService.Width}x{gridService.Height} ({totalCells} total cells)");
        Debug.Log($"{LOG_PREFIX} Cell size: {gridService.CellSize} units");
        Debug.Log($"{LOG_PREFIX} Origin: {gridService.OriginWorld}");
        Debug.Log($"{LOG_PREFIX} Empty cells: {emptyCells}");
        Debug.Log($"{LOG_PREFIX} Wall cells: {wallCells}");
        Debug.Log($"{LOG_PREFIX} Shelf cells: {shelfCells}");
        Debug.Log($"{LOG_PREFIX} Zone cells: {zoneCells}");
        Debug.Log($"{LOG_PREFIX} Robot spawn cells: {robotSpawnCells}");
        Debug.Log($"{LOG_PREFIX} Walkable cells: {walkableCells} ({walkableCells * 100f / totalCells:F1}%)");
        Debug.Log($"{LOG_PREFIX} === END SUMMARY ===");
    }
}