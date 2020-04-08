using UnityEngine;

namespace MultiplayerARPG
{
    public interface IPoolDescriptor
    {
        IPoolDescriptor ObjectPrefab { get; set; }
        GameObject gameObject { get; }
        Transform transform { get; }
        int PoolSize { get; }
        /// <summary>
        /// Use this function to prepare prefab default value
        /// </summary>
        void InitPrefab();
    }
}
