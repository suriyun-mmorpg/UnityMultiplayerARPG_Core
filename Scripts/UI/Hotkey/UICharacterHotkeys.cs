using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkeys : UIBase
    {
        public const string HOTKEY_AXIS_X = "HotkeyAxisX";
        public const string HOTKEY_AXIS_Y = "HotkeyAxisY";

        public IPlayerCharacterData character { get; protected set; }
        public List<ItemType> filterItemTypes = new List<ItemType>() { ItemType.Armor, ItemType.Shield, ItemType.Weapon, ItemType.Potion, ItemType.Building, ItemType.Pet, ItemType.Mount, ItemType.AttributeIncrease, ItemType.AttributeReset, ItemType.Skill, ItemType.SkillLearn, ItemType.SkillReset };
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
        public static Vector3? HotkeyAimPosition { get; private set; }
        private readonly List<UICharacterHotkeyJoystickEventHandler> hotkeyJoysticks = new List<UICharacterHotkeyJoystickEventHandler>();

        private Dictionary<string, List<UICharacterHotkey>> cacheUICharacterHotkeys;
        public Dictionary<string, List<UICharacterHotkey>> CacheUICharacterHotkeys
        {
            get
            {
                InitCaches();
                return cacheUICharacterHotkeys;
            }
        }

        private UICharacterHotkeySelectionManager cacheCharacterHotkeySelectionManager;
        public UICharacterHotkeySelectionManager CacheCharacterHotkeySelectionManager
        {
            get
            {
                if (cacheCharacterHotkeySelectionManager == null)
                    cacheCharacterHotkeySelectionManager = GetComponent<UICharacterHotkeySelectionManager>();
                if (cacheCharacterHotkeySelectionManager == null)
                    cacheCharacterHotkeySelectionManager = gameObject.AddComponent<UICharacterHotkeySelectionManager>();
                cacheCharacterHotkeySelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCharacterHotkeySelectionManager;
            }
        }

        private void InitCaches()
        {
            if (cacheUICharacterHotkeys == null)
            {
                CacheCharacterHotkeySelectionManager.DeselectSelectedUI();
                CacheCharacterHotkeySelectionManager.Clear();
                int j = 0;
                cacheUICharacterHotkeys = new Dictionary<string, List<UICharacterHotkey>>();
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
                        if (!cacheUICharacterHotkeys.ContainsKey(id))
                            cacheUICharacterHotkeys.Add(id, new List<UICharacterHotkey>());
                        cacheUICharacterHotkeys[id].Add(ui);
                        CacheCharacterHotkeySelectionManager.Add(ui);
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

        private void Update()
        {
            if (InputManager.useMobileInputOnNonMobile || Application.isMobilePlatform)
                UpdateHotkeyMobileInputs();
            else
                UpdateHotkeyInputs();
        }

        public override void Hide()
        {
            CacheCharacterHotkeySelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        public void UpdateData(IPlayerCharacterData character)
        {
            this.character = character;
            InitCaches();
            IList<CharacterHotkey> characterHotkeys = character.Hotkeys;
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
        public void SetUsingHotkey(UICharacterHotkey hotkey)
        {
            if (IsAnyHotkeyJoyStickDragging())
                return;
            // Cancel old using hotkey
            if (UsingHotkey != null)
            {
                UsingHotkey.FinishAimControls();
                UsingHotkey = null;
                HotkeyAimPosition = null;
            }
            UsingHotkey = hotkey;
        }

        /// <summary>
        /// Update hotkey input for PC devices
        /// </summary>
        private void UpdateHotkeyInputs()
        {
            if (UsingHotkey == null)
                return;

            HotkeyAimPosition = UsingHotkey.UpdateAimControls(Vector2.zero);
            if (Input.GetMouseButtonDown(0))
                FinishHotkeyAimControls(false);
        }

        /// <summary>
        /// Update hotkey input for Mobile devices
        /// </summary>
        private void UpdateHotkeyMobileInputs()
        {
            if (hotkeyCancelArea != null)
                hotkeyCancelArea.gameObject.SetActive(IsAnyHotkeyJoyStickDragging());
        }

        public void FinishHotkeyAimControls(bool hotkeyCancel)
        {
            if (UsingHotkey == null)
                return;

            UsingHotkey.FinishAimControls();
            if (!hotkeyCancel)
            {
                // Use hotkey
                UsingHotkey.Use(HotkeyAimPosition);
            }

            UsingHotkey = null;
            HotkeyAimPosition = null;
        }

        public void RegisterHotkeyJoystick(UICharacterHotkeyJoystickEventHandler hotkeyJoystick)
        {
            if (!hotkeyJoysticks.Contains(hotkeyJoystick))
                hotkeyJoysticks.Add(hotkeyJoystick);
        }

        public bool IsAnyHotkeyJoyStickDragging()
        {
            foreach (UICharacterHotkeyJoystickEventHandler hotkeyJoystick in hotkeyJoysticks)
            {
                if (hotkeyJoystick.IsDragging) return true;
            }
            return false;
        }
        #endregion
    }
}
