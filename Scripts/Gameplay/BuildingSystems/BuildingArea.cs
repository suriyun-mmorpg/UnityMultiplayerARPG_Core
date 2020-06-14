using UnityEngine;

namespace MultiplayerARPG
{
    public class BuildingArea : MonoBehaviour, IGameEntity, IUnHittable
    {
        public BuildingEntity entity;
        public string buildingType;
        public bool snapBuildingObject;
        
        public BaseGameEntity Entity
        {
            get { return entity; }
        }
        
        private void Start()
        {
            if (entity == null)
                entity = GetComponentInParent<BuildingEntity>();
        }
    }
}
