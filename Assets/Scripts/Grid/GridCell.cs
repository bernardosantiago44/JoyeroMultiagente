using UnityEngine;
using System;
/// <summary>
/// Unidad lógica del mapa. Clase pura (sin MonoBehaviour).
/// Contiene el tipo de celda (estático), un costo base de tránsito y
/// una máscara de ocupación para estados dinámicos (robot/joya/zona/reserva).
/// </summary>
/// <remarks>
/// Interacciones:
/// - Será contenida por <see cref="GridMap"/> (Issue #3).
/// - Consultada por <see cref="GridService"/> y <see cref="PathfindingService"/>.
/// - Mutada por sistemas de spawn, movimiento y reserva de celdas.
/// </remarks>
public sealed class GridCell
{
    /// <summary>Tipo "fijo" de la celda (muro, estante, vacía, zona, etc.).</summary>
    public CellType Type { get; private set; }

    /// <summary>Ocupación dinámica como bitmask (robot, joya, zona, reservado).</summary>
    public CellOccupant Occupants { get; private set; }

    /// <summary>
    /// Indica si por tipo la celda es transitable (no muro/estante).
    /// No considera la ocupación dinámica (robots/reservas).
    /// </summary>
    public bool IsWalkableByType =>
        Type != CellType.Wall && Type != CellType.Shelf;

    /// <summary>
    /// Indica si la celda está actualmente bloqueada para moverse (ocupación/reserva).
    /// No evalúa el tipo; para check completo usa <see cref="IsWalkableNow"/>.
    /// </summary>
    public bool IsBlockedByOccupant =>
        Occupants.HasFlag(CellOccupant.Robot) || Occupants.HasFlag(CellOccupant.Reserved);

    /// <summary>
    /// Resultado práctico: ¿se puede entrar a esta celda en este momento?
    /// Considera tipo + ocupación/reserva.
    /// </summary>
    public bool IsWalkableNow => IsWalkableByType; //&& !IsBlockedByOccupant;

    /// <summary>Crea una celda con tipo dado y ocupación opcional.</summary>
    public GridCell(CellType type = CellType.Empty, CellOccupant occupants = CellOccupant.None)
    {
        Type = type;
        Occupants = occupants;
    }

    /// <summary>Cambia el tipo de la celda. No modifica ocupación ni costo.</summary>
    public void SetType(CellType type) => Type = type;

    /// <summary>Agrega un flag de ocupación (bitwise OR).</summary>
    public void AddOccupant(CellOccupant o) => Occupants |= o;

    /// <summary>Remueve un flag de ocupación (bitwise AND NOT).</summary>
    public void RemoveOccupant(CellOccupant o) => Occupants &= ~o;

    /// <summary>True si el flag indicado está presente.</summary>
    public bool HasOccupant(CellOccupant o) => Occupants.HasFlag(o);

    /// <summary>Elimina toda ocupación.</summary>
    public void ClearOccupants() => Occupants = CellOccupant.None;

    public override string ToString()
        => $"GridCell(Type={Type}, WalkableByType={IsWalkableByType}, Occupants={Occupants})";
}

/// <summary>Tipos "estructurales" de la celda.</summary>
public enum CellType
{
    Empty = 0,
    Wall = 1,
    Shelf = 2,
    Jewel = 3, // tipo visual/lógico del terreno; la joya real también será un ocupante
    Zone = 4,
    RobotSpawn = 5
}

/// <summary>Ocupantes dinámicos (bitmask). Pueden coexistir múltiples flags.</summary>
[Flags]
public enum CellOccupant
{
    None     = 0,
    Robot    = 1 << 0, // bloquea paso a otros robots
    Jewel    = 1 << 1, // no bloquea (se puede recoger)
    Zone     = 1 << 2, // no bloquea
    Reserved = 1 << 3  // bloquea temporalmente (mediador de colisiones)
}