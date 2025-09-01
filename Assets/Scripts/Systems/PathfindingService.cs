using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implementa A* sobre <see cref="GridService"/>.
/// Clase de lógica pura para ser fácilmente reutilizable.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Consultado por <see cref="PathfindingComponent"/> y, opcionalmente, por <see cref="SimulationManager"/>.
/// - Requiere acceso de solo lectura a <see cref="GridService"/>.
/// </remarks>
public sealed class PathfindingService
{
    private readonly GridService _gridService;
    private const int DefaultMaxIterations = 1000;

    /// <summary>
    /// Crea una nueva instancia del servicio de pathfinding.
    /// </summary>
    /// <param name="gridService">Servicio de grid para consultas de walkability</param>
    public PathfindingService(GridService gridService)
    {
        _gridService = gridService ?? throw new System.ArgumentNullException(nameof(gridService));
    }

    /// <summary>
    /// Busca un camino usando A* con heurística Manhattan.
    /// </summary>
    /// <param name="start">Celda de inicio</param>
    /// <param name="goal">Celda de destino</param>
    /// <param name="outPath">Lista de celdas que forman el camino (incluyendo start y goal)</param>
    /// <param name="extraCost">Costo adicional opcional por celda (por defecto 0)</param>
    /// <param name="maxIterations">Límite máximo de iteraciones (por defecto 1000)</param>
    /// <returns>true si se encontró un camino válido</returns>
    public bool TryFindPath(Vector2Int start, Vector2Int goal, out List<Vector2Int> outPath, 
        int extraCost = 0, int maxIterations = DefaultMaxIterations)
    {
        outPath = new List<Vector2Int>();

        // Edge case: start y goal son iguales
        if (start == goal)
        {
            outPath.Add(start);
            return true;
        }

        // Edge case: coordenadas fuera de límites
        if (!_gridService.IsInside(start) || !_gridService.IsInside(goal))
        {
            return false;
        }

        // Edge case: start o goal no son walkable
        if (!_gridService.IsWalkable(start) || !_gridService.IsWalkable(goal))
        {
            return false;
        }

        return FindPathAStar(start, goal, outPath, extraCost, maxIterations);
    }

    /// <summary>
    /// Implementación del algoritmo A* con 4 direcciones.
    /// </summary>
    private bool FindPathAStar(Vector2Int start, Vector2Int goal, List<Vector2Int> outPath, 
        int extraCost, int maxIterations)
    {
        var openList = new List<PathNode>();
        var closedSet = new HashSet<Vector2Int>();

        var startNode = new PathNode(start, 0, CalculateManhattanDistance(start, goal), null);
        openList.Add(startNode);

        int iterations = 0;

        while (openList.Count > 0 && iterations < maxIterations)
        {
            iterations++;

            // Encuentra el nodo con menor F cost
            var currentNode = GetLowestFCostNode(openList);
            openList.Remove(currentNode);
            closedSet.Add(currentNode.Position);

            // ¿Llegamos al objetivo?
            if (currentNode.Position == goal)
            {
                ReconstructPath(currentNode, outPath);
                return true;
            }

            // Examina vecinos
            foreach (var neighborPos in _gridService.GetNeighbors4(currentNode.Position))
            {
                // Saltar si ya está en la lista cerrada
                if (closedSet.Contains(neighborPos))
                    continue;

                // Saltar si no es walkable
                if (!_gridService.IsWalkable(neighborPos))
                    continue;

                int tentativeGCost = currentNode.GCost + 1 + extraCost; // Movimiento horizontal/vertical cuesta 1
                int hCost = CalculateManhattanDistance(neighborPos, goal);

                // ¿Ya está en la lista abierta?
                var existingNode = openList.Find(n => n.Position == neighborPos);
                if (existingNode != null)
                {
                    // Si encontramos un camino mejor, actualizar
                    if (tentativeGCost < existingNode.GCost)
                    {
                        existingNode.UpdateCosts(tentativeGCost, hCost);
                        existingNode.Parent = currentNode;
                    }
                }
                else
                {
                    // Agregar nuevo nodo a la lista abierta
                    var newNode = new PathNode(neighborPos, tentativeGCost, hCost, currentNode);
                    openList.Add(newNode);
                }
            }
        }

        // No se encontró camino
        return false;
    }

    /// <summary>
    /// Encuentra el nodo con menor F cost en la lista abierta.
    /// </summary>
    private PathNode GetLowestFCostNode(List<PathNode> openList)
    {
        PathNode lowest = openList[0];
        for (int i = 1; i < openList.Count; i++)
        {
            var node = openList[i];
            if (node.FCost < lowest.FCost || 
                (node.FCost == lowest.FCost && node.HCost < lowest.HCost))
            {
                lowest = node;
            }
        }
        return lowest;
    }

    /// <summary>
    /// Calcula la distancia Manhattan entre dos puntos.
    /// </summary>
    private int CalculateManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// Reconstruye el camino desde el nodo objetivo hasta el inicio.
    /// </summary>
    private void ReconstructPath(PathNode goalNode, List<Vector2Int> outPath)
    {
        var pathStack = new Stack<Vector2Int>();
        var current = goalNode;

        while (current != null)
        {
            pathStack.Push(current.Position);
            current = current.Parent;
        }

        while (pathStack.Count > 0)
        {
            outPath.Add(pathStack.Pop());
        }
    }

    /// <summary>
    /// Nodo interno para el algoritmo A*.
    /// </summary>
    private class PathNode
    {
        /// <summary>Posición de la celda</summary>
        public Vector2Int Position { get; }

        /// <summary>Costo desde el inicio</summary>
        public int GCost { get; private set; }

        /// <summary>Distancia heurística al objetivo</summary>
        public int HCost { get; private set; }

        /// <summary>Costo total (G + H)</summary>
        public int FCost => GCost + HCost;

        /// <summary>Nodo padre para reconstruir el camino</summary>
        public PathNode Parent { get; set; }

        /// <summary>
        /// Crea un nuevo nodo de pathfinding.
        /// </summary>
        public PathNode(Vector2Int position, int gCost, int hCost, PathNode parent)
        {
            Position = position;
            GCost = gCost;
            HCost = hCost;
            Parent = parent;
        }

        /// <summary>
        /// Actualiza los costos del nodo.
        /// </summary>
        public void UpdateCosts(int gCost, int hCost)
        {
            GCost = gCost;
            HCost = hCost;
        }
    }
}