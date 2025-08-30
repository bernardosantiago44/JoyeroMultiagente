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
}