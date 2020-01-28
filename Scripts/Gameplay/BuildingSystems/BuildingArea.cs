using UnityEngine;

namespace MultiplayerARPG
{
    public class BuildingArea : MonoBehaviour, IGameEntity
    {
        public BuildingEntity entity;
        public string buildingType;
        public bool snapBuildingObject;
        
        public uint ObjectId
        {
            get { return Entity == null ? 0 : Entity.ObjectId; }
        }

        public BaseGameEntity Entity
        {
            get { return entity == null ? null : entity.Entity; }
        }
        
        private void Start()
        {
            if (entity == null)
                entity = GetComponentInParent<BuildingEntity>();
        }
    }
}
