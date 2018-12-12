using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class BuildingArea : MonoBehaviour
    {
        public string buildingType;
        public bool snapBuildingObject;

        [HideInInspector, System.NonSerialized]
        public BuildingEntity buildingEntity;

        public uint EntityObjectId
        {
            get { return buildingEntity == null ? 0 : buildingEntity.ObjectId; }
        }
    }
}
