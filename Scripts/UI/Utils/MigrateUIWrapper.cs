using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MigrateUIWrapper : MonoBehaviour
{
    [ContextMenu("Migrate UIs")]
    public void MigrateUIs()
    {
        var objects = FindObjectsOfType<UIBase>();
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
