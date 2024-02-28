using UnityEngine;

namespace MultiplayerARPG
{
    public interface IComponentWithPrefabRef
    {
        void SetupRefToPrefab(GameObject prefab);
    }

    [DisallowMultipleComponent]
    public abstract class ComponentWithPrefabRef<T> : MonoBehaviour, IComponentWithPrefabRef
        where T : MonoBehaviour
    {
        public T refToPrefab;

        public void SetupRefToPrefab(GameObject prefab)
        {
            refToPrefab = prefab.GetComponent<T>();
        }
    }
}
