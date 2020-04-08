using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PoolSystem
    {
        private static Dictionary<IPoolDescriptor, Queue<IPoolDescriptor>> m_Pools = new Dictionary<IPoolDescriptor, Queue<IPoolDescriptor>>();

        public static void Clear()
        {
            foreach (Queue<IPoolDescriptor> queue in m_Pools.Values)
            {
                while (queue.Count > 0)
                {
                    IPoolDescriptor instance = queue.Dequeue();
                    Object.Destroy(instance.gameObject);
                }
            }
            m_Pools.Clear();
        }

        public static void InitPool(IPoolDescriptor prefab)
        {
            if (m_Pools.ContainsKey(prefab))
                return;

            Queue<IPoolDescriptor> queue = new Queue<IPoolDescriptor>();

            IPoolDescriptor obj;

            for (int i = 0; i < prefab.PoolSize; ++i)
            {
                obj = Object.Instantiate(prefab.gameObject).GetComponent<IPoolDescriptor>();
                obj.ObjectPrefab = prefab;
                obj.gameObject.SetActive(false);
                queue.Enqueue(obj);
            }

            m_Pools[prefab] = queue;
        }

        public static T GetInstance<T>(T prefab, Vector3 position, Quaternion rotation)
            where T : class, IPoolDescriptor
        {
            T instance = GetInstance(prefab);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            return instance;
        }

        public static T GetInstance<T>(T prefab)
            where T : class, IPoolDescriptor
        {
            Queue<IPoolDescriptor> queue;
            if (m_Pools.TryGetValue(prefab, out queue))
            {
                IPoolDescriptor obj;

                if (queue.Count > 0)
                {
                    obj = queue.Dequeue();
                }
                else
                {
                    obj = Object.Instantiate(prefab.gameObject).GetComponent<IPoolDescriptor>();
                }
                obj.gameObject.SetActive(true);

                return obj as T;
            }

            InitPool(prefab);
            return GetInstance(prefab);
        }

        public static void PushBack(IPoolDescriptor instance)
        {
            Queue<IPoolDescriptor> queue;
            if (m_Pools.TryGetValue(instance.ObjectPrefab, out queue))
            {
                instance.gameObject.SetActive(false);
                queue.Enqueue(instance);
            }
        }
    }
}
