using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>Mounted vehicle HP HUD. Keep the observer outside its hidden controls root.</summary>
    public class UIVehicleHp : MonoBehaviour
    {
        public GameObject controlsRoot;
        public TextWrapper textHp;
        public UnityEngine.UI.Image hpFill;
        [Tooltip("Format: {0} = current HP, {1} = maximum HP")]
        public string textFormat = "HP {0}/{1}";

        private BasePlayerCharacterEntity _character;
        private DamageableEntity _vehicle;

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

        // Also catches changes to maximum HP and vehicle despawning.
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
        private void OnHpChanged(DamageableEntity entity, int oldHp, int hp) => Refresh();

        private void BindVehicle()
        {
            if (_vehicle != null)
                _vehicle.onCurrentHpChange -= OnHpChanged;
            IVehicleEntity vehicle = _character != null ? _character.PassengingVehicleEntity : null;
            _vehicle = !vehicle.IsNull() ? vehicle.Entity as DamageableEntity : null;
            if (_vehicle != null)
                _vehicle.onCurrentHpChange += OnHpChanged;
            Refresh();
        }

        public void Refresh()
        {
            bool visible = _character != null && _vehicle != null;
            if (controlsRoot != null && !transform.IsChildOf(controlsRoot.transform))
                controlsRoot.SetActive(visible);
            if (!visible)
                return;
            int maxHp = Mathf.Max(0, _vehicle.MaxHp);
            int currentHp = Mathf.Clamp(_vehicle.CurrentHp, 0, maxHp);
            if (textHp != null)
                textHp.text = string.Format(textFormat, currentHp, maxHp);
            if (hpFill != null)
                hpFill.fillAmount = maxHp > 0 ? (float)currentHp / maxHp : 0f;
        }
    }
}
