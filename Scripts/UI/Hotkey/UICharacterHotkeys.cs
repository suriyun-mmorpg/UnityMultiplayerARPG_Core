using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkeys : UIBase
    {
        public List<string> filterCategories = new List<string>();
        public bool doNotIncludeItems;
        public List<ItemType> filterItemTypes = new List<ItemType>() { ItemType.Armor, ItemType.Shield, ItemType.Weapon, ItemType.Potion, ItemType.Building, ItemType.Pet, ItemType.Mount, ItemType.Skill };
        public bool doNotIncludeSkills;
        public List<SkillType> filterSkillTypes = new List<SkillType>() { SkillType.Active, SkillType.CraftItem };
        public UICharacterHotkeyAssigner uiCharacterHotkeyAssigner;
        public UICharacterHotkeyPair[] uiCharacterHotkeys;
        public UICharacterSkill uiCharacterSkillPrefab;
        public UICharacterItem uiCharacterItemPrefab;

        [Header("Mobile Touch Controls")]
        [FormerlySerializedAs("hotkeyMovementJoyStick")]
        [FormerlySerializedAs("hotkeyAimJoyStick")]
        public MobileMovementJoystick hotkeyAimJoyStickPrefab;
        public RectTransform hotkeyCancelArea;
        public static UICharacterHotkey UsingHotkey { get; private set; }
        public static AimPosition HotkeyAimPosition { get; private set; }
        private static UICharacterHotkey s_otherHotkey;
        /// <summary>
        /// The hotkey which will be used by other components
        /// </summary>
        public static UICharacterHotkey OtherHotkey
        {
            get
            {
                if (s_otherHotkey == null)
                {
                    s_otherHotkey = new GameObject("_OtherHotkey").AddComponent<UICharacterHotkey>();
                    s_otherHotkey.transform.localScale = Vector3.zero;
                }
                return s_otherHotkey;
            }
        }
        private static readonly List<IHotkeyJoystickEventHandler> hotkeyJoysticks = new List<IHotkeyJoystickEventHandler>();

        private Dictionary<string, List<UICharacterHotkey>> _cacheUICharacterHotkeys;
        public Dictionary<string, List<UICharacterHotkey>> CacheUICharacterHotkeys
        {
            get
            {
                InitCaches();
                return _cacheUICharacterHotkeys;
            }
        }

        private UICharacterHotkeySelectionManager _cacheSelectionManager;
        public UICharacterHotkeySelectionManager CacheSelectionManager
        {
            get
            {
                if (_cacheSelectionManager == null)
                    _cacheSelectionManager = gameObject.GetOrAddComponent<UICharacterHotkeySelectionManager>();
                _cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return _cacheSelectionManager;
            }
        }

        private void InitCaches()
        {
            if (_cacheUICharacterHotkeys == null)
            {
                CacheSelectionManager.DeselectSelectedUI();
                CacheSelectionManager.Clear();
                int j = 0;
                _cacheUICharacterHotkeys = new Dictionary<string, List<UICharacterHotkey>>();
                for (int i = 0; i < uiCharacterHotkeys.Length; ++i)
                {
                    UICharacterHotkeyPair uiCharacterHotkey = uiCharacterHotkeys[i];
                    string id = uiCharacterHotkey.hotkeyId;
                    UICharacterHotkey ui = uiCharacterHotkey.ui;
                    if (!string.IsNullOrEmpty(id) && ui != null)
                    {
                        CharacterHotkey characterHotkey = new CharacterHotkey();
                        characterHotkey.hotkeyId = id;
                        characterHotkey.type = HotkeyType.None;
                        characterHotkey.relateId = string.Empty;
                        ui.Setup(this, uiCharacterHotkeyAssigner, characterHotkey, -1);
                        if (!_cacheUICharacterHotkeys.ContainsKey(id))
                            _cacheUICharacterHotkeys.Add(id, new List<UICharacterHotkey>());
                        _cacheUICharacterHotkeys[id].Add(ui);
                        CacheSelectionManager.Add(ui);
                        // Select first UI
                        if (j == 0)
                            ui.OnClickSelect();
                        ++j;
                    }
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            // Deactivate this because this variable used to be in-scene object variable
            // but now it is a variable for a prefab.
            if (hotkeyAimJoyStickPrefab != null)
                hotkeyAimJoyStickPrefab.gameObject.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            UpdateData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onSkillsOperation += OnSkillsOperation;
            GameInstance.PlayingCharacterEntity.onHotkeysOperation += OnHotkeysOperation;
        }

        protected virtual void OnDisable()
        {
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onSkillsOperation -= OnSkillsOperation;
            GameInstance.PlayingCharacterEntity.onHotkeysOperation -= OnHotkeysOperation;
        }

        private void OnEquipWeaponSetChange(byte equipWeaponSet)
        {
            UpdateData();
        }

        private void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void OnHotkeysOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateData();
        }

        private void Update()
        {
            if (InputManager.UseMobileInput())
                UpdateHotkeyMobileInputs();
            else
                UpdateHotkeyInputs();
        }

        public override void Hide()
        {
            CacheSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        public virtual void UpdateData()
        {
            InitCaches();
            IList<CharacterHotkey> characterHotkeys = GameInstance.PlayingCharacterEntity.Hotkeys;
            for (int i = 0; i < characterHotkeys.Count; ++i)
            {
                CharacterHotkey characterHotkey = characterHotkeys[i];
                List<UICharacterHotkey> uis;
                if (!string.IsNullOrEmpty(characterHotkey.hotkeyId) && CacheUICharacterHotkeys.TryGetValue(characterHotkey.hotkeyId, out uis))
                {
                    foreach (UICharacterHotkey ui in uis)
                    {
                        ui.Setup(this, uiCharacterHotkeyAssigner, characterHotkey, i);
                        ui.Show();
                    }
                }
            }
        }

        #region Mobile Controls
        public static void SetUsingHotkey(UICharacterHotkey hotkey)
        {
            if (IsAnyHotkeyJoyStickDragging())
                return;
            // Cancel old using hotkey
            if (UsingHotkey != null)
            {
                UsingHotkey.FinishAimControls(true, HotkeyAimPosition);
                UsingHotkey = null;
                HotkeyAimPosition = default;
            }
            UsingHotkey = hotkey;
            if (UsingHotkey != null && UsingHotkey.IsChanneledAbility())
                UsingHotkey.StartChanneledAbility();
        }

        /// <summary>
        /// Update hotkey input for PC devices
        /// </summary>
        private void UpdateHotkeyInputs()
        {
            if (UsingHotkey == null)
                return;
            HotkeyAimPosition = UsingHotkey.UpdateAimControls(Vector2.zero);
            // Click anywhere (on the map) to use skill
            if (InputManager.GetMouseButtonDown(0) && !UsingHotkey.IsChanneledAbility() && !UIBlockController.IsBlockController())
                FinishHotkeyAimControls(false);
        }

        /// <summary>
        /// Update hotkey input for Mobile devices
        /// </summary>
        private void UpdateHotkeyMobileInputs()
        {
            bool isAnyHotkeyJoyStickDragging = false;
            for (int i = 0; i < hotkeyJoysticks.Count; ++i)
            {
                if (hotkeyJoysticks[i] == null)
                    continue;
                hotkeyJoysticks[i].UpdateEvent();
                if (UsingHotkey == hotkeyJoysticks[i].UICharacterHotkey)
                    HotkeyAimPosition = hotkeyJoysticks[i].AimPosition;
                if (hotkeyJoysticks[i].IsDragging)
                    isAnyHotkeyJoyStickDragging = true;
            }

            if (hotkeyCancelArea != null)
                hotkeyCancelArea.gameObject.SetActive(isAnyHotkeyJoyStickDragging);
        }

        public static void FinishHotkeyAimControls(bool isCancel)
        {
            if (UsingHotkey == null)
                return;
            UsingHotkey.FinishAimControls(isCancel, HotkeyAimPosition);
            UsingHotkey = null;
            HotkeyAimPosition = default;
        }

        public void RegisterHotkeyJoystick(IHotkeyJoystickEventHandler hotkeyJoystick)
        {
            if (!hotkeyJoysticks.Contains(hotkeyJoystick))
                hotkeyJoysticks.Add(hotkeyJoystick);
        }

        public static bool IsAnyHotkeyJoyStickDragging()
        {
            foreach (IHotkeyJoystickEventHandler hotkeyJoystick in hotkeyJoysticks)
            {
                if (hotkeyJoystick == null)
                    continue;
                if (hotkeyJoystick.IsDragging)
                    return true;
            }
            return false;
        }
        #endregion

        public static void SetupAndUseOtherHotkey(HotkeyType type, string relateId)
        {
            CharacterHotkey hotkey = new CharacterHotkey();
            hotkey.type = type;
            hotkey.relateId = relateId;
            OtherHotkey.Data = hotkey;
            OtherHotkey.OnClickUse();
        }
    }
}
