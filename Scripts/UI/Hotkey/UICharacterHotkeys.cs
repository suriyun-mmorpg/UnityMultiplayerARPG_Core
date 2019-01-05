using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterHotkeySelectionManager))]
    public partial class UICharacterHotkeys : UIBase
    {
        public UICharacterHotkeyPair[] uiCharacterHotkeys;

        private Dictionary<string, List<UICharacterHotkey>> cacheUICharacterHotkeys;
        public Dictionary<string, List<UICharacterHotkey>> CacheUICharacterHotkeys
        {
            get
            {
                InitCaches();
                return cacheUICharacterHotkeys;
            }
        }

        private UICharacterHotkeySelectionManager selectionManager;
        public UICharacterHotkeySelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICharacterHotkeySelectionManager>();
                selectionManager.selectionMode = UISelectionMode.SelectSingle;
                return selectionManager;
            }
        }

        private void InitCaches()
        {
            if (cacheUICharacterHotkeys == null)
            {
                SelectionManager.DeselectSelectedUI();
                SelectionManager.Clear();
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
                        ui.Setup(characterHotkey, -1);
                        if (!cacheUICharacterHotkeys.ContainsKey(id))
                            cacheUICharacterHotkeys.Add(id, new List<UICharacterHotkey>());
                        cacheUICharacterHotkeys[id].Add(ui);
                        SelectionManager.Add(ui);
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
            SelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        public void UpdateData(IPlayerCharacterData characterData)
        {
            InitCaches();
            IList<CharacterHotkey> characterHotkeys = characterData.Hotkeys;
            for (int i = 0; i < characterHotkeys.Count; ++i)
            {
                CharacterHotkey characterHotkey = characterHotkeys[i];
                List<UICharacterHotkey> uis;
                if (!string.IsNullOrEmpty(characterHotkey.hotkeyId) && CacheUICharacterHotkeys.TryGetValue(characterHotkey.hotkeyId, out uis))
                {
                    foreach (UICharacterHotkey ui in uis)
                    {
                        ui.Setup(characterHotkey, i);
                        ui.Show();
                    }
                }
            }
        }
    }
}
