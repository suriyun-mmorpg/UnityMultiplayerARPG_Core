using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>Mounted vehicle fuel HUD. Keep the observer outside its hidden controls root.</summary>
    public class UIVehicleFuel : MonoBehaviour
    {
        public GameObject controlsRoot;
        public TMPro.TMP_Text textFuel;
        public UnityEngine.UI.Image fuelFill;
        public UnityEngine.UI.Button buttonRefuel;
        public Color normalColor = new Color(0.3f, 0.85f, 0.6f);
        public Color lowColor = new Color(1f, 0.65f, 0.15f);
        public Color emptyColor = new Color(1f, 0.3f, 0.25f);

        private BasePlayerCharacterEntity _character;
        private VehicleFuelComponent _fuel;
        private float _nextRefreshTime;

        private void Update()
        {
            if (_fuel != null && Time.unscaledTime >= _nextRefreshTime)
            {
                _nextRefreshTime = Time.unscaledTime + 0.25f;
                Refresh();
            }
        }

        private void OnEnable()
        {
            GameInstance.OnSetPlayingCharacterEvent += OnPlayingCharacterChanged;
            if (buttonRefuel != null)
                buttonRefuel.onClick.AddListener(Refuel);
            OnPlayingCharacterChanged(GameInstance.PlayingCharacter);
        }

        private void OnDisable()
        {
            GameInstance.OnSetPlayingCharacterEvent -= OnPlayingCharacterChanged;
            if (buttonRefuel != null)
                buttonRefuel.onClick.RemoveListener(Refuel);
            OnPlayingCharacterChanged(null);
        }

        private void OnPlayingCharacterChanged(IPlayerCharacterData character)
        {
            if (_character != null)
            {
                _character.onSetPassengingVehicle -= OnVehicleChanged;
                _character.onNonEquipItemsOperation -= OnInventoryChanged;
            }
            _character = character as BasePlayerCharacterEntity;
            if (_character != null)
            {
                _character.onSetPassengingVehicle += OnVehicleChanged;
                _character.onNonEquipItemsOperation += OnInventoryChanged;
            }
            BindVehicle();
        }

        private void OnVehicleChanged(BaseGameEntity entity) => BindVehicle();
        private void OnInventoryChanged(LiteNetLibSyncListOp op, int index, CharacterItem oldItem, CharacterItem newItem) => Refresh();

        private void BindVehicle()
        {
            if (_fuel != null)
                _fuel.onFuelChanged -= Refresh;
            _fuel = _character != null && !_character.PassengingVehicleEntity.IsNull()
                ? _character.PassengingVehicleEntity.Entity.GetComponent<VehicleFuelComponent>() : null;
            if (_fuel != null)
                _fuel.onFuelChanged += Refresh;
            Refresh();
        }

        public void Refresh()
        {
            bool visible = _character != null && _fuel != null && _fuel.UsesFuel && _fuel.IsInitialized;
            if (controlsRoot != null && !transform.IsChildOf(controlsRoot.transform))
                controlsRoot.SetActive(visible);
            if (!visible)
                return;
            if (textFuel != null)
                textFuel.text = _fuel.IsEmpty ? "FUEL EMPTY" : $"FUEL {_fuel.RemainingFuel:0.0}/{_fuel.Capacity:0.0} L";
            if (fuelFill != null)
            {
                fuelFill.fillAmount = _fuel.FuelFraction;
                fuelFill.color = _fuel.IsEmpty ? emptyColor : _fuel.FuelFraction <= 0.15f ? lowColor : normalColor;
            }
            if (buttonRefuel != null)
                buttonRefuel.interactable = _fuel.FindRefuelItemIndex(_character) >= 0;
        }

        public void Refuel()
        {
            if (_character == null || _fuel == null)
                return;
            int itemIndex = _fuel.FindRefuelItemIndex(_character);
            if (itemIndex >= 0)
                _character.CallCmdRefuelVehicle(_fuel.ObjectId, itemIndex);
        }
    }
}
