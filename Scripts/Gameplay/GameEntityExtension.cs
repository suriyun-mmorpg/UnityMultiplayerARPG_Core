using UnityEngine;

namespace MultiplayerARPG
{
    public static class GameEntityExtension
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
            return gameEntity.Entity.CacheTransform;
        }

        public static GameObject GetGameObject(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity)
                return null;
            return gameEntity.Entity.gameObject;
        }

        public static void PlayJumpAnimation(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity || !(gameEntity.Entity.Model is IJumppableModel))
                return;
            (gameEntity.Entity.Model as IJumppableModel).PlayJumpAnimation();
        }

        public static void PlayPickupAnimation(this IGameEntity gameEntity)
        {
            if (gameEntity == null || !gameEntity.Entity || !(gameEntity.Entity.Model is IPickupableModel))
                return;
            (gameEntity.Entity.Model as IPickupableModel).PlayPickupAnimation();
        }
    }
}
