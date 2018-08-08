using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseComponentRegister<T> : MonoBehaviour where T : Component
{
    private void Awake()
    {
        var components = GetComponents<T>();
        foreach (var component in components)
        {
            ComponentCollector.Add(component);
        }
    }
}
