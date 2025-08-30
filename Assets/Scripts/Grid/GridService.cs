using UnityEngine;
using System.Collections.Generic;

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
    /// <summary>Referencia al GridMap subyacente.</summary>
    public GridMap Map { get; }
    
    /// <summary>Ancho del mapa en celdas.</summary>
    public int Width => Map.Width;
    
    /// <summary>Alto del mapa en celdas.</summary>
    public int Height => Map.Height;
    
    /// <summary>Posición del mundo correspondiente al origen del grid (0,0).</summary>
    public Vector3 OriginWorld { get; }
    
    /// <summary>Tamaño de cada celda en unidades del mundo.</summary>
    public float CellSize { get; }

    /// <summary>
    /// Crea un nuevo GridService con los parámetros de conversión especificados.
    /// </summary>
    /// <param name="map">GridMap a usar como fuente de datos</param>
    /// <param name="originWorld">Posición del mundo para la celda (0,0)</param>
    /// <param name="cellSize">Tamaño de cada celda en unidades del mundo (por defecto 1.0)</param>
    public GridService(GridMap map, Vector3 originWorld, float cellSize = 1.0f)
    {
        Map = map ?? throw new System.ArgumentNullException(nameof(map));
        OriginWorld = originWorld;
        CellSize = cellSize;
    }

    /// <summary>
    /// Verifica si las coordenadas de celda están dentro de los límites del mapa.
    /// </summary>
    /// <param name="cell">Coordenadas de la celda</param>
    /// <returns>true si la celda está dentro del mapa</returns>
    public bool IsInside(Vector2Int cell) => Map.InBounds(cell);

    /// <summary>
    /// Verifica si la celda es transitable en este momento.
    /// Considera tanto el tipo de celda como la ocupación dinámica.
    /// </summary>
    /// <param name="cell">Coordenadas de la celda</param>
    /// <returns>true si la celda es transitable ahora</returns>
    public bool IsWalkable(Vector2Int cell)
    {
        if (!IsInside(cell)) return false;
        return Map.GetCell(cell).IsWalkableNow;
    }

    /// <summary>
    /// Obtiene la celda en las coordenadas especificadas de forma segura.
    /// </summary>
    /// <param name="cell">Coordenadas de la celda</param>
    /// <returns>La celda, o null si está fuera del mapa</returns>
    public GridCell GetCell(Vector2Int cell)
    {
        if (!IsInside(cell)) return null;
        return Map.GetCell(cell);
    }

    /// <summary>
    /// Obtiene los vecinos en las 4 direcciones cardinales de la celda especificada.
    /// Solo devuelve coordenadas que están dentro de los límites del mapa.
    /// </summary>
    /// <param name="cell">Coordenadas de la celda central</param>
    /// <returns>Lista de coordenadas de vecinos válidos</returns>
    public IEnumerable<Vector2Int> GetNeighbors4(Vector2Int cell)
    {
        return Map.GetNeighbors4(cell);
    }

    /// <summary>
    /// Convierte coordenadas de celda a posición del mundo en el plano XZ.
    /// El resultado es el centro de la celda.
    /// </summary>
    /// <param name="cell">Coordenadas de la celda</param>
    /// <returns>Posición del mundo en el centro de la celda</returns>
    public Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(
            OriginWorld.x + (cell.x + 0.5f) * CellSize,
            OriginWorld.y,
            OriginWorld.z + (cell.y + 0.5f) * CellSize
        );
    }

    /// <summary>
    /// Convierte posición del mundo a coordenadas de celda.
    /// Usa el plano XZ, ignorando la coordenada Y.
    /// </summary>
    /// <param name="world">Posición del mundo</param>
    /// <returns>Coordenadas de la celda que contiene la posición</returns>
    public Vector2Int WorldToCell(Vector3 world)
    {
        int x = Mathf.FloorToInt((world.x - OriginWorld.x) / CellSize);
        int z = Mathf.FloorToInt((world.z - OriginWorld.z) / CellSize);
        return new Vector2Int(x, z);
    }

    /// <summary>
    /// Agrega un ocupante a la celda especificada.
    /// </summary>
    /// <param name="cell">Coordenadas de la celda</param>
    /// <param name="occupant">Tipo de ocupante a agregar</param>
    /// <returns>true si se pudo agregar (celda válida)</returns>
    public bool AddOccupant(Vector2Int cell, CellOccupant occupant)
    {
        var gridCell = GetCell(cell);
        if (gridCell == null) return false;
        gridCell.AddOccupant(occupant);
        return true;
    }

    /// <summary>
    /// Remueve un ocupante de la celda especificada.
    /// </summary>
    /// <param name="cell">Coordenadas de la celda</param>
    /// <param name="occupant">Tipo de ocupante a remover</param>
    /// <returns>true si se pudo remover (celda válida)</returns>
    public bool RemoveOccupant(Vector2Int cell, CellOccupant occupant)
    {
        var gridCell = GetCell(cell);
        if (gridCell == null) return false;
        gridCell.RemoveOccupant(occupant);
        return true;
    }

    /// <summary>
    /// Verifica si la celda tiene el ocupante especificado.
    /// </summary>
    /// <param name="cell">Coordenadas de la celda</param>
    /// <param name="occupant">Tipo de ocupante a verificar</param>
    /// <returns>true si la celda tiene el ocupante especificado</returns>
    public bool HasOccupant(Vector2Int cell, CellOccupant occupant)
    {
        var gridCell = GetCell(cell);
        if (gridCell == null) return false;
        return gridCell.HasOccupant(occupant);
    }

    /// <summary>
    /// Intenta reservar una celda para prevenir conflictos de movimiento.
    /// </summary>
    /// <param name="cell">Coordenadas de la celda a reservar</param>
    /// <returns>true si se pudo reservar (celda válida y no bloqueada)</returns>
    public bool TryReserve(Vector2Int cell)
    {
        var gridCell = GetCell(cell);
        if (gridCell == null) return false;
        if (gridCell.IsBlockedByOccupant) return false;
        
        gridCell.AddOccupant(CellOccupant.Reserved);
        return true;
    }

    /// <summary>
    /// Libera la reserva de una celda.
    /// </summary>
    /// <param name="cell">Coordenadas de la celda a liberar</param>
    /// <returns>true si se pudo liberar (celda válida)</returns>
    public bool ReleaseReserve(Vector2Int cell)
    {
        var gridCell = GetCell(cell);
        if (gridCell == null) return false;
        gridCell.RemoveOccupant(CellOccupant.Reserved);
        return true;
    }
}
