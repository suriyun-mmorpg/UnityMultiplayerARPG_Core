using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class BuildingArea : MonoBehaviour
    {
        public string buildingType;
        public bool snapBuildingObject;

        [HideInInspector]
        public BuildingObject buildingObject;
        public BuildingEntity buildingEntity { get { return buildingObject == null ? null : buildingObject.buildingEntity; } }

        public uint EntityObjectId
        {
            get { return buildingObject == null ? 0 : buildingObject.EntityObjectId; }
        }
    }
}
