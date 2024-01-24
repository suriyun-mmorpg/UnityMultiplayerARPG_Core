using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class DefaultServerLogHandlers : MonoBehaviour, IServerLogHandlers
    {
        public void LogAttackEnd(IPlayerCharacterData playerCharacter, int simulateSeed)
        {
            // NOTE: Intended to do nothing
        }

        public void LogAttackInterrupt(IPlayerCharacterData playerCharacter, int simulateSeed)
        {
            // NOTE: Intended to do nothing
        }

        public void LogAttackStart(IPlayerCharacterData playerCharacter, int simulateSeed, float[] triggerDurations, byte fireSpread, bool isLeftHand, CharacterItem weapon)
        {
            // NOTE: Intended to do nothing
        }

        public void LogAttackTrigger(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex)
        {
            // NOTE: Intended to do nothing
        }

        public void LogAttackTriggerFail(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex, ActionTriggerFailReasons reason)
        {
            // NOTE: Intended to do nothing
        }

        public void LogBuffApply(IPlayerCharacterData playerCharacter, CharacterBuff characterBuff)
        {
            // NOTE: Intended to do nothing
        }

        public void LogBuffRemove(IPlayerCharacterData playerCharacter, CharacterBuff characterBuff, BuffRemoveReasons reason)
        {
            // NOTE: Intended to do nothing
        }

        public void LogBuyNpcItem(IPlayerCharacterData character, NpcSellItem npcSellItem, int amount)
        {
            // NOTE: Intended to do nothing
        }

        public void LogBuyNpcItem(IPlayerCharacterData character, BaseItem item, int requireGold, IList<CurrencyAmount> requiredCurrencies)
        {
            // NOTE: Intended to do nothing
        }

        public void LogBuyVendingItem(IPlayerCharacterData playerCharacter, IPlayerCharacterData sellerCharacter, CharacterItem buyItem, int price)
        {
            // NOTE: Intended to do nothing
        }

        public void LogChargeEnd(IPlayerCharacterData playerCharacter, bool willDoActionWhenStopCharging)
        {
            // NOTE: Intended to do nothing
        }

        public void LogChargeStart(IPlayerCharacterData playerCharacter)
        {
            // NOTE: Intended to do nothing
        }

        public void LogCraftItem(IPlayerCharacterData playerCharacter, ItemCraft itemCraft)
        {
            // NOTE: Intended to do nothing
        }

        public void LogCraftItem(IPlayerCharacterData playerCharacter, BaseItem craftedItem, int amount, int requiredGold, IList<ItemAmount> requiredItems, IList<CurrencyAmount> requiredCurrencies)
        {
            // NOTE: Intended to do nothing
        }

        public void LogDamageReceived(IPlayerCharacterData playerCharacter, HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime)
        {
            // NOTE: Intended to do nothing
        }

        public void LogDismentleItems(IPlayerCharacterData playerCharacter, IList<ItemAmount> dismentleItems)
        {
            // NOTE: Intended to do nothing
        }

        public void LogDismentleItems(IPlayerCharacterData playerCharacter, IList<ItemAmount> dismentleItems, int returnGold, IList<ItemAmount> returnItems, IList<CurrencyAmount> returnCurrencies)
        {
            // NOTE: Intended to do nothing
        }

        public void LogEnhanceSocketItem(IPlayerCharacterData playerCharacter, CharacterItem enhancedItem, BaseItem enhancerItem)
        {
            // NOTE: Intended to do nothing
        }

        public void LogEnterChat(ChatMessage chatMessage)
        {
            // NOTE: Intended to do nothing
        }

        public void LogEnterGame(IPlayerCharacterData playerCharacter)
        {
            // NOTE: Intended to do nothing
        }

        public void LogExchangeDealingItemsAndGold(IPlayerCharacterData playerCharacter, IPlayerCharacterData dealingCharacter, int dealingGold, IList<CharacterItem> dealingItems)
        {
            // NOTE: Intended to do nothing
        }

        public void LogExitGame(string characterId, string userId)
        {
            // NOTE: Intended to do nothing
        }

        public void LogKilled(IPlayerCharacterData playerCharacter, EntityInfo lastAttacker)
        {
            // NOTE: Intended to do nothing
        }

        public void LogQuestAbandon(IPlayerCharacterData character, Quest quest)
        {
            // NOTE: Intended to do nothing
        }

        public void LogQuestAccept(IPlayerCharacterData character, Quest quest)
        {
            // NOTE: Intended to do nothing
        }

        public void LogQuestComplete(IPlayerCharacterData character, Quest quest, byte selectedRewardIndex)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRefine(IPlayerCharacterData playerCharacter, CharacterItem refinedItem, IList<BaseItem> enhancerItems, float increaseSuccessRate, float decreaseRequireGoldRate, float chanceToNotDecreaseLevels, float chanceToNotDestroyItem, bool isSuccess, bool isDestroy, ItemRefineLevel itemRefineLevel, bool isReturning, ItemRefineFailReturning itemRefineFailReturning)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRefine(IPlayerCharacterData playerCharacter, CharacterItem refinedItem, IList<BaseItem> enhancerItems, float increaseSuccessRate, float decreaseRequireGoldRate, float chanceToNotDecreaseLevels, float chanceToNotDestroyItem, bool isSuccess, bool isDestroy, int requiredGold, IList<ItemAmount> requiredItems, IList<CurrencyAmount> requiredCurrencies, bool isReturning, int returnGold, IList<ItemAmount> returnItems, IList<CurrencyAmount> returnCurrencies)
        {
            // NOTE: Intended to do nothing
        }

        public void LogReloadEnd(IPlayerCharacterData playerCharacter)
        {
            // NOTE: Intended to do nothing
        }

        public void LogReloadInterrupt(IPlayerCharacterData playerCharacter)
        {
            // NOTE: Intended to do nothing
        }

        public void LogReloadStart(IPlayerCharacterData playerCharacter, float[] triggerDurations)
        {
            // NOTE: Intended to do nothing
        }

        public void LogReloadTrigger(IPlayerCharacterData playerCharacter, byte triggerIndex)
        {
            // NOTE: Intended to do nothing
        }

        public void LogReloadTriggerFail(IPlayerCharacterData playerCharacter, byte triggerIndex, ActionTriggerFailReasons reason)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRemoveEnhancerFromItem(IPlayerCharacterData playerCharacter, CharacterItem enhancedItem, BaseItem returnEnhancerItem)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRemoveEnhancerFromItem(IPlayerCharacterData playerCharacter, CharacterItem enhancedItem, int requiredGold, IList<ItemAmount> requiredItems, IList<CurrencyAmount> requiredCurrencies, BaseItem returnEnhancerItem)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRepair(IPlayerCharacterData playerCharacter, CharacterItem repairedItem, ItemRepairPrice itemRepairPrice)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRepair(IPlayerCharacterData playerCharacter, CharacterItem repairedItem, int requireGold, IList<ItemAmount> requiredItems, IList<CurrencyAmount> requiredCurrencies)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRewardCurrency(IPlayerCharacterData character, RewardGivenType givenType, Currency currency, int amount)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRewardCurrency(IPlayerCharacterData character, RewardGivenType givenType, CharacterCurrency currency)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRewardExp(IPlayerCharacterData character, RewardGivenType givenType, int exp, bool isLevelUp)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRewardGold(IPlayerCharacterData character, RewardGivenType givenType, int gold)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRewardItem(IPlayerCharacterData character, RewardGivenType givenType, BaseItem item, int amount)
        {
            // NOTE: Intended to do nothing
        }

        public void LogRewardItem(IPlayerCharacterData character, RewardGivenType givenType, CharacterItem item)
        {
            // NOTE: Intended to do nothing
        }

        public void LogSellNpcItem(IPlayerCharacterData character, NpcSellItem npcSellItem, int amount)
        {
            // NOTE: Intended to do nothing
        }

        public void LogSellNpcItem(IPlayerCharacterData character, CharacterItem characterItem, int amount)
        {
            // NOTE: Intended to do nothing
        }

        public void LogSellVendingItem(IPlayerCharacterData playerCharacter, IPlayerCharacterData buyerCharacter, CharacterItem buyItem, int price)
        {
            // NOTE: Intended to do nothing
        }

        public void LogUseSkillEnd(IPlayerCharacterData playerCharacter, int simulateSeed)
        {
            // NOTE: Intended to do nothing
        }

        public void LogUseSkillInterrupt(IPlayerCharacterData playerCharacter, int simulateSeed)
        {
            // NOTE: Intended to do nothing
        }

        public void LogUseSkillStart(IPlayerCharacterData playerCharacter, int simulateSeed, float[] triggerDurations, byte fireSpread, bool isLeftHand, CharacterItem weapon, BaseSkill skill, int skillLevel)
        {
            // NOTE: Intended to do nothing
        }

        public void LogUseSkillTrigger(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex)
        {
            // NOTE: Intended to do nothing
        }

        public void LogUseSkillTriggerFail(IPlayerCharacterData playerCharacter, int simulateSeed, byte triggerIndex, ActionTriggerFailReasons reason)
        {
            // NOTE: Intended to do nothing
        }
    }
}
