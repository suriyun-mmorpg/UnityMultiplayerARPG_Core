using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkeys : UIBase
    {
        public UICharacterHotkeyPair[] uiCharacterHotkeys;
        public UICharacterSkill uiCharacterSkillPrefab;
        public UICharacterItem uiCharacterItemPrefab;

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
                        characterHotkey.dataId = 0;
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

        public override void Hide()
        {
            CacheCharacterHotkeySelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        public void UpdateData()
        {
            InitCaches();
            IList<CharacterHotkey> characterHotkeys = BasePlayerCharacterController.OwningCharacter.Hotkeys;
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
    }
}
