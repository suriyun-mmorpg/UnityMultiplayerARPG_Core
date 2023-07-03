using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseGameplayRule : ScriptableObject
    {
        public float GoldRate { get; set; } = 1f;
        public float ExpRate { get; set; } = 1f;

        /// <summary>
        /// This function will be called when applying damage to a character, implement it to calculate character's stats to find if it should hit a character or not, return `TRUE` if it hit
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="attacker"></param>
        /// <param name="damageReceiver"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="weapon"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        /// <param name="randomSeed"></param>
        /// <param name="isCritical"></param>
        /// <param name="isBlocked"></param>
        /// <returns></returns>
        public abstract bool RandomAttackHitOccurs(Vector3 fromPosition, BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel, int randomSeed, out bool isCritical, out bool isBlocked);

        /// <summary>
        /// This function will be called when applying damage to a character, implement it to calculate character's attack damage to another character
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="attacker"></param>
        /// <param name="damageReceiver"></param>
        /// <param name="damageElement"></param>
        /// <param name="damageAmount"></param>
        /// <param name="weapon"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        /// <param name="randomSeed"></param>
        /// <returns></returns>
        public abstract float RandomAttackDamage(Vector3 fromPosition, BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, DamageElement damageElement, MinMaxFloat damageAmount, CharacterItem weapon, BaseSkill skill, int skillLevel, int randomSeed);

        /// <summary>
        /// This function will be called when applying damage to a character, implement it to calculate character's stats to reduce damage from another character
        /// </summary>
        /// <param name="damageReceiverResistances"></param>
        /// <param name="damageReceiverArmors"></param>
        /// <param name="damageAmount"></param>
        /// <param name="damageElement"></param>
        /// <returns></returns>
        public abstract float GetDamageReducedByResistance(Dictionary<DamageElement, float> damageReceiverResistances, Dictionary<DamageElement, float> damageReceiverArmors, float damageAmount, DamageElement damageElement);

        /// <summary>
        /// This function will be called when applying damage to a character, implement it to calculate character's attack damage to another character when critical occurs
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="damageReceiver"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        public abstract float GetCriticalDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage);

        /// <summary>
        /// This function will be called when applying damage to a character, implement it to calculate character's attack damage to another character when block occurs
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="damageReceiver"></param>
        /// <param name="damage"></param>
        /// <returns></returns>
        public abstract float GetBlockDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, float damage);

        /// <summary>
        /// This function will be called when applying damage to a character, it will be called after damage caculated (after random damage, get critical damage, get block damage) to allow developer to modify damage before apply to a character
        /// </summary>
        /// <param name="fromPosition"></param>
        /// <param name="instigator"></param>
        /// <param name="damageReceiver"></param>
        /// <param name="totalDamage"></param>
        /// <param name="weapon"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        /// <returns></returns>
        public abstract int GetTotalDamage(Vector3 fromPosition, EntityInfo instigator, DamageableEntity damageReceiver, float totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel);

        /// <summary>
        /// Calculate amount of recoverying HP per seconds, if it is `10` it will increase 10 character's current HP per second
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract float GetRecoveryHpPerSeconds(BaseCharacterEntity character);

        /// <summary>
        /// Calculate amount of recoverying MP per seconds, if it is `10` it will increase 10 character's current MP per second
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract float GetRecoveryMpPerSeconds(BaseCharacterEntity character);

        /// <summary>
        /// Calculate amount of recoverying Stamina per seconds, if it is `10` it will increase 10 character's current Stamina per second
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract float GetRecoveryStaminaPerSeconds(BaseCharacterEntity character);

        /// <summary>
        /// Calculate amount of decreasing HP per seconds, if it is `10` it will decrease 10 character's current HP per second
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract float GetDecreasingHpPerSeconds(BaseCharacterEntity character);

        /// <summary>
        /// Calculate amount of decreasing MP per seconds, if it is `10` it will decrease 10 character's current MP per second
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract float GetDecreasingMpPerSeconds(BaseCharacterEntity character);

        /// <summary>
        /// Calculate amount of decreasing Stamina per seconds, if it is `10` it will decrease 10 character's current Stamina per second
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract float GetDecreasingStaminaPerSeconds(BaseCharacterEntity character);

        /// <summary>
        /// Calculate amount of decreasing food per seconds, if it is `10` it will decrease 10 character's current food per second
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract float GetDecreasingFoodPerSeconds(BaseCharacterEntity character);

        /// <summary>
        /// Calculate amount of decreasing water per seconds, if it is `10` it will decrease 10 character's current water per second
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract float GetDecreasingWaterPerSeconds(BaseCharacterEntity character);

        /// <summary>
        /// This function will be called when player's character dying
        /// </summary>
        /// <param name="player"></param>
        /// <param name="attacker"></param>
        /// <param name="decreaseExp"></param>
        /// <param name="decreaseGold"></param>
        /// <param name="decreaseItems"></param>
        public abstract void GetPlayerDeadPunishment(BasePlayerCharacterEntity player, BaseCharacterEntity attacker, out int decreaseExp, out int decreaseGold, out int decreaseItems);

        /// <summary>
        /// Result from this function will be used to calculate character's move speed while characters carry too heavy items
        /// </summary>
        /// <param name="gameEntity"></param>
        /// <returns></returns>
        public abstract float GetOverweightMoveSpeedRate(BaseGameEntity gameEntity);

        /// <summary>
        /// Result from this function will be used to calculate character's move speed while character is sprinting
        /// </summary>
        /// <param name="gameEntity"></param>
        /// <returns></returns>
        public abstract float GetSprintMoveSpeedRate(BaseGameEntity gameEntity);

        /// <summary>
        /// Result from this function will be used to calculate character's move speed while character is walking
        /// </summary>
        /// <param name="gameEntity"></param>
        /// <returns></returns>
        public abstract float GetWalkMoveSpeedRate(BaseGameEntity gameEntity);

        /// <summary>
        /// Result from this function will be used to calculate character's move speed while character is crouching
        /// </summary>
        /// <param name="gameEntity"></param>
        /// <returns></returns>
        public abstract float GetCrouchMoveSpeedRate(BaseGameEntity gameEntity);

        /// <summary>
        /// Result from this function will be used to calculate character's move speed while character is crawling
        /// </summary>
        /// <param name="gameEntity"></param>
        /// <returns></returns>
        public abstract float GetCrawlMoveSpeedRate(BaseGameEntity gameEntity);

        /// <summary>
        /// Result from this function will be used to calculate character's move speed while character is swimming
        /// </summary>
        /// <param name="gameEntity"></param>
        /// <returns></returns>
        public abstract float GetSwimMoveSpeedRate(BaseGameEntity gameEntity);

        /// <summary>
        /// This function will be called to find character's total weight from carrying items (use it to store as caches later)
        /// </summary>
        /// <param name="character"></param>
        /// <param name="stats"></param>
        /// <returns></returns>
        public abstract float GetTotalWeight(ICharacterData character, CharacterStats stats);

        /// <summary>
        /// This function will be called to calculate character's limit weight stats (use it to store as caches later)
        /// </summary>
        /// <param name="character"></param>
        /// <param name="stats"></param>
        /// <returns></returns>
        public abstract float GetLimitWeight(ICharacterData character, CharacterStats stats);

        /// <summary>
        /// This function will be called to calculate character's total slot from carrying items (use it to store as caches later)
        /// </summary>
        /// <param name="character"></param>
        /// <param name="stats"></param>
        /// <returns></returns>
        public abstract int GetTotalSlot(ICharacterData character, CharacterStats stats);

        /// <summary>
        /// This function will be called to calculate character's limit slot stats (use it to store as caches later)
        /// </summary>
        /// <param name="character"></param>
        /// <param name="stats"></param>
        /// <returns></returns>
        public abstract int GetLimitSlot(ICharacterData character, CharacterStats stats);

        /// <summary>
        /// Use character's stats to find if character is hungry or not
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract bool IsHungry(BaseCharacterEntity character);

        /// <summary>
        /// Use character's stats to find if character is thirsty or not
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract bool IsThirsty(BaseCharacterEntity character);

        /// <summary>
        /// This will be called giving reward to a character, `giverLevel` is level of giver, `sourceLevel` is level of source, for example if `rewardGivenType` is `PartyShare`, then character's level of the one who kill a monster is `9`, then `giverLevel` will = `9`, and if monster's level is `18`, `sourceLevel` will = `18`, you may change amount of EXP by `giverLevel` or `sourceLevel`
        /// </summary>
        /// <param name="character"></param>
        /// <param name="reward"></param>
        /// <param name="multiplier"></param>
        /// <param name="rewardGivenType"></param>
        /// <param name="giverLevel"></param>
        /// <param name="sourceLevel"></param>
        /// <param name="rewardedExp"></param>
        /// <returns></returns>
        public abstract bool RewardExp(BaseCharacterEntity character, Reward reward, float multiplier, RewardGivenType rewardGivenType, int giverLevel, int sourceLevel, out int rewardedExp);

        /// <summary>
        /// This will be called giving reward to a character, `giverLevel` is level of giver, `sourceLevel` is level of source, for example if `rewardGivenType` is `PartyShare`, then character's level of the one who kill a monster is `9`, then `giverLevel` will = `9`, and if monster's level is `18`, `sourceLevel` will = `18`, you may change amount of currencies by `giverLevel` or `sourceLevel`
        /// </summary>
        /// <param name="character"></param>
        /// <param name="reward"></param>
        /// <param name="multiplier"></param>
        /// <param name="rewardGivenType"></param>
        /// <param name="giverLevel"></param>
        /// <param name="sourceLevel"></param>
        /// <param name="rewardedGold"></param>
        public abstract void RewardCurrencies(BaseCharacterEntity character, Reward reward, float multiplier, RewardGivenType rewardGivenType, int giverLevel, int sourceLevel, out int rewardedGold);

        /// <summary>
        /// This will be called when calculate equipment's total stats, may use this function to change equipment's stats rate by its durability
        /// </summary>
        /// <param name="characterItem"></param>
        /// <returns></returns>
        public abstract float GetEquipmentStatsRate(CharacterItem characterItem);

        /// <summary>
        /// Do something to character when respawning, usually use it to receover character's HP/MP, you may implement this function revovery full HP, or just 10% of max HP as you wish (by coding)
        /// </summary>
        /// <param name="character"></param>
        public abstract void OnCharacterRespawn(ICharacterData character);

        /// <summary>
        /// This function will be called when character entity received damage (HP decreased)
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="damageReceiver"></param>
        /// <param name="combatAmountType"></param>
        /// <param name="damage"></param>
        /// <param name="weapon"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        /// <param name="buff"></param>
        /// <param name="isDamageOverTime"></param>
        public abstract void OnCharacterReceivedDamage(BaseCharacterEntity attacker, BaseCharacterEntity damageReceiver, CombatAmountType combatAmountType, int damage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime);

        /// <summary>
        /// This function will be called when harvestable entity received damage (HP decreased)
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="damageReceiver"></param>
        /// <param name="combatAmountType"></param>
        /// <param name="damage"></param>
        /// <param name="weapon"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        /// <param name="buff"></param>
        /// <param name="isDamageOverTime"></param>
        public abstract void OnHarvestableReceivedDamage(BaseCharacterEntity attacker, HarvestableEntity damageReceiver, CombatAmountType combatAmountType, int damage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime);

        /// <summary>
        /// Get duration to apply recoverying, for example, if HP recovery per seconds is `10`, and this duration is `5`, it will recover `50` HP every `5` seconds, but if this duration is `0.5`, it will recover `5` HP every `0.5` seconds.
        /// </summary>
        /// <returns></returns>
        public abstract float GetRecoveryUpdateDuration();

        /// <summary>
        /// Apply fall damage to a character, you can use `lastGroundedPosition` to find fall height
        /// </summary>
        /// <param name="character"></param>
        /// <param name="lastGroundedPosition"></param>
        public abstract void ApplyFallDamage(BaseCharacterEntity character, Vector3 lastGroundedPosition);

        /// <summary>
        /// Use it to confirm that the character can interact networked object which its ID is `objectId`
        /// </summary>
        /// <param name="character"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        public abstract bool CanInteractEntity(BaseCharacterEntity character, uint objectId);

        /// <summary>
        /// Use it to find position around summoner, where summoned characters will be spawned and move follow the summoner
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract Vector3 GetSummonPosition(BaseCharacterEntity character);

        /// <summary>
        /// Use it to find rotation around summoner, where summoned characters will be spawned and move follow the summoner
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract Quaternion GetSummonRotation(BaseCharacterEntity character);

        /// <summary>
        /// Calculate battle point from character stats
        /// </summary>
        /// <param name="stats"></param>
        /// <returns></returns>
        public abstract float GetBattlePointFromCharacterStats(CharacterStats stats);

        /// <summary>
        /// Calculate PK point which `attacker` will receive when kill the `damageReceiver`
        /// </summary>
        /// <returns></returns>
        public abstract int GetPkPointWhenCharacterKilled(BasePlayerCharacterEntity attacker, BasePlayerCharacterEntity damageReceiver);

        /// <summary>
        /// Return `TRUE` if the player can turn PK mode on
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool CanTurnPkOn(BasePlayerCharacterEntity player);

        /// <summary>
        /// Return `TRUE` if the player can turn PK mode on
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public abstract bool CanTurnPkOff(BasePlayerCharacterEntity player);

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
