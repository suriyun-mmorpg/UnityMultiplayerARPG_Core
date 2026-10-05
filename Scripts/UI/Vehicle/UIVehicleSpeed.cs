using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>Mounted vehicle speed HUD. Keep the observer outside its hidden controls root.</summary>
    public class UIVehicleSpeed : MonoBehaviour
    {
        public GameObject controlsRoot;
        public TextWrapper textSpeed;
        [Min(0f)]
        [Tooltip("Converts movement speed in metres/second: 3.6 for km/h, 2.236936 for mph, 1 for m/s")]
        public float speedMultiplier = 3.6f;
        [Tooltip("Format: {0} = speed after applying the multiplier")]
        public string textFormat = "{0:0} km/h";

        private BasePlayerCharacterEntity _character;
        private IVehicleEntity _vehicle;

        private void OnEnable()
        {
            GameInstance.OnSetPlayingCharacterEvent += OnPlayingCharacterChanged;
            OnPlayingCharacterChanged(GameInstance.PlayingCharacter);
        }

        private void OnDisable()
        {
            GameInstance.OnSetPlayingCharacterEvent -= OnPlayingCharacterChanged;
            OnPlayingCharacterChanged(null);
        }

        private void Update() => Refresh();

        private void OnPlayingCharacterChanged(IPlayerCharacterData character)
        {
            if (_character != null)
                _character.onSetPassengingVehicle -= OnVehicleChanged;
            _character = character as BasePlayerCharacterEntity;
            if (_character != null)
                _character.onSetPassengingVehicle += OnVehicleChanged;
            BindVehicle();
        }

        private void OnVehicleChanged(BaseGameEntity entity) => BindVehicle();

        private void BindVehicle()
        {
            _vehicle = _character != null ? _character.PassengingVehicleEntity : null;
            Refresh();
        }

        public void Refresh()
        {
            bool visible = _character != null && !_vehicle.IsNull();
            if (controlsRoot != null && !transform.IsChildOf(controlsRoot.transform))
                controlsRoot.SetActive(visible);
            if (!visible)
                return;
            if (textSpeed != null)
                textSpeed.text = string.Format(textFormat, Mathf.Max(0f, _vehicle.CurrentMoveSpeed) * Mathf.Max(0f, speedMultiplier));
        }
    }
}
