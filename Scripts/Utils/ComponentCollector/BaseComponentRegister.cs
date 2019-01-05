using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseComponentRegister<T> : MonoBehaviour where T : Component
{
    private void Awake()
    {
        T[] components = GetComponents<T>();
        foreach (T component in components)
        {
            ComponentCollector.Add(component);
        }
    }
}
