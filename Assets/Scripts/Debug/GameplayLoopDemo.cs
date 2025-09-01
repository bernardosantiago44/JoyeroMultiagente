using UnityEngine;

/// <summary>
/// Demo script para probar el loop básico de gameplay: recoger y entregar joyas.
/// Muestra cómo usar los nuevos componentes en conjunto.
/// </summary>
public class GameplayLoopDemo : MonoBehaviour
{
    [Header("Demo Configuration")]
    [SerializeField] private bool _runDemo = true;
    [SerializeField] private float _stepDelay = 2.0f;
    
    private RobotController _robot;
    private Jewel _jewel;
    private ZoneController _zone;
    private MetricsLogger _metricsLogger;
    private RuleSystem _ruleSystem;
    
    private enum DemoState
    {
        Setup,
        MoveToJewel,
        PickupJewel,
        MoveToZone,
        DropJewel,
        CheckEndCondition,
        Finished
    }
    
    private DemoState _currentState = DemoState.Setup;
    private float _nextStepTime;
    
    private void Start()
    {
        if (!_runDemo) return;
        
        Debug.Log("[GameplayLoopDemo] Starting gameplay loop demonstration");
        _nextStepTime = Time.time + _stepDelay;
    }
    
    private void Update()
    {
        if (!_runDemo || _currentState == DemoState.Finished) return;
        
        if (Time.time >= _nextStepTime)
        {
            ExecuteNextStep();
            _nextStepTime = Time.time + _stepDelay;
        }
    }
    
    private void ExecuteNextStep()
    {
        switch (_currentState)
        {
            case DemoState.Setup:
                SetupDemo();
                break;
            case DemoState.MoveToJewel:
                MoveToJewel();
                break;
            case DemoState.PickupJewel:
                PickupJewel();
                break;
            case DemoState.MoveToZone:
                MoveToZone();
                break;
            case DemoState.DropJewel:
                DropJewel();
                break;
            case DemoState.CheckEndCondition:
                CheckEndCondition();
                break;
        }
    }
    
    private void SetupDemo()
    {
        Debug.Log("[GameplayLoopDemo] === SETUP PHASE ===");
        
        // Buscar componentes en la escena
        _robot = FindObjectOfType<RobotController>();
        _jewel = FindObjectOfType<Jewel>();
        _zone = FindObjectOfType<ZoneController>();
        
        // Obtener servicios
        ServiceRegistry.TryResolve<MetricsLogger>(out _metricsLogger);
        ServiceRegistry.TryResolve<RuleSystem>(out _ruleSystem);
        
        if (_robot == null)
        {
            Debug.LogError("[GameplayLoopDemo] No robot found in scene!");
            _currentState = DemoState.Finished;
            return;
        }
        
        if (_jewel == null)
        {
            Debug.LogError("[GameplayLoopDemo] No jewel found in scene!");
            _currentState = DemoState.Finished;
            return;
        }
        
        if (_zone == null)
        {
            Debug.LogError("[GameplayLoopDemo] No zone found in scene!");
            _currentState = DemoState.Finished;
            return;
        }
        
        Debug.Log($"[GameplayLoopDemo] Found robot at {_robot.Cell}, jewel at {_jewel.Cell}, zone at {_zone.Cell}");
        Debug.Log($"[GameplayLoopDemo] Jewel color: {_jewel.Color}, Zone accepts: {_zone.AcceptedColor}");
        
        _currentState = DemoState.MoveToJewel;
    }
    
    private void MoveToJewel()
    {
        Debug.Log("[GameplayLoopDemo] === MOVE TO JEWEL PHASE ===");
        
        if (_robot.Cell == _jewel.Cell)
        {
            Debug.Log("[GameplayLoopDemo] Robot is already at jewel position");
            _currentState = DemoState.PickupJewel;
            return;
        }
        
        bool moveStarted = _robot.MoveToCell(_jewel.Cell);
        if (moveStarted)
        {
            Debug.Log($"[GameplayLoopDemo] Robot moving from {_robot.Cell} to jewel at {_jewel.Cell}");
            // Wait for movement to complete before proceeding
            StartCoroutine(WaitForRobotIdle(() => _currentState = DemoState.PickupJewel));
        }
        else
        {
            Debug.LogError("[GameplayLoopDemo] Failed to start movement to jewel");
            _currentState = DemoState.Finished;
        }
    }
    
    private void PickupJewel()
    {
        Debug.Log("[GameplayLoopDemo] === PICKUP JEWEL PHASE ===");
        
        bool pickupSuccess = _robot.TryPickupJewel();
        if (pickupSuccess)
        {
            Debug.Log($"[GameplayLoopDemo] ✓ Robot successfully picked up {_jewel.Color} jewel");
            _currentState = DemoState.MoveToZone;
        }
        else
        {
            Debug.LogError("[GameplayLoopDemo] Failed to pickup jewel");
            _currentState = DemoState.Finished;
        }
    }
    
    private void MoveToZone()
    {
        Debug.Log("[GameplayLoopDemo] === MOVE TO ZONE PHASE ===");
        
        if (_robot.Cell == _zone.Cell)
        {
            Debug.Log("[GameplayLoopDemo] Robot is already at zone position");
            _currentState = DemoState.DropJewel;
            return;
        }
        
        bool moveStarted = _robot.MoveToCell(_zone.Cell);
        if (moveStarted)
        {
            Debug.Log($"[GameplayLoopDemo] Robot moving from {_robot.Cell} to zone at {_zone.Cell}");
            // Wait for movement to complete before proceeding
            StartCoroutine(WaitForRobotIdle(() => _currentState = DemoState.DropJewel));
        }
        else
        {
            Debug.LogError("[GameplayLoopDemo] Failed to start movement to zone");
            _currentState = DemoState.Finished;
        }
    }
    
    private void DropJewel()
    {
        Debug.Log("[GameplayLoopDemo] === DROP JEWEL PHASE ===");
        
        bool dropSuccess = _robot.TryDropJewel();
        if (dropSuccess)
        {
            Debug.Log("[GameplayLoopDemo] ✓ Robot successfully delivered jewel to zone");
            _currentState = DemoState.CheckEndCondition;
        }
        else
        {
            Debug.LogError("[GameplayLoopDemo] Failed to drop jewel at zone");
            _currentState = DemoState.Finished;
        }
    }
    
    private void CheckEndCondition()
    {
        Debug.Log("[GameplayLoopDemo] === CHECK END CONDITION PHASE ===");
        
        if (_ruleSystem != null)
        {
            bool shouldEnd = _ruleSystem.ShouldEndSimulation();
            int availableJewels = _ruleSystem.GetAvailableJewelCount();
            
            Debug.Log($"[GameplayLoopDemo] Available jewels: {availableJewels}");
            Debug.Log($"[GameplayLoopDemo] Should end simulation: {shouldEnd}");
        }
        
        if (_metricsLogger != null)
        {
            Debug.Log($"[GameplayLoopDemo] Total steps: {_metricsLogger.TotalSteps}");
            Debug.Log($"[GameplayLoopDemo] Total deliveries: {_metricsLogger.TotalJewelsDelivered}");
            Debug.Log($"[GameplayLoopDemo] Total score: {_metricsLogger.TotalScore}");
            
            _metricsLogger.ShowFinalReport();
        }
        
        Debug.Log("[GameplayLoopDemo] === DEMO COMPLETED SUCCESSFULLY ===");
        _currentState = DemoState.Finished;
    }
    
    private System.Collections.IEnumerator WaitForRobotIdle(System.Action callback)
    {
        // Wait until robot stops moving
        while (_robot != null && _robot.gameObject.activeSelf)
        {
            // Check if robot is idle (not in MovingToTarget state)
            // This is a simplified check - in a real implementation we'd expose the state
            yield return new WaitForSeconds(0.1f);
            
            // For demo purposes, wait a fixed time
            yield return new WaitForSeconds(1.0f);
            break;
        }
        
        callback?.Invoke();
    }
}