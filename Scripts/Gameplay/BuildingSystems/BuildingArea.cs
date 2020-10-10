using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>
    /// Attach this component to any objects in scene to make it able to construct building.
    /// If this is child of building entity it will attach `UnHittable` component to avoid anything hitting it.
    /// </summary>
    public class BuildingArea : MonoBehaviour
    {
        public BuildingEntity entity;
        public string buildingType;
        public bool snapBuildingObject;

        private void Awake()
        {
            if (entity == null)
                entity = GetComponentInParent<BuildingEntity>();
            if (entity != null)
                gameObject.GetOrAddComponent<UnHittable>();
        }

        public bool IsPartOfBuildingEntity(BuildingEntity entity)
        {
            if (this.entity == null)
                return false;
            return this.entity.ObjectId == entity.ObjectId;
        }

        public uint GetEntityObjectId()
        {
            if (entity == null)
                return 0;
            return entity.ObjectId;
        }
    }
}
