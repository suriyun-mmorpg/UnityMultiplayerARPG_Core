using LiteNetLibManager;
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

        public LiteNetLibIdentity Identity
        {
            get { return entity.Identity; }
        }

        private void Start()
        {
            if (entity == null)
                entity = GetComponentInParent<BuildingEntity>();
        }

        public virtual void PrepareRelatesData()
        {
            // Do nothing
        }
    }
}
