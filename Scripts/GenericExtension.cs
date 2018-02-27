using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GenericExtension
{
    public static void RemoveChildren(this Transform transform)
    {
        for (var i = transform.childCount - 1; i >= 0; --i)
        {
            var lastChild = transform.GetChild(i);
            Object.Destroy(lastChild.gameObject);
        }
    }

    public static void SetChildrenActive(this Transform transform, bool isActive)
    {
        for (var i = 0; i < transform.childCount; ++i)
        {
            transform.GetChild(i).gameObject.SetActive(isActive);
        }
    }

    public static void RemoveComponents<T>(this GameObject gameObject) where T : Component
    {
        var components = gameObject.GetComponents<T>();
        foreach (var component in components)
        {
            Object.DestroyImmediate(component);
        }
    }

    public static void RemoveComponentsInChildren<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        var components = gameObject.GetComponentsInChildren<T>(includeInactive);
        foreach (var component in components)
        {
            Object.DestroyImmediate(component);
        }
    }

    public static void RemoveComponentsInParent<T>(this GameObject gameObject, bool includeInactive) where T : Component
    {
        var components = gameObject.GetComponentsInParent<T>(includeInactive);
        foreach (var component in components)
        {
            Object.DestroyImmediate(component);
        }
    }
}
