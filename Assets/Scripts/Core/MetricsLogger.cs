using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Servicio/MB que acumula KPIs de la simulación (movimientos, tiempo por acción,
/// joyas recogidas/entregadas, eficiencia) y los expone a UI o exporta.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Recibe eventos desde <see cref="RobotController"/> y <see cref="ZoneController"/>.
/// - Consultado por UI y por <see cref="SimulationManager"/> para fin de simulación.
/// </remarks>
public sealed class MetricsLogger : MonoBehaviour
{
    private int _totalSteps = 0;
    private int _totalJewelsDelivered = 0;
    private Dictionary<JewelColor, int> _jewelsByColor = new Dictionary<JewelColor, int>();
    private int _totalScore = 0;
    private float _simulationStartTime;
    
    /// <summary>Total de pasos/movimientos ejecutados por todos los robots</summary>
    public int TotalSteps => _totalSteps;
    
    /// <summary>Total de joyas entregadas exitosamente</summary>
    public int TotalJewelsDelivered => _totalJewelsDelivered;
    
    /// <summary>Puntuación total acumulada</summary>
    public int TotalScore => _totalScore;
    
    /// <summary>Tiempo transcurrido desde el inicio de la simulación</summary>
    public float ElapsedTime => Time.time - _simulationStartTime;
    
    private void Awake()
    {
        // Inicializar contadores por color
        foreach (JewelColor color in System.Enum.GetValues(typeof(JewelColor)))
        {
            _jewelsByColor[color] = 0;
        }
        
        // Registrar en ServiceRegistry para que otros componentes puedan acceder
        ServiceRegistry.Register(this);
    }
    
    private void Start()
    {
        _simulationStartTime = Time.time;
        Debug.Log("[MetricsLogger] Metrics tracking started");
    }
    
    /// <summary>
    /// Registra que un robot ha dado un paso/movimiento.
    /// </summary>
    /// <param name="robotId">ID del robot que se movió</param>
    public void RecordRobotStep(int robotId)
    {
        _totalSteps++;
        Debug.Log($"[MetricsLogger] Robot {robotId} step recorded. Total steps: {_totalSteps}");
    }
    
    /// <summary>
    /// Registra que una joya ha sido entregada exitosamente.
    /// </summary>
    /// <param name="jewelColor">Color de la joya entregada</param>
    /// <param name="jewelValue">Valor de la joya entregada</param>
    public void RecordJewelDelivered(JewelColor jewelColor, int jewelValue)
    {
        _totalJewelsDelivered++;
        _jewelsByColor[jewelColor]++;
        _totalScore += jewelValue;
        
        Debug.Log($"[MetricsLogger] {jewelColor} jewel delivered (value: {jewelValue}). Total delivered: {_totalJewelsDelivered}, Score: {_totalScore}");
    }
    
    /// <summary>
    /// Muestra un resumen completo de las métricas en la consola.
    /// </summary>
    public void ShowFinalReport()
    {
        Debug.Log("=== FINAL SIMULATION REPORT ===");
        Debug.Log($"Simulation Duration: {ElapsedTime:F2} seconds");
        Debug.Log($"Total Robot Steps: {_totalSteps}");
        Debug.Log($"Total Jewels Delivered: {_totalJewelsDelivered}");
        Debug.Log($"Total Score: {_totalScore}");
        
        Debug.Log("Jewels by Color:");
        foreach (var kvp in _jewelsByColor)
        {
            if (kvp.Value > 0)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}");
            }
        }
        
        // Calcular eficiencia si hay datos
        if (_totalSteps > 0 && _totalJewelsDelivered > 0)
        {
            float efficiency = (float)_totalJewelsDelivered / _totalSteps;
            Debug.Log($"Efficiency: {efficiency:F3} jewels per step");
        }
        
        Debug.Log("=== END REPORT ===");
    }
    
    /// <summary>
    /// Reinicia todas las métricas para una nueva simulación.
    /// </summary>
    public void Reset()
    {
        _totalSteps = 0;
        _totalJewelsDelivered = 0;
        _totalScore = 0;
        _simulationStartTime = Time.time;
        
        foreach (JewelColor color in System.Enum.GetValues(typeof(JewelColor)))
        {
            _jewelsByColor[color] = 0;
        }
        
        Debug.Log("[MetricsLogger] Metrics reset for new simulation");
    }
}