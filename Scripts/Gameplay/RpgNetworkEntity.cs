using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibHighLevel;

public class RpgNetworkEntity : LiteNetLibBehaviour
{
    private Transform tempTransform;
    public Transform TempTransform
    {
        get
        {
            if (tempTransform == null)
                tempTransform = GetComponent<Transform>();
            return tempTransform;
        }
    }

    private BaseRpgNetworkManager tempManager;
    public BaseRpgNetworkManager TempManager
    {
        get
        {
            if (tempManager == null)
                tempManager = Manager as BaseRpgNetworkManager;
            return tempManager;
        }
    }
}
