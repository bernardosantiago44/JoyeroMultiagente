using UnityEngine;

/// <summary>
/// Representa una joya en el mundo (ítem recolectable).
/// Debe exponer color/tipo/valor (vía campos o SO) y su celda actual.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Es spawneada por <see cref="SpawnSystem"/> / <see cref="GridSpawner"/>.
/// - Se consulta desde <see cref="GridService"/> para ocupación de celdas.
/// - Los robots (implementaciones de <see cref="IAgent"/>) ejecutan "Pick" sobre ella.
/// </remarks>
public class Jewel : MonoBehaviour
{
    
}
