using UnityEngine;

namespace MultiplayerARPG
{
    public static class GameEntityExtension
    {
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
