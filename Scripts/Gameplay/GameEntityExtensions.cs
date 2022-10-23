using UnityEngine;

namespace MultiplayerARPG
{
    public static class GameEntityExtensions
    {
        public static long GetConnectionId(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity)
                return -1;
            return gameEntity.Entity.ConnectionId;
        }

        public static uint GetObjectId(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity)
                return 0;
            return gameEntity.Entity.ObjectId;
        }

        public static Transform GetTransform(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity)
                return null;
            return gameEntity.Entity.transform;
        }

        public static GameObject GetGameObject(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity)
                return null;
            return gameEntity.Entity.gameObject;
        }

        public static bool IsNull(this IGameEntity obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }

        public static bool IsNull(this IGameEntityComponent obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }

        public static bool IsNull(this IDamageableEntity obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }

        public static bool IsNull(this ITargetableEntity obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }

        public static bool IsNull(this IEntityMovement obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }

        public static bool IsNull(this IEntityMovementComponent obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }

        public static bool IsNull(this IBaseActivatableEntity obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }

        public static bool IsNull(this IActivatableEntity obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }

        public static bool IsNull(this IHoldActivatableEntity obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }

        public static bool IsNull(this IPickupActivatableEntity obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }

        public static bool IsNull(this IUnHittable obj)
        {
            if (obj is Object unityObj)
                return unityObj == null;
            return obj == null;
        }
    }
}
