using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingArea : MonoBehaviour
{
    public string buildingType;
    public bool snapBuildingObject;

    [HideInInspector]
    public BuildingObject buildingObject;

    public uint EntityObjectId
    {
        get { return buildingObject == null ? 0 : buildingObject.EntityObjectId; }
    }
}
