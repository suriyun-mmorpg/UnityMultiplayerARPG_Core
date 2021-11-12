using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameplayRule : ScriptableObject
    {
        public float GoldRate { get; set; } = 1f;
        public float ExpRate { get; set; } = 1f;
        public abstract bool RandomAttackHitOccurs(Vector3 fromPosition, BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed, out bool isCritical, out bool isBlocked);
        public abstract float RandomAttackDamage(Vector3 fromPosition, BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, DamageElement damageElement, MinMaxFloat damageAmount, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed);
        public abstract float GetHitChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
        public abstract float GetCriticalChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
        public abstract float GetCriticalDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage);
        public abstract float GetBlockChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
        public abstract float GetBlockDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage);
        public abstract float GetDamageReducedByResistance(Dictionary<DamageElement, float> damageReceiverResistances, Dictionary<DamageElement, float> damageReceiverArmors, float damageAmount, DamageElement damageElement);
        public abstract int GetTotalDamage(Vector3 fromPosition, EntityInfo instigator, DamageableEntity damageReceiver, float totalDamage, CharacterItem weapon, BaseSkill skill, short skillLevel);
        public abstract float GetRecoveryHpPerSeconds(BaseCharacterEntity character);
        public abstract float GetRecoveryMpPerSeconds(BaseCharacterEntity character);
        public abstract float GetRecoveryStaminaPerSeconds(BaseCharacterEntity character);
        public abstract float GetDecreasingHpPerSeconds(BaseCharacterEntity character);
        public abstract float GetDecreasingMpPerSeconds(BaseCharacterEntity character);
        public abstract float GetDecreasingStaminaPerSeconds(BaseCharacterEntity character);
        public abstract float GetDecreasingFoodPerSeconds(BaseCharacterEntity character);
        public abstract float GetDecreasingWaterPerSeconds(BaseCharacterEntity character);
        public abstract float GetExpLostPercentageWhenDeath(BaseCharacterEntity character);
        public abstract float GetOverweightMoveSpeedRate(BaseGameEntity gameEntity);
        public abstract float GetSprintMoveSpeedRate(BaseGameEntity gameEntity);
        public abstract float GetWalkMoveSpeedRate(BaseGameEntity gameEntity);
        public abstract float GetCrouchMoveSpeedRate(BaseGameEntity gameEntity);
        public abstract float GetCrawlMoveSpeedRate(BaseGameEntity gameEntity);
        public abstract float GetSwimMoveSpeedRate(BaseGameEntity gameEntity);
        public abstract float GetTotalWeight(ICharacterData character, CharacterStats stats);
        public abstract float GetLimitWeight(ICharacterData character, CharacterStats stats);
        public abstract short GetTotalSlot(ICharacterData character, CharacterStats stats);
        public abstract short GetLimitSlot(ICharacterData character, CharacterStats stats);
        public abstract bool IsHungry(BaseCharacterEntity character);
        public abstract bool IsThirsty(BaseCharacterEntity character);
        public abstract bool RewardExp(BaseCharacterEntity character, Reward reward, float multiplier, RewardGivenType rewardGivenType, out int rewardedExp);
        public abstract void RewardCurrencies(BaseCharacterEntity character, Reward reward, float multiplier, RewardGivenType rewardGivenType, out int rewardedGold);
        public abstract float GetEquipmentStatsRate(CharacterItem characterItem);
        public abstract void OnCharacterRespawn(ICharacterData character);
        public abstract void OnCharacterReceivedDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, CombatAmountType combatAmountType, int damage, CharacterItem weapon, BaseSkill skill, short skillLevel);
        public abstract void OnHarvestableReceivedDamage(BaseCharacterEntity attacker, HarvestableEntity damageReceiver, CombatAmountType combatAmountType, int damage, CharacterItem weapon, BaseSkill skill, short skillLevel);
        public abstract bool CurrenciesEnoughToBuyItem(IPlayerCharacterData character, NpcSellItem sellItem, short amount);
        public abstract void DecreaseCurrenciesWhenBuyItem(IPlayerCharacterData character, NpcSellItem sellItem, short amount);
        public abstract void IncreaseCurrenciesWhenSellItem(IPlayerCharacterData character, BaseItem item, short amount);
        public abstract bool CurrenciesEnoughToRefineItem(IPlayerCharacterData character, ItemRefineLevel refineLevel);
        public abstract void DecreaseCurrenciesWhenRefineItem(IPlayerCharacterData character, ItemRefineLevel refineLevel);
        public abstract bool CurrenciesEnoughToRepairItem(IPlayerCharacterData character, ItemRepairPrice repairPrice);
        public abstract void DecreaseCurrenciesWhenRepairItem(IPlayerCharacterData character, ItemRepairPrice repairPrice);
        public abstract bool CurrenciesEnoughToCraftItem(IPlayerCharacterData character, ItemCraft itemCraft);
        public abstract void DecreaseCurrenciesWhenCraftItem(IPlayerCharacterData character, ItemCraft itemCraft);
        public abstract bool CurrenciesEnoughToRemoveEnhancer(IPlayerCharacterData character);
        public abstract void DecreaseCurrenciesWhenRemoveEnhancer(IPlayerCharacterData character);
        public abstract bool CurrenciesEnoughToCreateGuild(IPlayerCharacterData character, SocialSystemSetting setting);
        public abstract void DecreaseCurrenciesWhenCreateGuild(IPlayerCharacterData character, SocialSystemSetting setting);
        public abstract Reward MakeMonsterReward(MonsterCharacter monster, short level);
        public abstract Reward MakeQuestReward(Quest quest);
        public abstract float GetRecoveryUpdateDuration();
        public abstract void ApplyFallDamage(BaseCharacterEntity character, Vector3 lastGroundedPosition);
        public abstract bool CanInteractEntity(BaseCharacterEntity character, uint objectId);
        public abstract Vector3 GetSummonPosition(BaseCharacterEntity character);
        public abstract Quaternion GetSummonRotation(BaseCharacterEntity character);
        public virtual byte GetItemMaxSocket(IPlayerCharacterData character, CharacterItem characterItem)
        {
            IEquipmentItem item = characterItem.GetEquipmentItem();
            return item == null ? (byte)0 : item.MaxSocket;
        }
    }
}
