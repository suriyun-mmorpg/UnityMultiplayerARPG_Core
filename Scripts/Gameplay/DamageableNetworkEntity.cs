using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

public abstract class DamageableNetworkEntity : RpgNetworkEntity
{
    [SerializeField]
    protected SyncFieldInt currentHp = new SyncFieldInt();

    public virtual int CurrentHp { get { return currentHp.Value; } set { currentHp.Value = value; } }

    public abstract void ReceiveDamage(BaseCharacterEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, int hitEffectsId);
}
