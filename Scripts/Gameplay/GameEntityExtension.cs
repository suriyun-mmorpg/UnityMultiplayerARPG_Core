using UnityEngine;

namespace MultiplayerARPG
{
    public static class GameEntityExtension
    {
        public static long GetConnectionId(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity)
                return -1;
            return gameEntity.Identity.ConnectionId;
        }

        public static uint GetObjectId(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity)
                return 0;
            return gameEntity.Identity.ObjectId;
        }

        public static Transform GetTransform(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity)
                return null;
            return gameEntity.Entity.CacheTransform;
        }

        public static GameObject GetGameObject(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity)
                return null;
            return gameEntity.Entity.gameObject;
        }
    }
}
