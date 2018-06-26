using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class CharacterDataEvent : UnityEvent<ICharacterData> { }

    [System.Serializable]
    public class UICharacterEvent : UnityEvent<UICharacter> { }

    [System.Serializable]
    public class CharacterBuffEvent : UnityEvent<CharacterBuff> { }

    [System.Serializable]
    public class UICharacterBuffEvent : UnityEvent<UICharacterBuff> { }

    [System.Serializable]
    public class CharacterHotkeyEvent : UnityEvent<CharacterHotkey> { }

    [System.Serializable]
    public class UICharacterHotkeyEvent : UnityEvent<UICharacterHotkey> { }

    [System.Serializable]
    public class CharacterItemEvent : UnityEvent<CharacterItemLevelTuple> { }

    [System.Serializable]
    public class UICharacterItemEvent : UnityEvent<UICharacterItem> { }

    [System.Serializable]
    public class CharacterQuestEvent : UnityEvent<CharacterQuest> { }

    [System.Serializable]
    public class UICharacterQuestEvent : UnityEvent<UICharacterQuest> { }

    [System.Serializable]
    public class CharacterSkillEvent : UnityEvent<CharacterSkillLevelTuple> { }

    [System.Serializable]
    public class UICharacterSkillEvent : UnityEvent<UICharacterSkill> { }
}
