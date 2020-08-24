using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct GameEffectPoolContainer
    {
        public Transform container;
        public GameEffect prefab;

        public void GetInstance()
        {
            PoolSystem.GetInstance(prefab, container.position, container.rotation).FollowingTarget = container;
        }
    }
}