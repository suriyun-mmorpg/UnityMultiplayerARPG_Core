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
                    try
                    {
                        // I tried to avoid null exception but it still ocurring
                        if (instance != null && instance.gameObject != null)
                            Object.Destroy(instance.gameObject);
                    }
                    catch { }
                }
            }
            m_Pools.Clear();
        }

        public static void InitPool(IPoolDescriptor prefab)
        {
            if (prefab == null || m_Pools.ContainsKey(prefab))
                return;

            prefab.InitPrefab();

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

        public static T GetInstance<T>(T prefab)
            where T : class, IPoolDescriptor
        {
            if (prefab == null)
                return null;
            T instance = GetInstance(prefab, Vector3.zero, Quaternion.identity);
            return instance;
        }

        public static T GetInstance<T>(T prefab, Vector3 position, Quaternion rotation)
            where T : class, IPoolDescriptor
        {
            if (prefab == null)
                return null;
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
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.gameObject.SetActive(true);
                obj.OnGetInstance();

                return obj as T;
            }

            InitPool(prefab);
            return GetInstance(prefab, position, rotation);
        }

        public static void PushBack(IPoolDescriptor instance)
        {
            if (instance == null)
            {
                Debug.LogWarning("[PoolSystem] Cannot push back. The instance's is empty.");
                return;
            }
            if (instance.ObjectPrefab == null)
            {
                Debug.LogWarning("[PoolSystem] Cannot push back. The instance's prefab is empty");
                return;
            }
            Queue<IPoolDescriptor> queue;
            if (!m_Pools.TryGetValue(instance.ObjectPrefab, out queue))
            {
                Debug.LogWarning("[PoolSystem] Cannot push back. The instance's prefab does not initailized yet.");
                return;
            }
            instance.gameObject.SetActive(false);
            queue.Enqueue(instance);
        }
    }
}
