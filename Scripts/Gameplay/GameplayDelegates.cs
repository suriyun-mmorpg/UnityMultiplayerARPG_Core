using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public delegate void GenericDelegate();
    public delegate void NetworkDestroyDelegate(
        byte reasons);
    public delegate void ReceiveDamageDelegate(
        IGameEntity attacker,
        CharacterItem weapon,
        Dictionary<DamageElement, MinMaxFloat> damageAmounts,
        BaseSkill skill,
        short skillLevel);
    public delegate void ReceivedDamage(
        IGameEntity attacker,
        CombatAmountType combatAmountType,
        int damage);
    public delegate void AttackRoutineDelegate(
        bool isLeftHand,
        CharacterItem weapon,
        int hitIndex,
        Dictionary<DamageElement, MinMaxFloat> damageAmounts,
        Vector3 aimPosition);
    public delegate void UseSkillRoutineDelegate(
        BaseSkill skill,
        short level,
        bool isLeftHand,
        CharacterItem weapon,
        int hitIndex,
        Dictionary<DamageElement, MinMaxFloat> damageAmounts,
        Vector3 aimPosition);
    public delegate void ApplyBuffDelegate(
        int dataId,
        BuffType type,
        short level);
}
