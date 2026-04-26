using System.Collections.Generic;
using UnityEngine;

public static class ComponentCache<T> where T : Component
{
    private static readonly Dictionary<Transform, T> _cache = new Dictionary<Transform, T>();

    public static T Get(Transform transform)
    {
        if (!_cache.TryGetValue(transform, out T component))
        {
            component = transform.GetComponent<T>();
            _cache.Add(transform, component);
        }
        return component;
    }

    public static T Get(Collider collider)
    {
        return Get(collider.transform);
    }

    public static void ClearCache()
    {
        _cache.Clear();
    }
}