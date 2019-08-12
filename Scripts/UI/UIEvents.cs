using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

namespace MultiplayerARPG
{
    #region Events for UI Character
    [System.Serializable]
    public class CharacterDataEvent : UnityEvent<ICharacterData> { }

    [System.Serializable]
    public class UICharacterEvent : UnityEvent<UICharacter> { }
    #endregion

    #region Events for UI Character Class
    [System.Serializable]
    public class CharacterClassEvent : UnityEvent<BaseCharacter> { }

    [System.Serializable]
    public class UICharacterClassEvent : UnityEvent<UICharacterClass> { }
    #endregion

    #region Events for UI Character Buff
    [System.Serializable]
    public class CharacterBuffEvent : UnityEvent<CharacterBuff> { }

    [System.Serializable]
    public class UICharacterBuffEvent : UnityEvent<UICharacterBuff> { }
    #endregion

    #region Events for UI Character Hotkey
    [System.Serializable]
    public class CharacterHotkeyEvent : UnityEvent<CharacterHotkey> { }

    [System.Serializable]
    public class UICharacterHotkeyEvent : UnityEvent<UICharacterHotkey> { }
    #endregion

    #region Events for UI Character Item
    [System.Serializable]
    public class CharacterItemEvent : UnityEvent<CharacterItemTuple> { }

    [System.Serializable]
    public class UICharacterItemEvent : UnityEvent<UICharacterItem> { }
    #endregion

    #region Events for UI Craft Item
    [System.Serializable]
    public class CraftItemEvent : UnityEvent<ItemCraft> { }

    [System.Serializable]
    public class UICraftItemEvent : UnityEvent<UICraftItem> { }
    #endregion

    #region Events for UI Character Quest
    [System.Serializable]
    public class CharacterQuestEvent : UnityEvent<CharacterQuest> { }

    [System.Serializable]
    public class UICharacterQuestEvent : UnityEvent<UICharacterQuest> { }
    #endregion

    #region Events for UI Character Skill
    [System.Serializable]
    public class CharacterSkillEvent : UnityEvent<CharacterSkillTuple> { }

    [System.Serializable]
    public class UICharacterSkillEvent : UnityEvent<UICharacterSkill> { }
    #endregion

    #region Events for UI Character Summon
    [System.Serializable]
    public class CharacterSummonEvent : UnityEvent<CharacterSummon> { }

    [System.Serializable]
    public class UICharacterSummonEvent : UnityEvent<UICharacterSummon> { }
    #endregion

    #region Events for UI Cash Shop Item
    [System.Serializable]
    public class CashShopItemEvent : UnityEvent<CashShopItem> { }

    [System.Serializable]
    public class UICashShopItemEvent : UnityEvent<UICashShopItem> { }
    #endregion

    #region Events for UI Cash Package
    [System.Serializable]
    public class CashPackageEvent : UnityEvent<CashPackage> { }

    [System.Serializable]
    public class UICashPackageEvent : UnityEvent<UICashPackage> { }
    #endregion

    #region Events for UI Social Character
    [System.Serializable]
    public class SocialCharacterEvent : UnityEvent<SocialCharacterEntityTuple> { }

    [System.Serializable]
    public class UISocialCharacterEvent : UnityEvent<UISocialCharacter> { }
    #endregion

    #region Events for UI Guild Role
    [System.Serializable]
    public class GuildRoleEvent : UnityEvent<GuildRoleData> { }

    [System.Serializable]
    public class UIGuildRoleEvent : UnityEvent<UIGuildRole> { }
    #endregion

    #region Events for UI Guild Skill
    [System.Serializable]
    public class GuildSkillEvent : UnityEvent<GuildSkillTuple> { }

    [System.Serializable]
    public class UIGuildSkillEvent : UnityEvent<UIGuildSkill> { }
    #endregion

    #region Events for UI Faction
    [System.Serializable]
    public class FactionEvent : UnityEvent<Faction> { }

    [System.Serializable]
    public class UIFactionEvent : UnityEvent<UIFaction> { }
    #endregion
}
