using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public delegate void GenericDelegate();
    public delegate void NetworkDestroyDelegate(
        byte reasons);
    public delegate void ReceiveDamageDelegate(
        IAttackerEntity attacker,
        CharacterItem weapon,
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
        CharacterBuff debuff);
    public delegate void ReceivedDamage(
        IAttackerEntity attacker,
        CombatAmountType combatAmountType,
        int damage);
    public delegate void AttackRoutineDelegate(
        AnimActionType animActionType,
        int weaponTypeDataId,
        int animationIndex,
        float triggerDuration,
        float totalDuration,
        bool isLeftHand,
        CharacterItem weapon,
        DamageInfo damageInfo,
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts);
    public delegate void UseSkillRoutineDelegate(
        Skill skill,
        short level,
        AnimActionType animActionType,
        int skillOrWeaponTypeDataId,
        int animationIndex,
        float triggerDuration,
        float totalDuration,
        bool isLeftHand,
        CharacterItem weapon,
        DamageInfo damageInfo,
        Dictionary<DamageElement, MinMaxFloat> allDamageAmounts,
        bool hasAimPosition,
        Vector3 aimPosition);
    public delegate void ApplyBuffDelegate(
        int dataId,
        BuffType type,
        short level);
}
