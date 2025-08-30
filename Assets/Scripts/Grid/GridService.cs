using UnityEngine;

/// <summary>
/// Fachada de alto nivel sobre el <see cref="GridMap"/> para consultas comunes:
/// ocupación, walkability, conversión mundo↔celda y reservas de celdas.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Consumido por agentes y por <see cref="PathfindingService"/>.
/// - Escrito por <see cref="SpawnSystem"/> al poblar el mundo.
/// </remarks>
public sealed class GridService
{
    public bool IsInside(Vector2Int cell) => false;
    public bool IsWalkable(Vector2Int cell) => false;
    public Vector3 CellToWorld(Vector2Int cell) => default;
    public Vector2Int WorldToCell(Vector3 world) => default;
}
