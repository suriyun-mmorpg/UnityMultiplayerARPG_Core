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
        public bool allowRotateInSocket;
        public Collider CacheCollider { get; private set; }
        public Rigidbody CacheRigidbody { get; private set; }
        public Collider2D CacheCollider2D { get; private set; }
        public Rigidbody2D CacheRigidbody2D { get; private set; }

        private void Start()
        {
            if (entity == null)
                entity = GetComponentInParent<BuildingEntity>();
            if (entity != null)
            {
                gameObject.GetOrAddComponent<UnHittable>();
                CacheCollider = GetComponent<Collider>();
                if (CacheCollider)
                {
                    CacheRigidbody = gameObject.GetOrAddComponent<Rigidbody>();
                    CacheRigidbody.useGravity = false;
                    CacheRigidbody.isKinematic = true;
                    return;
                }
                CacheCollider2D = GetComponent<Collider2D>();
                if (CacheCollider2D)
                {
                    CacheRigidbody2D = gameObject.GetOrAddComponent<Rigidbody2D>();
                    CacheRigidbody2D.gravityScale = 0;
                    CacheRigidbody2D.isKinematic = true;
                }
            }
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
