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
public sealed class Jewel : MonoBehaviour
{
    [SerializeField] private JewelColor _color = JewelColor.Red;
    [SerializeField] private int _value = 1;
    
    private Vector2Int _currentCell;
    private GridService _gridService;
    
    /// <summary>Color de la joya para clasificación por zonas</summary>
    public JewelColor Color => _color;
    
    /// <summary>Valor en puntos de la joya</summary>
    public int Value => _value;
    
    /// <summary>Posición actual en el grid</summary>
    public Vector2Int Cell => _currentCell;
    
    /// <summary>True si la joya está disponible para recoger</summary>
    public bool IsAvailable { get; private set; } = true;
    
    private void Start()
    {
        // Obtener GridService
        if (!ServiceRegistry.TryResolve<GridService>(out _gridService))
        {
            Debug.LogError($"[Jewel] GridService not found for jewel at {transform.position}");
            return;
        }
        
        // Registrar posición en el grid
        _currentCell = _gridService.WorldToCell(transform.position);
        
        if (_gridService.IsInside(_currentCell))
        {
            _gridService.AddOccupant(_currentCell, CellOccupant.Jewel);
            Debug.Log($"[Jewel] {_color} jewel registered at cell {_currentCell}");
        }
        else
        {
            Debug.LogWarning($"[Jewel] Jewel at world position {transform.position} is outside grid bounds");
        }
    }
    
    /// <summary>
    /// Simula que un robot recoge esta joya.
    /// La joya desaparece del mundo y del grid.
    /// </summary>
    /// <returns>True si la recogida fue exitosa</returns>
    public bool TryPickUp()
    {
        if (!IsAvailable)
        {
            Debug.LogWarning($"[Jewel] Attempted to pick up jewel that is not available");
            return false;
        }
        
        // Remover del grid
        if (_gridService != null && _gridService.IsInside(_currentCell))
        {
            _gridService.RemoveOccupant(_currentCell, CellOccupant.Jewel);
        }
        
        // Marcar como no disponible y ocultar
        IsAvailable = false;
        gameObject.SetActive(false);
        
        Debug.Log($"[Jewel] {_color} jewel picked up from cell {_currentCell}");
        return true;
    }
    
    private void OnDestroy()
    {
        // Limpiar ocupación del grid si aún está registrada
        if (_gridService != null && IsAvailable && _gridService.IsInside(_currentCell))
        {
            _gridService.RemoveOccupant(_currentCell, CellOccupant.Jewel);
        }
    }
}

/// <summary>Colores disponibles para las joyas</summary>
public enum JewelColor
{
    Red = 0,
    Blue = 1,
    Green = 2,
    Yellow = 3
}
