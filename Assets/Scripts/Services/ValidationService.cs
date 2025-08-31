using UnityEngine;

/// <summary>
/// Ejecuta validaciones de arranque y consistencia del mundo:
/// conectividad básica, densidades válidas, posiciones no solapadas, etc.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Invocado por <see cref="GameBootstrap"/> tras el spawning.
/// - Puede usar <see cref="GridService"/> para inspección y emitir advertencias/errores.
/// </remarks>
public sealed class ValidationService
{
    private const string LOG_PREFIX = "[Validation]";

    /// <summary>
    /// Ejecuta todas las validaciones disponibles y reporta resultados.
    /// Valida configuraciones (Fase A) y opcionalmente el mapa (Fase B).
    /// </summary>
    public void RunAll()
    {
        Debug.Log($"{LOG_PREFIX} Starting validation checks...");

        bool hasErrors = false;
        bool hasWarnings = false;

        // Phase A: Configuration validation
        hasErrors |= !ValidateSimulationConfig();
        hasErrors |= !ValidateAgentConfig();
        hasErrors |= !ValidateSpawnConfig();

        // Phase B: Map validation (if GridService is available)
        if (ServiceRegistry.TryResolve<GridService>(out var gridService))
        {
            var mapValidation = ValidateGridMap(gridService);
            hasErrors |= !mapValidation.isValid;
            hasWarnings |= mapValidation.hasWarnings;
        }
        else
        {
            Debug.Log($"{LOG_PREFIX} INFO: GridService not available, skipping map validation");
        }

        // Final summary
        if (hasErrors)
        {
            Debug.LogError($"{LOG_PREFIX} VALIDATION FAILED: Critical errors found that may prevent proper simulation");
        }
        else if (hasWarnings)
        {
            Debug.LogWarning($"{LOG_PREFIX} VALIDATION PASSED: No critical errors, but warnings found");
        }
        else
        {
            Debug.Log($"{LOG_PREFIX} INFO: All validation checks passed successfully");
        }
    }

    /// <summary>
    /// Valida la configuración de simulación.
    /// </summary>
    /// <returns>true si la validación es exitosa, false si hay errores críticos</returns>
    private bool ValidateSimulationConfig()
    {
        if (!ServiceRegistry.TryResolve<SimulationConfig>(out var config))
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: SimulationConfig not found in ServiceRegistry");
            return false;
        }

        bool isValid = true;

        // Validate dimensions
        if (config.Width <= 0)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: SimulationConfig.Width must be > 0 (actual: {config.Width})");
            isValid = false;
        }

        if (config.Height <= 0)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: SimulationConfig.Height must be > 0 (actual: {config.Height})");
            isValid = false;
        }

        // Validate cell size
        if (config.CellSize <= 0)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: SimulationConfig.CellSize must be > 0 (actual: {config.CellSize})");
            isValid = false;
        }

        // Validate max time
        if (config.MaxTime <= 0)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: SimulationConfig.MaxTime must be > 0 (actual: {config.MaxTime})");
            isValid = false;
        }

        // Check minimum dimensions
        if (config.Width < config.MinWidth)
        {
            Debug.LogWarning($"{LOG_PREFIX} WARNING: SimulationConfig.Width {config.Width} is below recommended minimum {config.MinWidth}");
        }

        if (config.Height < config.MinHeight)
        {
            Debug.LogWarning($"{LOG_PREFIX} WARNING: SimulationConfig.Height {config.Height} is below recommended minimum {config.MinHeight}");
        }

        Debug.Log($"{LOG_PREFIX} INFO: SimulationConfig validation completed (Width: {config.Width}, Height: {config.Height}, CellSize: {config.CellSize})");
        return isValid;
    }

    /// <summary>
    /// Valida la configuración de agentes.
    /// </summary>
    /// <returns>true si la validación es exitosa, false si hay errores críticos</returns>
    private bool ValidateAgentConfig()
    {
        if (!ServiceRegistry.TryResolve<AgentConfig>(out var config))
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: AgentConfig not found in ServiceRegistry");
            return false;
        }

        bool isValid = true;

        // Validate move speed
        if (config.MoveSpeed <= 0)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: AgentConfig.MoveSpeed must be > 0 (actual: {config.MoveSpeed})");
            isValid = false;
        }

        // Validate carry capacity
        if (config.CarryCapacity < 0)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: AgentConfig.CarryCapacity must be >= 0 (actual: {config.CarryCapacity})");
            isValid = false;
        }

        // Warning for zero capacity
        if (config.CarryCapacity == 0)
        {
            Debug.LogWarning($"{LOG_PREFIX} WARNING: AgentConfig.CarryCapacity is 0, robots won't be able to carry jewels");
        }

        Debug.Log($"{LOG_PREFIX} INFO: AgentConfig validation completed (MoveSpeed: {config.MoveSpeed}, CarryCapacity: {config.CarryCapacity})");
        return isValid;
    }

    /// <summary>
    /// Valida la configuración de spawn.
    /// </summary>
    /// <returns>true si la validación es exitosa, false si hay errores críticos</returns>
    private bool ValidateSpawnConfig()
    {
        if (!ServiceRegistry.TryResolve<SpawnConfig>(out var config))
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: SpawnConfig not found in ServiceRegistry");
            return false;
        }

        bool isValid = true;

        // Validate number of robots
        if (config.NumRobots < 1)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: SpawnConfig.NumRobots must be >= 1 (actual: {config.NumRobots})");
            isValid = false;
        }

        // Validate number of jewels
        if (config.NumJewels < 0)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: SpawnConfig.NumJewels must be >= 0 (actual: {config.NumJewels})");
            isValid = false;
        }

        // Validate obstacle density
        if (config.ObstacleDensity < 0.0f || config.ObstacleDensity > 1.0f)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: SpawnConfig.ObstacleDensity must be between 0.0 and 1.0 (actual: {config.ObstacleDensity})");
            isValid = false;
        }

        // Get obstacle density threshold from SimulationConfig
        if (ServiceRegistry.TryResolve<SimulationConfig>(out var simConfig))
        {
            if (config.ObstacleDensity > simConfig.ObstacleDensityMax)
            {
                Debug.LogWarning($"{LOG_PREFIX} WARNING: Obstacle density {config.ObstacleDensity:F2} exceeds recommended max {simConfig.ObstacleDensityMax:F2}");
            }

            if (config.ObstacleDensity > 0.60f)
            {
                Debug.LogWarning($"{LOG_PREFIX} WARNING: Very high obstacle density {config.ObstacleDensity:F2} may severely limit robot movement");
            }
        }

        // Validate jewel colors
        if (config.JewelColors == null || config.JewelColors.Length == 0)
        {
            Debug.LogWarning($"{LOG_PREFIX} WARNING: SpawnConfig.JewelColors is empty, no colors available for jewels");
        }

        Debug.Log($"{LOG_PREFIX} INFO: SpawnConfig validation completed (NumRobots: {config.NumRobots}, NumJewels: {config.NumJewels}, ObstacleDensity: {config.ObstacleDensity:F2})");
        return isValid;
    }

    /// <summary>
    /// Valida el mapa del grid si está disponible.
    /// </summary>
    /// <param name="gridService">Servicio de grid a validar</param>
    /// <returns>Resultado de validación con flags de errores y warnings</returns>
    private (bool isValid, bool hasWarnings) ValidateGridMap(GridService gridService)
    {
        if (gridService?.Map == null)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: GridService.Map is null");
            return (false, false);
        }

        bool isValid = true;
        bool hasWarnings = false;

        var map = gridService.Map;

        // Validate map dimensions match config
        if (ServiceRegistry.TryResolve<SimulationConfig>(out var simConfig))
        {
            if (map.Width != simConfig.Width)
            {
                Debug.LogError($"{LOG_PREFIX} ERROR: GridMap width {map.Width} doesn't match SimulationConfig width {simConfig.Width}");
                isValid = false;
            }

            if (map.Height != simConfig.Height)
            {
                Debug.LogError($"{LOG_PREFIX} ERROR: GridMap height {map.Height} doesn't match SimulationConfig height {simConfig.Height}");
                isValid = false;
            }
        }

        // Count walkable cells and calculate density
        int totalCells = map.Width * map.Height;
        int walkableCells = 0;
        int obstacleCells = 0;

        for (int x = 0; x < map.Width; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                var cell = map.GetCell(x, y);
                if (cell.IsWalkableByType)
                {
                    walkableCells++;
                }
                else if (cell.Type == CellType.Wall || cell.Type == CellType.Shelf)
                {
                    obstacleCells++;
                }
            }
        }

        // Validate basic walkability
        if (walkableCells == 0)
        {
            Debug.LogError($"{LOG_PREFIX} ERROR: No walkable cells found in the map");
            isValid = false;
        }
        else if (walkableCells < totalCells * 0.1f) // Less than 10% walkable
        {
            Debug.LogWarning($"{LOG_PREFIX} WARNING: Very few walkable cells ({walkableCells}/{totalCells} = {(walkableCells * 100.0f / totalCells):F1}%)");
            hasWarnings = true;
        }

        // Validate obstacle density
        float actualObstacleDensity = (float)obstacleCells / totalCells;
        if (ServiceRegistry.TryResolve<SimulationConfig>(out var config))
        {
            if (actualObstacleDensity > config.ObstacleDensityMax)
            {
                Debug.LogWarning($"{LOG_PREFIX} WARNING: Actual obstacle density {actualObstacleDensity:F2} exceeds configured max {config.ObstacleDensityMax:F2}");
                hasWarnings = true;
            }
        }

        // Basic connectivity check (simplified flood fill from first walkable cell)
        if (walkableCells > 1)
        {
            int reachableCells = CountReachableCells(map);
            if (reachableCells < walkableCells)
            {
                Debug.LogWarning($"{LOG_PREFIX} WARNING: Map may be fragmented - only {reachableCells}/{walkableCells} walkable cells are reachable");
                hasWarnings = true;
            }
        }

        Debug.Log($"{LOG_PREFIX} INFO: GridMap({map.Width}x{map.Height}) OK. Walkable: {walkableCells}/{totalCells} ({(walkableCells * 100.0f / totalCells):F1}%)");
        return (isValid, hasWarnings);
    }

    /// <summary>
    /// Cuenta las celdas alcanzables usando una búsqueda simple desde la primera celda transitable.
    /// </summary>
    private int CountReachableCells(GridMap map)
    {
        // Find first walkable cell
        Vector2Int start = new Vector2Int(-1, -1);
        for (int x = 0; x < map.Width && start.x == -1; x++)
        {
            for (int y = 0; y < map.Height; y++)
            {
                if (map.GetCell(x, y).IsWalkableByType)
                {
                    start = new Vector2Int(x, y);
                    break;
                }
            }
        }

        if (start.x == -1) return 0; // No walkable cells

        // Simple flood fill
        var visited = new bool[map.Width, map.Height];
        var queue = new System.Collections.Generic.Queue<Vector2Int>();
        queue.Enqueue(start);
        visited[start.x, start.y] = true;
        int count = 1;

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            var neighbors = map.GetNeighbors4(current.x, current.y);

            foreach (var neighbor in neighbors)
            {
                if (!visited[neighbor.x, neighbor.y] && map.GetCell(neighbor.x, neighbor.y).IsWalkableByType)
                {
                    visited[neighbor.x, neighbor.y] = true;
                    queue.Enqueue(neighbor);
                    count++;
                }
            }
        }

        return count;
    }
}
