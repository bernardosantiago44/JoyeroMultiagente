using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Orquestador principal de estados de la simulación (Init/Running/Paused/Finished),
/// controla el loop de ticks y los reseteos/replays.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Coordina <see cref="SpawnSystem"/>, <see cref="RuleSystem"/> y notifica a UI.
/// - Itera <see cref="IAgent.Tick"/> y decide cuándo pedir <see cref="IAgent.PlanNextAction"/>.
/// </remarks>
public sealed class SimulationManager : MonoBehaviour
{
    [SerializeField] private float _tickInterval = 0.5f; // Segundos entre ticks
    
    private RuleSystem _ruleSystem;
    private MetricsLogger _metricsLogger;
    private SimulationState _currentState = SimulationState.Init;
    private float _lastTickTime;
    private List<IAgent> _agents = new List<IAgent>();
    
    /// <summary>Estados de la simulación</summary>
    public enum SimulationState
    {
        Init,
        Running,
        Paused,
        Finished
    }
    
    /// <summary>Estado actual de la simulación</summary>
    public SimulationState CurrentState => _currentState;
    
    private void Start()
    {
        // Obtener servicios
        ServiceRegistry.TryResolve<MetricsLogger>(out _metricsLogger);
        
        // Crear RuleSystem
        _ruleSystem = new RuleSystem();
        ServiceRegistry.Register(_ruleSystem);
        
        Debug.Log("[SimulationManager] Simulation Manager initialized");
        
        // Iniciar la simulación
        StartSimulation();
    }
    
    private void Update()
    {
        if (_currentState == SimulationState.Running)
        {
            // Verificar si es hora del siguiente tick
            if (Time.time - _lastTickTime >= _tickInterval)
            {
                SimulationTick();
                _lastTickTime = Time.time;
            }
        }
    }
    
    /// <summary>
    /// Inicia la simulación.
    /// </summary>
    public void StartSimulation()
    {
        if (_currentState != SimulationState.Init && _currentState != SimulationState.Finished)
        {
            Debug.LogWarning("[SimulationManager] Cannot start simulation from current state: " + _currentState);
            return;
        }
        
        // Buscar todos los agentes en la escena
        RefreshAgentList();
        
        _currentState = SimulationState.Running;
        _lastTickTime = Time.time;
        
        Debug.Log($"[SimulationManager] Simulation started with {_agents.Count} agents");
    }
    
    /// <summary>
    /// Pausa la simulación.
    /// </summary>
    public void PauseSimulation()
    {
        if (_currentState == SimulationState.Running)
        {
            _currentState = SimulationState.Paused;
            Debug.Log("[SimulationManager] Simulation paused");
        }
    }
    
    /// <summary>
    /// Reanuda la simulación.
    /// </summary>
    public void ResumeSimulation()
    {
        if (_currentState == SimulationState.Paused)
        {
            _currentState = SimulationState.Running;
            _lastTickTime = Time.time;
            Debug.Log("[SimulationManager] Simulation resumed");
        }
    }
    
    /// <summary>
    /// Termina la simulación y muestra el reporte final.
    /// </summary>
    public void EndSimulation()
    {
        if (_currentState == SimulationState.Finished)
            return;
            
        _currentState = SimulationState.Finished;
        
        Debug.Log("[SimulationManager] Simulation ended");
        
        // Mostrar reporte final
        if (_metricsLogger != null)
        {
            _metricsLogger.ShowFinalReport();
        }
    }
    
    /// <summary>
    /// Ejecuta un tick de simulación.
    /// </summary>
    private void SimulationTick()
    {
        // Verificar condición de fin
        if (_ruleSystem != null && _ruleSystem.ShouldEndSimulation())
        {
            EndSimulation();
            return;
        }
        
        // Actualizar todos los agentes
        foreach (var agent in _agents)
        {
            if (agent != null)
            {
                agent.Tick();
                agent.PlanNextAction();
            }
        }
        
        // Log de estado cada ciertos ticks para debug
        if (Time.frameCount % 100 == 0) // Cada ~100 frames
        {
            int availableJewels = _ruleSystem?.GetAvailableJewelCount() ?? 0;
            Debug.Log($"[SimulationManager] Tick - Available jewels: {availableJewels}");
        }
    }
    
    /// <summary>
    /// Actualiza la lista de agentes buscándolos en la escena.
    /// </summary>
    private void RefreshAgentList()
    {
        _agents.Clear();
        
        // Buscar todos los objetos que implementen IAgent
        var robotControllers = Object.FindObjectsOfType<RobotController>();
        foreach (var robot in robotControllers)
        {
            _agents.Add(robot);
        }
        
        Debug.Log($"[SimulationManager] Found {_agents.Count} agents in scene");
    }
}