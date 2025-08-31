using UnityEngine;

/// <summary>
/// Simple manual test runner for ValidationService to verify core functionality.
/// </summary>
public class ValidationTestRunner : MonoBehaviour
{
    [Header("Test Configuration")]
    [SerializeField] private bool runTestsOnStart = true;

    private void Start()
    {
        if (runTestsOnStart)
        {
            RunValidationTests();
        }
    }

    public void RunValidationTests()
    {
        Debug.Log("[ValidationTestRunner] Starting manual validation tests...");

        // Test 1: Create configurations with valid values
        var simConfig = ScriptableObject.CreateInstance<SimulationConfig>();
        var agentConfig = ScriptableObject.CreateInstance<AgentConfig>();
        var spawnConfig = ScriptableObject.CreateInstance<SpawnConfig>();
        var validationService = new ValidationService();

        // Register valid configurations
        ServiceRegistry.Clear();
        ServiceRegistry.Register(simConfig);
        ServiceRegistry.Register(agentConfig);
        ServiceRegistry.Register(spawnConfig);

        Debug.Log("[ValidationTestRunner] Running validation with default (valid) configurations...");
        validationService.RunAll();

        // Test 2: Test with GridService
        var map = new GridMap(10, 8);
        var gridService = new GridService(map, Vector3.zero, 1.0f);
        ServiceRegistry.Register(gridService);

        Debug.Log("[ValidationTestRunner] Running validation with GridService...");
        validationService.RunAll();

        // Test 3: Create a fragmented map
        // Add a wall that divides the map
        for (int x = 2; x < 8; x++)
        {
            map.SetCell(x, 4, new GridCell(CellType.Wall));
        }

        Debug.Log("[ValidationTestRunner] Running validation with fragmented map...");
        validationService.RunAll();

        Debug.Log("[ValidationTestRunner] Manual validation tests completed. Check console for validation messages.");
    }
}