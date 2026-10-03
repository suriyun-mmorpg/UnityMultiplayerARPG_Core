using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>Driver horn button observer. Keep this outside its initially hidden controls root.</summary>
    public class UIVehicleHorn : MonoBehaviour
    {
        public GameObject controlsRoot;
        public UnityEngine.UI.Button buttonHorn;
        private BasePlayerCharacterEntity _character;
        private VehicleHornComponent _horn;

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
            if (_character != null) _character.onSetPassengingVehicle -= OnVehicleChanged;
            _character = character as BasePlayerCharacterEntity;
            if (_character != null) _character.onSetPassengingVehicle += OnVehicleChanged;
            BindVehicle();
        }

        private void OnVehicleChanged(BaseGameEntity entity) => BindVehicle();

        private void BindVehicle()
        {
            if (_horn != null) _horn.SetLocalInput(false);
            IVehicleEntity vehicle = _character != null ? _character.PassengingVehicleEntity : null;
            _horn = !vehicle.IsNull() && vehicle.IsDriver(_character.PassengingVehicleSeatIndex)
                ? vehicle.Entity.GetComponent<VehicleHornComponent>() : null;
            Refresh();
        }

        public void Refresh()
        {
            bool available = _character != null && _horn != null && _horn.CanUseLocalHorn;
            if (!available && _horn != null) _horn.SetLocalInput(false);
            if (controlsRoot != null && !transform.IsChildOf(controlsRoot.transform)) controlsRoot.SetActive(available);
            if (buttonHorn != null) buttonHorn.interactable = available;
        }

        public void SetPressed(bool pressed)
        {
            if (_horn != null) _horn.SetLocalInput(pressed);
        }
    }
}
