using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UtilsComponents
{
    public class CanvasScalerSetupByAspectRatio : MonoBehaviour
    {
        [System.Serializable]
        public struct Setting : System.IComparable
        {
            public int width;
            public int height;
            public float matchWidthOrHeight;

            public float Aspect()
            {
                return (float)width / (float)height;
            }

            public int CompareTo(object obj)
            {
                if (obj is Setting setting)
                    return Aspect().CompareTo(setting.Aspect());
                return 0;
            }
        }
        public float defaultMatchWidthOrHeight = 1f;
        public List<Setting> settings = new List<Setting>();
        public List<CanvasScaler> scalers = new List<CanvasScaler>();
        private int _dirtyWidth;
        private int _dirtyHeight;

        public void Update()
        {
            if (_dirtyWidth != Screen.width ||
                _dirtyHeight != Screen.height)
            {
                _dirtyWidth = Screen.width;
                _dirtyHeight = Screen.height;
                UpdateCanvasScalers();
            }
        }

        public float GetSetting(float aspect)
        {
            settings.Sort();
            foreach (Setting setting in settings)
            {
                if (aspect + aspect * 0.025f > setting.Aspect() &&
                    aspect - aspect * 0.025f < setting.Aspect())
                    return setting.matchWidthOrHeight;
            }
            return defaultMatchWidthOrHeight;
        }

        public void UpdateCanvasScaler(CanvasScaler scaler, float matchWidthOrHeight)
        {
            if (scaler == null || scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                return;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = matchWidthOrHeight;
#if UNITY_EDITOR
            EditorUtility.SetDirty(scaler);
#endif
        }

        [ContextMenu("Update Canvas Scalers")]
        public void UpdateCanvasScalers()
        {
            float width = (float)Screen.width;
            float height = (float)Screen.height;
            float aspect = width / height;
            float matchWidthOrHeight = GetSetting(aspect);
            for (int i = 0; i < scalers.Count; ++i)
            {
                UpdateCanvasScaler(scalers[i], matchWidthOrHeight);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Find All Canvas Scaler And Set To List")]
        public void FindAllCanvasScalerAndSetToList()
        {
            CanvasScaler[] result = FindObjectsOfType<CanvasScaler>(true);
            scalers.Clear();
            scalers.AddRange(result);
            EditorUtility.SetDirty(this);
        }
#endif
    }
}
