using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameplayRule : ScriptableObject
    {
        public float GoldRate { get; set; } = 1f;
        public float ExpRate { get; set; } = 1f;
        public abstract bool RandomAttackHitOccurs(Vector3 fromPosition, BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel, int randomSeed, out bool isCritical, out bool isBlocked);
        public abstract float RandomAttackDamage(Vector3 fromPosition, BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, DamageElement damageElement, MinMaxFloat damageAmount, CharacterItem weapon, BaseSkill skill, int skillLevel, int randomSeed);
        public abstract float GetHitChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
        public abstract float GetCriticalChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
        public abstract float GetCriticalDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage);
        public abstract float GetBlockChance(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver);
        public abstract float GetBlockDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage);
        public abstract float GetDamageReducedByResistance(Dictionary<DamageElement, float> damageReceiverResistances, Dictionary<DamageElement, float> damageReceiverArmors, float damageAmount, DamageElement damageElement);
        public abstract int GetTotalDamage(Vector3 fromPosition, EntityInfo instigator, DamageableEntity damageReceiver, float totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel);
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
        public abstract int GetTotalSlot(ICharacterData character, CharacterStats stats);
        public abstract int GetLimitSlot(ICharacterData character, CharacterStats stats);
        public abstract bool IsHungry(BaseCharacterEntity character);
        public abstract bool IsThirsty(BaseCharacterEntity character);
        public abstract bool RewardExp(BaseCharacterEntity character, Reward reward, float multiplier, RewardGivenType rewardGivenType, out int rewardedExp);
        public abstract void RewardCurrencies(BaseCharacterEntity character, Reward reward, float multiplier, RewardGivenType rewardGivenType, out int rewardedGold);
        public abstract float GetEquipmentStatsRate(CharacterItem characterItem);
        public abstract void OnCharacterRespawn(ICharacterData character);
        public abstract void OnCharacterReceivedDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, CombatAmountType combatAmountType, int damage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime);
        public abstract void OnHarvestableReceivedDamage(BaseCharacterEntity attacker, HarvestableEntity damageReceiver, CombatAmountType combatAmountType, int damage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime);
        public abstract float GetRecoveryUpdateDuration();
        public abstract void ApplyFallDamage(BaseCharacterEntity character, Vector3 lastGroundedPosition);
        public abstract bool CanInteractEntity(BaseCharacterEntity character, uint objectId);
        public abstract Vector3 GetSummonPosition(BaseCharacterEntity character);
        public abstract Quaternion GetSummonRotation(BaseCharacterEntity character);
        public abstract float GetBattlePointFromCharacterStats(CharacterStats stats);

        public virtual bool CurrenciesEnoughToBuyItem(IPlayerCharacterData character, NpcSellItem sellItem, int amount)
        {
            if (character.Gold < sellItem.sellPrice * amount)
                return false;
            if (sellItem.sellPrices == null || sellItem.sellPrices.Length == 0)
                return true;
            return character.HasEnoughCurrencyAmounts(GameDataHelpers.CombineCurrencies(sellItem.sellPrices, null), out _, out _, amount);
        }

        public virtual void DecreaseCurrenciesWhenBuyItem(IPlayerCharacterData character, NpcSellItem sellItem, int amount)
        {
            character.Gold -= sellItem.sellPrice * amount;
            if (sellItem.sellPrices == null || sellItem.sellPrices.Length == 0)
                return;
            character.DecreaseCurrencies(sellItem.sellPrices, amount);
        }

        public virtual void IncreaseCurrenciesWhenSellItem(IPlayerCharacterData character, BaseItem item, int amount)
        {
            character.Gold = character.Gold.Increase(item.SellPrice * amount);
        }

        public virtual bool CurrenciesEnoughToRefineItem(IPlayerCharacterData character, ItemRefineLevel refineLevel, float decreaseRate)
        {
            if (character.Gold < GetRefineItemRequireGold(character, refineLevel, decreaseRate))
                return false;
            if (refineLevel.RequireCurrencies == null || refineLevel.RequireCurrencies.Length == 0)
                return true;
            return character.HasEnoughCurrencyAmounts(GameDataHelpers.CombineCurrencies(refineLevel.RequireCurrencies, null), out _, out _);
        }

        public virtual void DecreaseCurrenciesWhenRefineItem(IPlayerCharacterData character, ItemRefineLevel refineLevel, float decreaseRate)
        {
            character.Gold -= GetRefineItemRequireGold(character, refineLevel, decreaseRate);
            if (refineLevel.RequireCurrencies == null || refineLevel.RequireCurrencies.Length == 0)
                return;
            character.DecreaseCurrencies(refineLevel.RequireCurrencies);
        }

        public virtual int GetRefineItemRequireGold(IPlayerCharacterData character, ItemRefineLevel refineLevel, float decreaseRate)
        {
            int price = Mathf.CeilToInt(refineLevel.RequireGold - (refineLevel.RequireGold * decreaseRate));
            if (price < 0)
                price = 0;
            return price;
        }

        public virtual bool CurrenciesEnoughToRepairItem(IPlayerCharacterData character, ItemRepairPrice repairPrice)
        {
            if (character.Gold < repairPrice.RequireGold)
                return false;
            if (repairPrice.RequireCurrencies == null || repairPrice.RequireCurrencies.Length == 0)
                return true;
            return character.HasEnoughCurrencyAmounts(GameDataHelpers.CombineCurrencies(repairPrice.RequireCurrencies, null), out _, out _);
        }

        public virtual void DecreaseCurrenciesWhenRepairItem(IPlayerCharacterData character, ItemRepairPrice repairPrice)
        {
            character.Gold -= repairPrice.RequireGold;
            if (repairPrice.RequireCurrencies == null || repairPrice.RequireCurrencies.Length == 0)
                return;
            character.DecreaseCurrencies(repairPrice.RequireCurrencies);
        }

        public virtual bool CurrenciesEnoughToCraftItem(IPlayerCharacterData character, ItemCraft itemCraft)
        {
            if (character.Gold < itemCraft.RequireGold)
                return false;
            if (itemCraft.RequireCurrencies == null || itemCraft.RequireCurrencies.Length == 0)
                return true;
            return character.HasEnoughCurrencyAmounts(GameDataHelpers.CombineCurrencies(itemCraft.RequireCurrencies, null), out _, out _);
        }

        public virtual void DecreaseCurrenciesWhenCraftItem(IPlayerCharacterData character, ItemCraft itemCraft)
        {
            character.Gold -= itemCraft.RequireGold;
            if (itemCraft.RequireCurrencies == null || itemCraft.RequireCurrencies.Length == 0)
                return;
            character.DecreaseCurrencies(itemCraft.RequireCurrencies);
        }

        public virtual bool CurrenciesEnoughToRemoveEnhancer(IPlayerCharacterData character)
        {
            if (character.Gold < GameInstance.Singleton.enhancerRemoval.RequireGold)
                return false;
            if (GameInstance.Singleton.enhancerRemoval.RequireCurrencies == null || GameInstance.Singleton.enhancerRemoval.RequireCurrencies.Length == 0)
                return true;
            return character.HasEnoughCurrencyAmounts(GameDataHelpers.CombineCurrencies(GameInstance.Singleton.enhancerRemoval.RequireCurrencies, null), out _, out _);
        }

        public virtual void DecreaseCurrenciesWhenRemoveEnhancer(IPlayerCharacterData character)
        {
            character.Gold -= GameInstance.Singleton.enhancerRemoval.RequireGold;
            if (GameInstance.Singleton.enhancerRemoval.RequireCurrencies == null || GameInstance.Singleton.enhancerRemoval.RequireCurrencies.Length == 0)
                return;
            character.DecreaseCurrencies(GameInstance.Singleton.enhancerRemoval.RequireCurrencies);
        }

        public virtual bool CurrenciesEnoughToCreateGuild(IPlayerCharacterData character, SocialSystemSetting setting)
        {
            if (character.Gold < setting.CreateGuildRequiredGold)
                return false;
            if (setting.CreateGuildRequireCurrencies.Count == 0)
                return true;
            return character.HasEnoughCurrencyAmounts(setting.CreateGuildRequireCurrencies, out _, out _);
        }

        public virtual void DecreaseCurrenciesWhenCreateGuild(IPlayerCharacterData character, SocialSystemSetting setting)
        {
            character.Gold -= setting.CreateGuildRequiredGold;
            if (setting.CreateGuildRequireCurrencies.Count == 0)
                return;
            character.DecreaseCurrencies(setting.CreateGuildRequireCurrencies);
        }

        public virtual Reward MakeMonsterReward(MonsterCharacter monster, int level)
        {
            Reward result = new Reward();
            result.exp = monster.RandomExp(level);
            result.gold = monster.RandomGold(level);
            result.currencies = monster.RandomCurrencies();
            return result;
        }

        public virtual Reward MakeQuestReward(Quest quest)
        {
            Reward result = new Reward();
            result.exp = quest.rewardExp;
            result.gold = quest.rewardGold;
            result.currencies = quest.rewardCurrencies;
            return result;
        }

        public virtual byte GetItemMaxSocket(IPlayerCharacterData character, CharacterItem characterItem)
        {
            IEquipmentItem item = characterItem.GetEquipmentItem();
            return item == null ? (byte)0 : item.MaxSocket;
        }
    }
}
