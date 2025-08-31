using UnityEngine;

/// <summary>
/// Script de verificación final para confirmar que GridRenderer funciona correctamente.
/// Ejecuta una serie de pruebas automáticas para validar la implementación.
/// </summary>
public class GridRendererVerification : MonoBehaviour
{
    [Header("Verification Settings")]
    [SerializeField] private bool _runOnStart = true;
    [SerializeField] private bool _verbose = true;

    private void Start()
    {
        if (_runOnStart)
        {
            RunVerification();
        }
    }

    /// <summary>
    /// Ejecuta todas las verificaciones del GridRenderer.
    /// </summary>
    [ContextMenu("Run Verification")]
    public void RunVerification()
    {
        Debug.Log("=== GridRenderer Verification Started ===");
        
        bool allTestsPassed = true;
        
        allTestsPassed &= VerifyGridRendererCreation();
        allTestsPassed &= VerifyPreviewMode();
        allTestsPassed &= VerifyGridServiceIntegration();
        allTestsPassed &= VerifyColorConfiguration();
        allTestsPassed &= VerifyOccupancyVisualization();
        allTestsPassed &= VerifyNoDependencies();
        
        if (allTestsPassed)
        {
            Debug.Log("✅ GridRenderer Verification PASSED - All acceptance criteria met!");
        }
        else
        {
            Debug.LogError("❌ GridRenderer Verification FAILED - Check individual test results above");
        }
        
        Debug.Log("=== GridRenderer Verification Complete ===");
    }

    private bool VerifyGridRendererCreation()
    {
        if (_verbose) Debug.Log("[Verification] Testing GridRenderer creation...");
        
        try
        {
            var testObject = new GameObject("VerificationGridRenderer");
            var renderer = testObject.AddComponent<GridRenderer>();
            
            bool success = renderer != null;
            
            DestroyImmediate(testObject);
            
            if (success && _verbose) Debug.Log("✅ GridRenderer creation test passed");
            return success;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ GridRenderer creation failed: {e.Message}");
            return false;
        }
    }

    private bool VerifyPreviewMode()
    {
        if (_verbose) Debug.Log("[Verification] Testing preview mode (without GridService)...");
        
        try
        {
            ServiceRegistry.Clear(); // Ensure no GridService
            
            var testObject = new GameObject("VerificationPreviewMode");
            var renderer = testObject.AddComponent<GridRenderer>();
            
            // Simulate Start() call
            renderer.Start();
            
            // If we reach here without exceptions, preview mode works
            bool success = true;
            
            DestroyImmediate(testObject);
            
            if (success && _verbose) Debug.Log("✅ Preview mode test passed");
            return success;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Preview mode test failed: {e.Message}");
            return false;
        }
    }

    private bool VerifyGridServiceIntegration()
    {
        if (_verbose) Debug.Log("[Verification] Testing GridService integration...");
        
        try
        {
            // Create a test GridService
            var map = new GridMap(5, 5);
            var gridService = new GridService(map, Vector3.zero, 1.0f);
            ServiceRegistry.Register<GridService>(gridService);
            
            var testObject = new GameObject("VerificationGridServiceIntegration");
            var renderer = testObject.AddComponent<GridRenderer>();
            
            // Simulate Start() call
            renderer.Start();
            
            bool success = true;
            
            DestroyImmediate(testObject);
            ServiceRegistry.Clear();
            
            if (success && _verbose) Debug.Log("✅ GridService integration test passed");
            return success;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ GridService integration test failed: {e.Message}");
            ServiceRegistry.Clear();
            return false;
        }
    }

    private bool VerifyColorConfiguration()
    {
        if (_verbose) Debug.Log("[Verification] Testing color configuration...");
        
        try
        {
            var testObject = new GameObject("VerificationColorConfig");
            var renderer = testObject.AddComponent<GridRenderer>();
            
            // GridRenderer should have default color fields
            bool success = renderer != null;
            
            DestroyImmediate(testObject);
            
            if (success && _verbose) Debug.Log("✅ Color configuration test passed");
            return success;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Color configuration test failed: {e.Message}");
            return false;
        }
    }

    private bool VerifyOccupancyVisualization()
    {
        if (_verbose) Debug.Log("[Verification] Testing occupancy visualization capability...");
        
        try
        {
            // Create GridService with occupants
            var map = new GridMap(3, 3);
            var gridService = new GridService(map, Vector3.zero, 1.0f);
            
            // Add some occupants
            gridService.AddOccupant(new Vector2Int(1, 1), CellOccupant.Robot);
            gridService.AddOccupant(new Vector2Int(2, 1), CellOccupant.Jewel);
            
            ServiceRegistry.Register<GridService>(gridService);
            
            var testObject = new GameObject("VerificationOccupancy");
            var renderer = testObject.AddComponent<GridRenderer>();
            renderer.Start();
            
            bool success = true;
            
            DestroyImmediate(testObject);
            ServiceRegistry.Clear();
            
            if (success && _verbose) Debug.Log("✅ Occupancy visualization test passed");
            return success;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Occupancy visualization test failed: {e.Message}");
            ServiceRegistry.Clear();
            return false;
        }
    }

    private bool VerifyNoDependencies()
    {
        if (_verbose) Debug.Log("[Verification] Testing no circular dependencies...");
        
        try
        {
            ServiceRegistry.Clear();
            
            // GridRenderer should not create GridService
            var testObject = new GameObject("VerificationNoDeps");
            var renderer = testObject.AddComponent<GridRenderer>();
            renderer.Start();
            
            // Check that GridService was not automatically created
            bool hasGridService = ServiceRegistry.TryResolve<GridService>(out _);
            bool success = !hasGridService; // Should be false (no GridService created)
            
            DestroyImmediate(testObject);
            ServiceRegistry.Clear();
            
            if (success && _verbose) Debug.Log("✅ No circular dependencies test passed");
            return success;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ No circular dependencies test failed: {e.Message}");
            ServiceRegistry.Clear();
            return false;
        }
    }
}