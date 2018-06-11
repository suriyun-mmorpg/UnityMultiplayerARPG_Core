using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICharacterHotkeys : UIBase
{
    public UICharacterHotkeyPair[] uiCharacterHotkeys;

    private Dictionary<string, List<UICharacterHotkey>> cacheUICharacterHotkeys = null;
    public Dictionary<string, List<UICharacterHotkey>> CacheUICharacterHotkeys
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
            cacheUICharacterHotkeys = new Dictionary<string, List<UICharacterHotkey>>();
            foreach (var uiCharacterHotkey in uiCharacterHotkeys)
            {
                var id = uiCharacterHotkey.hotkeyId;
                var ui = uiCharacterHotkey.ui;
                if (!string.IsNullOrEmpty(id) && ui != null)
                {
                    var characterHotkey = new CharacterHotkey();
                    characterHotkey.hotkeyId = id;
                    characterHotkey.type = HotkeyType.None;
                    characterHotkey.dataId = 0;
                    ui.Setup(characterHotkey, -1);
                    if (!cacheUICharacterHotkeys.ContainsKey(id))
                        cacheUICharacterHotkeys.Add(id, new List<UICharacterHotkey>());
                    cacheUICharacterHotkeys[id].Add(ui);
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
            List<UICharacterHotkey> uis;
            if (!string.IsNullOrEmpty(characterHotkey.hotkeyId) && CacheUICharacterHotkeys.TryGetValue(characterHotkey.hotkeyId, out uis))
            {
                foreach (var ui in uis)
                {
                    ui.Setup(characterHotkey, i);
                }
            }
        }
    }
}
