using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PositionInMap : MonoBehaviour
{
    public GameMapEntity mapEntity;

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

    private void Update()
    {
        if (mapEntity == null)
            return;

        var currentWorldPosition = TempTransform.position;
        var savingPosition = currentWorldPosition - mapEntity.MapOffsets;
        Debug.LogError(savingPosition);
    }
}
