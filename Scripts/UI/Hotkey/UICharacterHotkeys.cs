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
        public UICharacterHotkeyPair[] uiCharacterHotkeys;
        public UICharacterSkill uiCharacterSkillPrefab;
        public UICharacterItem uiCharacterItemPrefab;

        [Header("Mobile Touch Controls")]
        [FormerlySerializedAs("hotkeyMovementJoyStick")]
        public MobileMovementJoystick hotkeyAimJoyStick;
        public RectTransform hotkeyCancelArea;
        private UICharacterHotkey draggingHotkey;
        private Vector2 hotkeyAxis;
        private CanvasGroup hotkeyAimJoyStickGroup;
        private CanvasGroup hotkeyCancelAreaGroup;
        public bool hotkeyCancel { get; private set; }
        public bool hotkeyStartDragged { get; private set; }

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
                        ui.Setup(this, characterHotkey, -1);
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

        private void Start()
        {
            if (hotkeyAimJoyStick != null)
            {
                hotkeyAimJoyStickGroup = hotkeyAimJoyStick.GetComponent<CanvasGroup>();
                if (hotkeyAimJoyStickGroup == null)
                    hotkeyAimJoyStickGroup = hotkeyAimJoyStick.gameObject.AddComponent<CanvasGroup>();
            }

            if (hotkeyCancelArea != null)
            {
                hotkeyCancelAreaGroup = hotkeyCancelArea.GetComponent<CanvasGroup>();
                if (hotkeyCancelAreaGroup == null)
                    hotkeyCancelAreaGroup = hotkeyCancelArea.gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void Update()
        {
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
                        ui.Setup(this, characterHotkey, i);
                        ui.Show();
                    }
                }
            }
        }

        #region Mobile Controls
        public void EnableHotkeyJoystick(UICharacterHotkey hotkey)
        {
            if (hotkey == null || hotkeyStartDragged)
                return;
            // Set axis and key
            hotkeyAimJoyStick.axisXName = HOTKEY_AXIS_X;
            hotkeyAimJoyStick.axisYName = HOTKEY_AXIS_Y;
            draggingHotkey = hotkey;
            // Set joystick position to the same position with hotkey button
            hotkeyAimJoyStick.transform.position = hotkey.transform.position;
        }

        private void UpdateHotkeyInputs()
        {
            // No joy stick set, return
            if (hotkeyAimJoyStick == null)
                return;

            if (hotkeyAimJoyStickGroup != null)
                hotkeyAimJoyStickGroup.alpha = hotkeyStartDragged ? 1 : 0;

            if (hotkeyCancelAreaGroup != null)
                hotkeyCancelAreaGroup.alpha = hotkeyStartDragged ? 1 : 0;

            if (hotkeyAimJoyStick != null)
                hotkeyAimJoyStick.gameObject.SetActive(draggingHotkey != null);

            if (hotkeyCancelArea != null)
                hotkeyCancelArea.gameObject.SetActive(draggingHotkey != null);

            if (draggingHotkey == null)
                return;

            hotkeyAxis = new Vector2(InputManager.GetAxis(HOTKEY_AXIS_X, false), InputManager.GetAxis(HOTKEY_AXIS_Y, false));
            hotkeyCancel = false;

            if (hotkeyCancelArea != null)
            {
                Vector3 localMousePosition = hotkeyCancelArea.InverseTransformPoint(hotkeyAimJoyStick.CurrentPosition);
                if (hotkeyCancelArea.rect.Contains(localMousePosition))
                {
                    hotkeyCancel = true;
                }
            }

            if (!hotkeyStartDragged && hotkeyAimJoyStick.IsDragging)
            {
                hotkeyStartDragged = true;
            }

            if (hotkeyStartDragged && !hotkeyAimJoyStick.IsDragging)
            {
                if (!hotkeyCancel)
                {
                    // Use hotkey
                    draggingHotkey.OnClickUse();
                }
                draggingHotkey = null;
                hotkeyStartDragged = false;
            }
        }
        #endregion
    }
}
