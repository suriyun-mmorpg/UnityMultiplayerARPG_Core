using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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
#if UNITY_EDITOR
        [InspectorButton(nameof(ReplacePrefab))]
        public bool btnRepacePrefab;
#endif

        public void SetupRefToPrefab(GameObject prefab)
        {
            refToPrefab = prefab.GetComponent<T>();
        }

#if UNITY_EDITOR
        public void ReplacePrefab()
        {
            string path = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(refToPrefab);
            PrefabUtility.SaveAsPrefabAsset(gameObject, path, out bool success);
            Debug.Log($"Replaced {gameObject} to {path} success?: {success}");
        }
#endif
    }
}
