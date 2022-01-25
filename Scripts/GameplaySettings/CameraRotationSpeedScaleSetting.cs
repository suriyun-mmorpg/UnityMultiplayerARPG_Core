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
        private readonly static Dictionary<string, float> CameraRotationSpeedScales = new Dictionary<string, float>();
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
                    CameraRotationSpeedScales[cameraRotationSpeedScaleSaveKey] = value;
                    PlayerPrefs.SetFloat(cameraRotationSpeedScaleSaveKey, value);
                }
            }
        }

        public static float GetCameraRotationSpeedScaleByKey(string key, float defaultValue)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;
            if (!CameraRotationSpeedScales.ContainsKey(key))
                CameraRotationSpeedScales[key] = PlayerPrefs.GetFloat(key, defaultValue);
            return CameraRotationSpeedScales[key];
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
