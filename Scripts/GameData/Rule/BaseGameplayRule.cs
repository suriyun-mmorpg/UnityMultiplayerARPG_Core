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
        public abstract float GetAttackSpeed(BaseCharacterEntity character);
        public abstract float GetTotalWeight(ICharacterData character);
        public abstract short GetTotalSlot(ICharacterData character);
        public abstract bool IsHungry(BaseCharacterEntity character);
        public abstract bool IsThirsty(BaseCharacterEntity character);
        public abstract bool RewardExp(BaseCharacterEntity character, Reward reward, float multiplier, RewardGivenType rewardGivenType);
        public abstract void RewardCurrencies(BaseCharacterEntity character, Reward reward, float multiplier, RewardGivenType rewardGivenType);
        public abstract float GetEquipmentStatsRate(CharacterItem characterItem);
        public abstract void OnCharacterReceivedDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, CombatAmountType combatAmountType, int damage);
        public abstract void OnHarvestableReceivedDamage(BaseCharacterEntity attacker, HarvestableEntity damageReceiver, CombatAmountType combatAmountType, int damage);
        public abstract bool CurrenciesEnoughToBuyItem(IPlayerCharacterData character, NpcSellItem sellItem, short amount);
        public abstract void DecreaseCurrenciesWhenBuyItem(IPlayerCharacterData character, NpcSellItem sellItem, short amount);
        public abstract void IncreaseCurrenciesWhenSellItem(IPlayerCharacterData character, Item item, short amount);
        public abstract bool CurrenciesEnoughToRefineItem(IPlayerCharacterData character, ItemRefineLevel refineLevel);
        public abstract void DecreaseCurrenciesWhenRefineItem(IPlayerCharacterData character, ItemRefineLevel refineLevel);
        public abstract bool CurrenciesEnoughToRepairItem(IPlayerCharacterData character, ItemRepairPrice repairPrice);
        public abstract void DecreaseCurrenciesWhenRepairItem(IPlayerCharacterData character, ItemRepairPrice repairPrice);
        public abstract bool CurrenciesEnoughToCraftItem(IPlayerCharacterData character, ItemCraft itemCraft);
        public abstract void DecreaseCurrenciesWhenCraftItem(IPlayerCharacterData character, ItemCraft itemCraft);
        public abstract bool CurrenciesEnoughToCreateGuild(IPlayerCharacterData character, SocialSystemSetting setting);
        public abstract void DecreaseCurrenciesWhenCreateGuild(IPlayerCharacterData character, SocialSystemSetting setting);
        public abstract Reward MakeMonsterReward(MonsterCharacter monster);
        public abstract Reward MakeQuestReward(Quest quest);
        public abstract float GetRecoveryUpdateDuration();
    }
}
