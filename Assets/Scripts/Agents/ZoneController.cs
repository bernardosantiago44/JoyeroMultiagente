using UnityEngine;

/// <summary>
/// Define una zona de entrega/objetivo del mapa donde los robots depositan joyas.
/// Puede validar drops, sumar puntaje y notificar eventos de entrega.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Consultada por <see cref="GridService"/> para ocupación y tipo de celda.
/// - Recibe eventos desde <see cref="RobotController"/> al hacer "Drop".
/// - Puede informar a <see cref="MetricsLogger"/> y activar condiciones de fin en <see cref="RuleSystem"/>.
/// </remarks>
public sealed class ZoneController : MonoBehaviour
{
    [SerializeField] private JewelColor _acceptedColor = JewelColor.Red;
    
    private Vector2Int _currentCell;
    private GridService _gridService;
    private MetricsLogger _metricsLogger;
    
    /// <summary>Color de joya que acepta esta zona</summary>
    public JewelColor AcceptedColor => _acceptedColor;
    
    /// <summary>Posición de la zona en el grid</summary>
    public Vector2Int Cell => _currentCell;
    
    private void Start()
    {
        // Obtener servicios
        if (!ServiceRegistry.TryResolve<GridService>(out _gridService))
        {
            Debug.LogError($"[ZoneController] GridService not found for zone at {transform.position}");
            return;
        }
        
        ServiceRegistry.TryResolve<MetricsLogger>(out _metricsLogger);
        
        // Registrar posición en el grid
        _currentCell = _gridService.WorldToCell(transform.position);
        
        if (_gridService.IsInside(_currentCell))
        {
            _gridService.AddOccupant(_currentCell, CellOccupant.Zone);
            Debug.Log($"[ZoneController] {_acceptedColor} zone registered at cell {_currentCell}");
        }
        else
        {
            Debug.LogWarning($"[ZoneController] Zone at world position {transform.position} is outside grid bounds");
        }
    }
    
    /// <summary>
    /// Intenta entregar una joya en esta zona.
    /// Valida que el color coincida con el aceptado por la zona.
    /// </summary>
    /// <param name="jewelColor">Color de la joya a entregar</param>
    /// <param name="jewelValue">Valor de la joya a entregar</param>
    /// <returns>True si la entrega fue exitosa</returns>
    public bool TryDeliverJewel(JewelColor jewelColor, int jewelValue)
    {
        if (jewelColor != _acceptedColor)
        {
            Debug.LogWarning($"[ZoneController] Zone accepts {_acceptedColor} but received {jewelColor}");
            return false;
        }
        
        // Entrega exitosa
        Debug.Log($"[ZoneController] Successfully delivered {jewelColor} jewel (value: {jewelValue}) to zone at {_currentCell}");
        
        // Notificar al MetricsLogger si está disponible
        if (_metricsLogger != null)
        {
            _metricsLogger.RecordJewelDelivered(jewelColor, jewelValue);
        }
        
        return true;
    }
    
    private void OnDestroy()
    {
        // Limpiar ocupación del grid
        if (_gridService != null && _gridService.IsInside(_currentCell))
        {
            _gridService.RemoveOccupant(_currentCell, CellOccupant.Zone);
        }
    }
}