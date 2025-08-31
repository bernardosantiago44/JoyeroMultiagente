using UnityEngine;

/// <summary>
/// Punto de arranque de la escena. Crea/inyecta servicios (GridService, PathfindingService, etc.),
/// carga configs y expone referencias compartidas.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Inicializa <see cref="SimulationManager"/> y registra sistemas globales.
/// - Provee <see cref="PathfindingService"/> a <see cref="PathfindingComponent"/>.
/// - Usa <see cref="ValidationService"/> para sanity checks.
/// </remarks>
public sealed class GameBootstrap : MonoBehaviour
    {
        [Header("Configs (SO)")]
        [SerializeField] private SimulationConfig _simulationConfig;
        [SerializeField] private AgentConfig _agentConfig;
        [SerializeField] private SpawnConfig _spawnConfig;
        [SerializeField] private ValidationService _validationService; // puede ser null al inicio

        public static GameBootstrap Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[GameBootstrap] Duplicado detectado. Destruyendo este objeto.");
                Destroy(gameObject);
                return;
            }
            if (_validationService == null)
            {
               _validationService = new ValidationService(); 
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 1) Validar referencias mínimas
            if (_simulationConfig == null)
                Debug.LogError("[GameBootstrap] Falta SimulationConfig asignado en el inspector.");
            if (_agentConfig == null)
                Debug.LogError("[GameBootstrap] Falta AgentConfig asignado en el inspector.");
            if (_spawnConfig == null)
                Debug.LogError("[GameBootstrap] Falta SpawnConfig asignado en el inspector.");

            // 2) Registrar configs y servicios mínimos
            ServiceRegistry.Clear(); // Limpiar escena
            if (_simulationConfig != null) ServiceRegistry.Register(_simulationConfig);
            if (_agentConfig != null) ServiceRegistry.Register(_agentConfig);
            if (_spawnConfig != null) ServiceRegistry.Register(_spawnConfig);
            if (_validationService != null) ServiceRegistry.Register(_validationService);

            // 3) Validaciones iniciales (solo configuración, sin grid)
            try
            {
                if (_validationService != null)
                {
                    _validationService.RunAll();
                }
                else
                {
                    Debug.LogWarning("[GameBootstrap] ValidationService no asignado. Saltando validaciones iniciales.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GameBootstrap] Validaciones fallaron: {ex.Message}");
            }

            Debug.Log("[GameBootstrap] Inicializado. Configs y servicios registrados.");
        }
    }
