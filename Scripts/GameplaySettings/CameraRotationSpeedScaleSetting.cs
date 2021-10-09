using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class CameraRotationSpeedScaleSetting : MonoBehaviour
    {
        public Slider slider;
        public float defaultValue = 1f;
        public string cameraRotationSpeedScaleSaveKey = "DEFAULT_CAMERA_ROTATION_SPEED_SCALE";
        public float? cameraRotationSpeedScale;
        public float CameraRotationSpeedScale
        {
            get
            {
                if (!cameraRotationSpeedScale.HasValue)
                    cameraRotationSpeedScale = PlayerPrefs.GetFloat(cameraRotationSpeedScaleSaveKey, defaultValue);
                return cameraRotationSpeedScale.Value;
            }
            set
            {
                cameraRotationSpeedScale = value;
                PlayerPrefs.SetFloat(cameraRotationSpeedScaleSaveKey, value);
            }
        }

        private void Start()
        {
            slider.minValue = 0.01f;
            slider.maxValue = 1f;
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
        }
    }
}
