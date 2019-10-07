using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterHotkey))]
    public class UICharacterHotkeyJoystickEventHandler : MonoBehaviour, IPointerEnterHandler
    {
        private UICharacterHotkey cacheUiCharacterHotkey;
        public UICharacterHotkey CacheUiCharacterHotkey
        {
            get
            {
                if (cacheUiCharacterHotkey == null)
                    cacheUiCharacterHotkey = GetComponent<UICharacterHotkey>();
                return cacheUiCharacterHotkey;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (CacheUiCharacterHotkey.uiCharacterHotkeys != null)
                CacheUiCharacterHotkey.uiCharacterHotkeys.SetUsingHotkey(CacheUiCharacterHotkey);
        }
    }
}
