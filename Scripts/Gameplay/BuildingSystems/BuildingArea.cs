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

        /// <summary>
        /// Object id will be 0, if this component attached to objects in scene
        /// </summary>
        public uint ObjectId { get; private set; }

        private void Awake()
        {
            ObjectId = 0;
            if (entity == null)
                entity = GetComponentInParent<BuildingEntity>();
            if (entity != null)
            {
                ObjectId = entity.ObjectId;
                gameObject.GetOrAddComponent<UnHittable>();
            }
        }
    }
}
