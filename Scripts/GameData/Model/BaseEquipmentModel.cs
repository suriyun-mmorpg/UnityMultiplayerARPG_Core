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

    public Transform overrideMissileBarrel;

    public abstract void OnLevelChanged(int level);
}
