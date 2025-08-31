using UnityEngine;

[CreateAssetMenu(fileName = "AgentConfig", menuName = "Scriptable Objects/AgentConfig")]
/// <summary>
/// Configuración por agente (velocidad, capacidad, FOV, tiempos de acción).
/// Editable en el inspector como ScriptableObject para tuning sin recompilar.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Consumida por <see cref="RobotController"/> y componentes de navegación/percepción.
/// - Cargada por <see cref="GameBootstrap"/>.
/// </remarks>
public sealed class AgentConfig : ScriptableObject
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2.0f;
    
    [Header("Capacity Settings")]
    [SerializeField] private int _carryCapacity = 1;

    [Header("Behavior Settings")]
    [SerializeField] private float _actionDelay = 0.5f;

    /// <summary>Velocidad de movimiento del robot en unidades por segundo</summary>
    public float MoveSpeed => _moveSpeed;

    /// <summary>Capacidad de carga de joyas del robot</summary>
    public int CarryCapacity => _carryCapacity;

    /// <summary>Retraso entre acciones en segundos</summary>
    public float ActionDelay => _actionDelay;
}