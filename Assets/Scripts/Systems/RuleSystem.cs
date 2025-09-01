using UnityEngine;

/// <summary>
/// Reglas globales de la simulación: condiciones de fin, límites de pasos/tiempo,
/// scoring, penalizaciones y validación de acciones.
/// </summary>
/// <remarks>
/// Interacciones:
/// - Consultado por <see cref="SimulationManager"/> para transiciones de estado.
/// - Notificado por <see cref="RobotController"/> y <see cref="ZoneController"/> en eventos clave.
/// </remarks>
public sealed class RuleSystem
{
    private const string LOG_PREFIX = "[RuleSystem]";
    
    /// <summary>
    /// Verifica si la simulación debe terminar.
    /// Condición simple: no quedan joyas disponibles en el mundo.
    /// </summary>
    /// <returns>True si la simulación debe terminar</returns>
    public bool ShouldEndSimulation()
    {
        // Buscar todas las joyas activas en la escena
        Jewel[] allJewels = Object.FindObjectsOfType<Jewel>();
        
        int availableJewels = 0;
        foreach (var jewel in allJewels)
        {
            if (jewel.IsAvailable)
            {
                availableJewels++;
            }
        }
        
        bool shouldEnd = availableJewels == 0;
        
        if (shouldEnd)
        {
            Debug.Log($"{LOG_PREFIX} End condition met: No available jewels remaining");
        }
        
        return shouldEnd;
    }
    
    /// <summary>
    /// Obtiene el número actual de joyas disponibles en el mundo.
    /// </summary>
    /// <returns>Número de joyas que aún pueden ser recogidas</returns>
    public int GetAvailableJewelCount()
    {
        Jewel[] allJewels = Object.FindObjectsOfType<Jewel>();
        
        int count = 0;
        foreach (var jewel in allJewels)
        {
            if (jewel.IsAvailable)
            {
                count++;
            }
        }
        
        return count;
    }
    
    /// <summary>
    /// Valida si una acción de movimiento es permitida.
    /// Por ahora siempre permite el movimiento (sin restricciones adicionales).
    /// </summary>
    /// <param name="robotId">ID del robot que quiere moverse</param>
    /// <param name="fromCell">Celda origen</param>
    /// <param name="toCell">Celda destino</param>
    /// <returns>True si el movimiento es válido</returns>
    public bool ValidateMovement(int robotId, Vector2Int fromCell, Vector2Int toCell)
    {
        // Implementación básica - siempre permite movimiento
        // En el futuro aquí podrían ir reglas más complejas como límites de energía, etc.
        return true;
    }
    
    /// <summary>
    /// Valida si una acción de recoger joya es permitida.
    /// </summary>
    /// <param name="robotId">ID del robot</param>
    /// <param name="jewelColor">Color de la joya a recoger</param>
    /// <returns>True si la acción es válida</returns>
    public bool ValidatePickup(int robotId, JewelColor jewelColor)
    {
        // Implementación básica - siempre permite recoger
        return true;
    }
    
    /// <summary>
    /// Valida si una acción de entregar joya es permitida.
    /// </summary>
    /// <param name="robotId">ID del robot</param>
    /// <param name="jewelColor">Color de la joya a entregar</param>
    /// <param name="zoneColor">Color aceptado por la zona</param>
    /// <returns>True si la acción es válida</returns>
    public bool ValidateDelivery(int robotId, JewelColor jewelColor, JewelColor zoneColor)
    {
        // La validación principal es que los colores coincidan
        return jewelColor == zoneColor;
    }
}