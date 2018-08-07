using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MigrateUIWrapper : MonoBehaviour
{
    [ContextMenu("Migrate UIs")]
    public void MigrateUIs()
    {
        // Find all components include inactive
        List<UIBase> objects = new List<UIBase>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var s = SceneManager.GetSceneAt(i);
            if (s.isLoaded)
            {
                var allGameObjects = s.GetRootGameObjects();
                for (int j = 0; j < allGameObjects.Length; j++)
                {
                    var go = allGameObjects[j];
                    objects.AddRange(go.GetComponentsInChildren<UIBase>(true));
                }
            }
        }
        // Call migrate ui components
        foreach (var obj in objects)
        {
            var type = obj.GetType();
            var method = type.GetMethod("MigrateUIComponents", BindingFlags.Public | BindingFlags.Instance);
            var args = new object[0];
            if (method != null)
                method.Invoke(obj, args);
#if UNITY_EDITOR
            EditorUtility.SetDirty(obj.gameObject);
#endif
        }
    }
}
