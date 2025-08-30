using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Modelo lógico del mundo en celdas (ancho/alto, acceso a celdas, vecinos).
/// No depende de MonoBehaviour; pensado para lógica y tests unitarios.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Fuente de datos para <see cref="GridService"/> y <see cref="PathfindingService"/>.
/// - Es inicializado por <see cref="SpawnSystem"/> / <see cref="GridSpawner"/>.
/// </remarks>
public sealed class GridMap
{
    private readonly GridCell[,] cells;
    
    /// <summary>Ancho del mapa en celdas.</summary>
    public int Width { get; }
    
    /// <summary>Alto del mapa en celdas.</summary>
    public int Height { get; }

    /// <summary>
    /// Crea un nuevo GridMap con las dimensiones especificadas.
    /// Todas las celdas se inicializan como Empty por defecto.
    /// </summary>
    /// <param name="width">Ancho del mapa en celdas (debe ser > 0)</param>
    /// <param name="height">Alto del mapa en celdas (debe ser > 0)</param>
    public GridMap(int width, int height)
    {
        if (width <= 0) throw new System.ArgumentException("Width must be greater than 0", nameof(width));
        if (height <= 0) throw new System.ArgumentException("Height must be greater than 0", nameof(height));
        
        Width = width;
        Height = height;
        cells = new GridCell[width, height];
        
        // Inicializar todas las celdas como Empty
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells[x, y] = new GridCell(CellType.Empty);
            }
        }
    }

    /// <summary>
    /// Verifica si las coordenadas están dentro de los límites del mapa.
    /// </summary>
    /// <param name="x">Coordenada X</param>
    /// <param name="y">Coordenada Y</param>
    /// <returns>true si las coordenadas están dentro del mapa</returns>
    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    /// <summary>
    /// Verifica si las coordenadas están dentro de los límites del mapa.
    /// </summary>
    /// <param name="coords">Coordenadas como Vector2Int</param>
    /// <returns>true si las coordenadas están dentro del mapa</returns>
    public bool InBounds(Vector2Int coords)
    {
        return InBounds(coords.x, coords.y);
    }

    /// <summary>
    /// Obtiene la celda en las coordenadas especificadas.
    /// </summary>
    /// <param name="x">Coordenada X</param>
    /// <param name="y">Coordenada Y</param>
    /// <returns>La celda en las coordenadas especificadas</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">Si las coordenadas están fuera del mapa</exception>
    public GridCell GetCell(int x, int y)
    {
        if (!InBounds(x, y))
            throw new System.ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are out of bounds for map size {Width}x{Height}");
        
        return cells[x, y];
    }

    /// <summary>
    /// Obtiene la celda en las coordenadas especificadas.
    /// </summary>
    /// <param name="coords">Coordenadas como Vector2Int</param>
    /// <returns>La celda en las coordenadas especificadas</returns>
    /// <exception cref="System.ArgumentOutOfRangeException">Si las coordenadas están fuera del mapa</exception>
    public GridCell GetCell(Vector2Int coords)
    {
        return GetCell(coords.x, coords.y);
    }

    /// <summary>
    /// Establece la celda en las coordenadas especificadas.
    /// </summary>
    /// <param name="x">Coordenada X</param>
    /// <param name="y">Coordenada Y</param>
    /// <param name="cell">La celda a establecer</param>
    /// <exception cref="System.ArgumentOutOfRangeException">Si las coordenadas están fuera del mapa</exception>
    /// <exception cref="System.ArgumentNullException">Si la celda es null</exception>
    public void SetCell(int x, int y, GridCell cell)
    {
        if (!InBounds(x, y))
            throw new System.ArgumentOutOfRangeException($"Coordinates ({x}, {y}) are out of bounds for map size {Width}x{Height}");
        
        if (cell == null)
            throw new System.ArgumentNullException(nameof(cell));
        
        cells[x, y] = cell;
    }

    /// <summary>
    /// Establece la celda en las coordenadas especificadas.
    /// </summary>
    /// <param name="coords">Coordenadas como Vector2Int</param>
    /// <param name="cell">La celda a establecer</param>
    /// <exception cref="System.ArgumentOutOfRangeException">Si las coordenadas están fuera del mapa</exception>
    /// <exception cref="System.ArgumentNullException">Si la celda es null</exception>
    public void SetCell(Vector2Int coords, GridCell cell)
    {
        SetCell(coords.x, coords.y, cell);
    }

    /// <summary>
    /// Obtiene los vecinos en las 4 direcciones cardinales (arriba, abajo, izquierda, derecha).
    /// Solo devuelve coordenadas que están dentro de los límites del mapa.
    /// </summary>
    /// <param name="x">Coordenada X del centro</param>
    /// <param name="y">Coordenada Y del centro</param>
    /// <returns>Lista de coordenadas de vecinos válidos</returns>
    public List<Vector2Int> GetNeighbors4(int x, int y)
    {
        var neighbors = new List<Vector2Int>();
        
        // Direcciones: arriba, derecha, abajo, izquierda
        var directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // arriba
            new Vector2Int(1, 0),   // derecha  
            new Vector2Int(0, -1),  // abajo
            new Vector2Int(-1, 0)   // izquierda
        };

        foreach (var dir in directions)
        {
            int nx = x + dir.x;
            int ny = y + dir.y;
            
            if (InBounds(nx, ny))
            {
                neighbors.Add(new Vector2Int(nx, ny));
            }
        }

        return neighbors;
    }

    /// <summary>
    /// Obtiene los vecinos en las 4 direcciones cardinales (arriba, abajo, izquierda, derecha).
    /// Solo devuelve coordenadas que están dentro de los límites del mapa.
    /// </summary>
    /// <param name="coords">Coordenadas del centro como Vector2Int</param>
    /// <returns>Lista de coordenadas de vecinos válidos</returns>
    public List<Vector2Int> GetNeighbors4(Vector2Int coords)
    {
        return GetNeighbors4(coords.x, coords.y);
    }
}
