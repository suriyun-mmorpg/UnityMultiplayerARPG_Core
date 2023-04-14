using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class CameraRotationSpeedScaleSetting : MonoBehaviour
    {
        public Slider slider;
        public TextWrapper textScaleValue;
        public float minValue = 0.01f;
        public float maxValue = 1f;
        public float defaultValue = 1f;
        public string cameraRotationSpeedScaleSaveKey = "DEFAULT_CAMERA_ROTATION_SPEED_SCALE";
        private readonly static Dictionary<string, float> s_cameraRotationSpeedScales = new Dictionary<string, float>();
        public float CameraRotationSpeedScale
        {
            get
            {
                return GetCameraRotationSpeedScaleByKey(cameraRotationSpeedScaleSaveKey, defaultValue);
            }
            set
            {
                if (!string.IsNullOrEmpty(cameraRotationSpeedScaleSaveKey))
                {
                    s_cameraRotationSpeedScales[cameraRotationSpeedScaleSaveKey] = value;
                    PlayerPrefs.SetFloat(cameraRotationSpeedScaleSaveKey, value);
                }
            }
        }

        public static float GetCameraRotationSpeedScaleByKey(string key, float defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;
            if (!s_cameraRotationSpeedScales.ContainsKey(key))
                s_cameraRotationSpeedScales[key] = PlayerPrefs.GetFloat(key, defaultValue);
            return s_cameraRotationSpeedScales[key];
        }

        private void Start()
        {
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.SetValueWithoutNotify(CameraRotationSpeedScale);
            slider.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            slider.onValueChanged.RemoveListener(OnValueChanged);
        }

        public void OnValueChanged(float value)
        {
            CameraRotationSpeedScale = value;
            if (textScaleValue != null)
                textScaleValue.text = value.ToString("N2");
        }
    }
}
