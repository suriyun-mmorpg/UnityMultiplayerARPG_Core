using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionIgnore : MonoBehaviour
{
    public CollisionIgnoreOption[] ignoreOptions;
    private void Awake()
    {
        foreach (var ignoreOption in ignoreOptions)
        {
            Physics.IgnoreLayerCollision(ignoreOption.layer1.LayerIndex, ignoreOption.layer2.LayerIndex, ignoreOption.ignore);
        }
    }
}

[System.Serializable]
public struct CollisionIgnoreOption
{
    public SingleUnityLayer layer1;
    public SingleUnityLayer layer2;
    public bool ignore;
}
