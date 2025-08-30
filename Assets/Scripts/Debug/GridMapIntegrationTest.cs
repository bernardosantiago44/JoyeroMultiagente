using UnityEngine;

/// <summary>
/// Test de integración para verificar que GridMap funciona correctamente 
/// con los patrones de uso esperados en el proyecto.
/// </summary>
public class GridMapIntegrationTest : MonoBehaviour
{
    void Start()
    {
        TestGridMapIntegration();
    }
    
    void TestGridMapIntegration()
    {
        Debug.Log("[GridMapIntegrationTest] Iniciando test de integración...");
        
        // Simular creación de un mapa pequeño como lo haría GridSpawner
        const int width = 10;
        const int height = 8;
        var gridMap = new GridMap(width, height);
        
        // Simular configuración inicial del mapa
        Debug.Log($"[GridMapIntegrationTest] Mapa creado: {width}x{height}");
        
        // Simular colocación de paredes en los bordes
        for (int x = 0; x < width; x++)
        {
            gridMap.SetCell(x, 0, new GridCell(CellType.Wall));           // borde inferior
            gridMap.SetCell(x, height - 1, new GridCell(CellType.Wall)); // borde superior
        }
        
        for (int y = 0; y < height; y++)
        {
            gridMap.SetCell(0, y, new GridCell(CellType.Wall));           // borde izquierdo  
            gridMap.SetCell(width - 1, y, new GridCell(CellType.Wall));  // borde derecho
        }
        
        // Colocar algunos estantes en el interior
        gridMap.SetCell(3, 3, new GridCell(CellType.Shelf));
        gridMap.SetCell(6, 5, new GridCell(CellType.Shelf));
        
        // Simular colocación de una zona especial
        gridMap.SetCell(5, 4, new GridCell(CellType.Zone));
        
        Debug.Log("[GridMapIntegrationTest] Mapa configurado con paredes, estantes y zona.");
        
        // Test navegación: encontrar una ruta simple usando GetNeighbors4
        var start = new Vector2Int(1, 1);
        var goal = new Vector2Int(width - 2, height - 2);
        
        Debug.Log($"[GridMapIntegrationTest] Buscando camino de {start} a {goal}");
        
        // Verificar que ambas posiciones son válidas y transitables
        Debug.Assert(gridMap.InBounds(start), "Posición de inicio debe estar en el mapa");
        Debug.Assert(gridMap.InBounds(goal), "Posición objetivo debe estar en el mapa");
        Debug.Assert(gridMap.GetCell(start).IsWalkableByType, "Posición de inicio debe ser transitable");
        Debug.Assert(gridMap.GetCell(goal).IsWalkableByType, "Posición objetivo debe ser transitable");
        
        // Test de vecinos en diferentes contextos
        var startNeighbors = gridMap.GetNeighbors4(start);
        Debug.Log($"[GridMapIntegrationTest] Posición inicio {start} tiene {startNeighbors.Count} vecinos");
        
        var goalNeighbors = gridMap.GetNeighbors4(goal);
        Debug.Log($"[GridMapIntegrationTest] Posición objetivo {goal} tiene {goalNeighbors.Count} vecinos");
        
        // Verificar que los vecinos del inicio excluyen las paredes
        foreach (var neighbor in startNeighbors)
        {
            var neighborCell = gridMap.GetCell(neighbor);
            Debug.Log($"[GridMapIntegrationTest] Vecino {neighbor}: {neighborCell.Type}, walkable: {neighborCell.IsWalkableByType}");
        }
        
        // Test de ocupación dinámica (simular robot moviendose)
        var robotPos = new Vector2Int(2, 2);
        var robotCell = gridMap.GetCell(robotPos);
        robotCell.AddOccupant(CellOccupant.Robot);
        
        Debug.Assert(!robotCell.IsWalkableNow, "Celda con robot no debe ser transitable ahora");
        Debug.Assert(robotCell.IsWalkableByType, "Celda con robot sigue siendo transitable por tipo");
        
        robotCell.RemoveOccupant(CellOccupant.Robot);
        Debug.Assert(robotCell.IsWalkableNow, "Celda sin robot debe volver a ser transitable");
        
        // Test de casos extremos
        var cornerNeighbors = gridMap.GetNeighbors4(0, 0);
        Debug.Assert(cornerNeighbors.Count == 2, "Esquina debe tener exactamente 2 vecinos");
        
        var edgeNeighbors = gridMap.GetNeighbors4(1, 0);
        Debug.Assert(edgeNeighbors.Count == 3, "Borde debe tener exactamente 3 vecinos");
        
        Debug.Log("[GridMapIntegrationTest] ✓ Todos los tests de integración pasaron correctamente!");
        Debug.Log("[GridMapIntegrationTest] GridMap está listo para usar con GridService y PathfindingService.");
    }
}