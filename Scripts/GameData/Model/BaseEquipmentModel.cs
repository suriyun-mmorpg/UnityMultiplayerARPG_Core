using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEquipmentModel : MonoBehaviour
{
    private int level;
    public int Level
    {
        get { return level; }
        set
        {
            if (level != value)
            {
                level = value;
                OnLevelChanged(level);
            }
        }
    }

    [Tooltip("This is overriding missile damage transform, if this is not empty, it will spawn missile damage entity from this transform")]
    public Transform missileDamageTransform;

    public abstract void OnLevelChanged(int level);
}
