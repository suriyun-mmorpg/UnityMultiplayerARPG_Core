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
                        characterHotkey.type = HotkeyTypes.None;
                        characterHotkey.dataId = string.Empty;
                        ui.Setup(characterHotkey, -1);
                        cacheUICharacterHotkeys.Add(id, ui);
                    }
                }
            }
            return cacheUICharacterHotkeys;
        }
    }

    public void UpdateData(IPlayerCharacterData characterData)
    {
        var characterHotkeys = characterData.Hotkeys;
        for (var i = 0; i < characterHotkeys.Count; ++i)
        {
            var characterHotkey = characterHotkeys[i];
            UICharacterHotkey ui;
            if (CacheUICharacterHotkeys.TryGetValue(characterHotkey.hotkeyId, out ui))
                ui.Setup(characterHotkey, i);
        }
    }
}
