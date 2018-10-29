using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseGameplayRule : ScriptableObject
    {
        public abstract float GetHitChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
        public abstract float GetCriticalChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
        public abstract float GetCriticalDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage);
        public abstract float GetBlockChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
        public abstract float GetBlockDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage);
        public abstract float GetDamageReducedByResistance(BaseCharacterEntity damageReceiver, float damageAmount, DamageElement damageElement);
        public abstract float GetRecoveryHpPerSeconds(BaseCharacterEntity character);
        public abstract float GetRecoveryMpPerSeconds(BaseCharacterEntity character);
        public abstract float GetRecoveryStaminaPerSeconds(BaseCharacterEntity character);
        public abstract float GetDecreasingHpPerSeconds(BaseCharacterEntity character);
        public abstract float GetDecreasingMpPerSeconds(BaseCharacterEntity character);
        public abstract float GetDecreasingStaminaPerSeconds(BaseCharacterEntity character);
        public abstract float GetDecreasingFoodPerSeconds(BaseCharacterEntity character);
        public abstract float GetDecreasingWaterPerSeconds(BaseCharacterEntity character);
        public abstract float GetExpLostPercentageWhenDeath(BaseCharacterEntity character);
        public abstract float GetMoveSpeed(BaseCharacterEntity character);
        public abstract bool IsHungry(BaseCharacterEntity character);
        public abstract bool IsThirsty(BaseCharacterEntity character);
        public abstract bool IncreaseExp(BaseCharacterEntity character, int exp);
        public abstract float GetEquipmentBonusRate(CharacterItem characterItem);
        public abstract void OnCharacterReceivedDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, CombatAmountType combatAmountType, int damage);
        public abstract void OnHarvestableReceivedDamage(BaseCharacterEntity attacker, HarvestableEntity damageReceiver, CombatAmountType combatAmountType, int damage);
    }
}
