using System;
using System.Collections.Generic;

/// <summary>
/// Registro simple de servicios a nivel proceso/escena.
/// Permite registrar y resolver dependencias sin acoplar a un IoC container pesado.
/// </summary>
public static class ServiceRegistry
{
    /// <summary>
    /// Mapa de servicios registrados. Por ejemplo, 
    /// <see cref="SimulationConfig"/> o <see cref="AgentConfig"/>.
    /// </summary>
    private static readonly Dictionary<Type, object> _map = new();

    /// <summary>
    /// Registra una instancia de servicio en el contenedor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="instance"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void Register<T>(T instance) where T : class
    {
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        _map[typeof(T)] = instance;
    }

    
    public static bool TryResolve<T>(out T service) where T : class
    {
        if (_map.TryGetValue(typeof(T), out var obj))
        {
            service = (T)obj;
            return true;
        }
        service = null;
        return false;
    }

    public static T Resolve<T>() where T : class
    {
        if (!TryResolve<T>(out var s))
            throw new InvalidOperationException($"Service not found: {typeof(T).Name}");
        return s;
    }

    public static bool IsRegistered<T>() where T : class => _map.ContainsKey(typeof(T));

    public static void Clear() => _map.Clear();
}

