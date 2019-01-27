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
    public class CharacterClassEvent : UnityEvent<BaseCharacter> { }

    [System.Serializable]
    public class UICharacterClassEvent : UnityEvent<UICharacterClass> { }

    [System.Serializable]
    public class CharacterBuffEvent : UnityEvent<CharacterBuff> { }

    [System.Serializable]
    public class UICharacterBuffEvent : UnityEvent<UICharacterBuff> { }

    [System.Serializable]
    public class CharacterHotkeyEvent : UnityEvent<CharacterHotkey> { }

    [System.Serializable]
    public class UICharacterHotkeyEvent : UnityEvent<UICharacterHotkey> { }

    [System.Serializable]
    public class CharacterItemEvent : UnityEvent<CharacterItemTuple> { }

    [System.Serializable]
    public class UICharacterItemEvent : UnityEvent<UICharacterItem> { }

    [System.Serializable]
    public class CharacterQuestEvent : UnityEvent<CharacterQuest> { }

    [System.Serializable]
    public class UICharacterQuestEvent : UnityEvent<UICharacterQuest> { }

    [System.Serializable]
    public class CharacterSkillEvent : UnityEvent<SkillTuple> { }

    [System.Serializable]
    public class UICharacterSkillEvent : UnityEvent<UICharacterSkill> { }

    [System.Serializable]
    public class CharacterSummonEvent : UnityEvent<CharacterSummon> { }

    [System.Serializable]
    public class UICharacterSummonEvent : UnityEvent<UICharacterSummon> { }

    [System.Serializable]
    public class CashShopItemEvent : UnityEvent<CashShopItem> { }

    [System.Serializable]
    public class UICashShopItemEvent : UnityEvent<UICashShopItem> { }

    [System.Serializable]
    public class CashPackageEvent : UnityEvent<CashPackage> { }

    [System.Serializable]
    public class UICashPackageEvent : UnityEvent<UICashPackage> { }

    [System.Serializable]
    public class SocialCharacterEvent : UnityEvent<SocialCharacterEntityTuple> { }

    [System.Serializable]
    public class UISocialCharacterEvent : UnityEvent<UISocialCharacter> { }

    [System.Serializable]
    public class GuildRoleEvent : UnityEvent<GuildRoleData> { }

    [System.Serializable]
    public class UIGuildRoleEvent : UnityEvent<UIGuildRole> { }

    [System.Serializable]
    public class GuildSkillEvent : UnityEvent<GuildSkillTuple> { }

    [System.Serializable]
    public class UIGuildSkillEvent : UnityEvent<UIGuildSkill> { }
}
