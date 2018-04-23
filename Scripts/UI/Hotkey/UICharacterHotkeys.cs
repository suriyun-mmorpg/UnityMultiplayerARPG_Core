using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterHotkeys : UIBase
{
    public UICharacterHotkeyPair[] uiCharacterHotkeys;

    private Dictionary<string, UICharacterHotkey> cacheUICharacterHotkeys = null;
    public Dictionary<string, UICharacterHotkey> CacheUICharacterHotkeys
    {
        get
        {
            InitCaches();
            return cacheUICharacterHotkeys;
        }
    }

    private void InitCaches()
    {
        if (cacheUICharacterHotkeys == null)
        {
            cacheUICharacterHotkeys = new Dictionary<string, UICharacterHotkey>();
            foreach (var uiCharacterHotkey in uiCharacterHotkeys)
            {
                var id = uiCharacterHotkey.hotkeyId;
                var ui = uiCharacterHotkey.ui;
                if (!string.IsNullOrEmpty(id) && ui != null && !cacheUICharacterHotkeys.ContainsKey(id))
                {
                    var characterHotkey = new CharacterHotkey();
                    characterHotkey.hotkeyId = id;
                    characterHotkey.type = HotkeyType.None;
                    characterHotkey.dataId = string.Empty;
                    ui.Setup(characterHotkey, -1);
                    cacheUICharacterHotkeys.Add(id, ui);
                }
            }
        }
    }

    public void UpdateData(IPlayerCharacterData characterData)
    {
        InitCaches();
        var characterHotkeys = characterData.Hotkeys;
        for (var i = 0; i < characterHotkeys.Count; ++i)
        {
            var characterHotkey = characterHotkeys[i];
            UICharacterHotkey ui;
            if (!string.IsNullOrEmpty(characterHotkey.hotkeyId) && CacheUICharacterHotkeys.TryGetValue(characterHotkey.hotkeyId, out ui))
                ui.Setup(characterHotkey, i);
        }
    }
}
