using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BaseCharacterEntity
    {
        public void ValidateRecovery(EntityInfo instigator)
        {
            if (!IsServer)
                return;

            // Validate Hp
            if (CurrentHp < 0)
                CurrentHp = 0;
            if (CurrentHp > CachedData.MaxHp)
                CurrentHp = CachedData.MaxHp;
            // Validate Mp
            if (CurrentMp < 0)
                CurrentMp = 0;
            if (CurrentMp > CachedData.MaxMp)
                CurrentMp = CachedData.MaxMp;
            // Validate Stamina
            if (CurrentStamina < 0)
                CurrentStamina = 0;
            if (CurrentStamina > CachedData.MaxStamina)
                CurrentStamina = CachedData.MaxStamina;
            // Validate Food
            if (CurrentFood < 0)
                CurrentFood = 0;
            if (CurrentFood > CachedData.MaxFood)
                CurrentFood = CachedData.MaxFood;
            // Validate Water
            if (CurrentWater < 0)
                CurrentWater = 0;
            if (CurrentWater > CachedData.MaxWater)
                CurrentWater = CachedData.MaxWater;

            if (this.IsDead())
                Killed(instigator);
        }

        public virtual void Killed(EntityInfo lastAttacker)
        {
            StopAllCoroutines();
            for (int i = buffs.Count - 1; i >= 0; --i)
            {
                if (!buffs[i].GetBuff().GetBuff().doNotRemoveOnDead)
                    buffs.RemoveAt(i);
            }
            for (int i = summons.Count - 1; i >= 0; --i)
            {
                summons[i].UnSummon(this);
                summons.RemoveAt(i);
            }
            if (CurrentGameInstance.clearSkillCooldownOnDead)
            {
                skillUsages.Clear();
            }
            CallAllOnDead();
        }

        public virtual void OnRespawn()
        {
            if (!IsServer)
                return;
            _lastGrounded = true;
            _lastGroundedPosition = EntityTransform.position;
            RespawnGroundedCheckCountDown = RESPAWN_GROUNDED_CHECK_DURATION;
            RespawnInvincibleCountDown = RESPAWN_INVINCIBLE_DURATION;
            CallAllOnRespawn();
        }

        public void RewardExp(Reward reward, float multiplier, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;
            int rewardedExp;
            if (!CurrentGameplayRule.RewardExp(this, reward, multiplier, rewardGivenType, out rewardedExp))
            {
                GameInstance.ServerGameMessageHandlers.NotifyRewardExp(ConnectionId, rewardGivenType, rewardedExp);
                return;
            }
            GameInstance.ServerGameMessageHandlers.NotifyRewardExp(ConnectionId, rewardGivenType, rewardedExp);
            CallAllOnLevelUp();
        }

        public void RewardCurrencies(Reward reward, float multiplier, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;
            int rewardedGold;
            CurrentGameplayRule.RewardCurrencies(this, reward, multiplier, rewardGivenType, out rewardedGold);
            GameInstance.ServerGameMessageHandlers.NotifyRewardGold(ConnectionId, rewardGivenType, rewardedGold);
            if (reward.currencies != null && reward.currencies.Length > 0)
            {
                foreach (CurrencyAmount currency in reward.currencies)
                {
                    if (currency.currency == null || currency.amount == 0)
                        continue;
                    GameInstance.ServerGameMessageHandlers.NotifyRewardCurrency(ConnectionId, rewardGivenType, currency.currency.DataId, currency.amount);
                }
            }
        }

        protected override void ApplyReceiveDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel, int randomSeed, out CombatAmountType combatAmountType, out int totalDamage)
        {
            if (instigator.TryGetEntity(out BaseCharacterEntity attackerCharacter))
            {
                // Notify enemy spotted when received damage from enemy
                NotifyEnemySpotted(attackerCharacter);

                // Notify enemy spotted when damage taken to enemy
                attackerCharacter.NotifyEnemySpotted(this);
            }

            bool isCritical;
            bool isBlocked;
            if (!CurrentGameInstance.GameplayRule.RandomAttackHitOccurs(fromPosition, attackerCharacter, this, damageAmounts, weapon, skill, skillLevel, randomSeed, out isCritical, out isBlocked))
            {
                // Don't hit (Miss)
                combatAmountType = CombatAmountType.Miss;
                totalDamage = 0;
                return;
            }

            // Calculate damages
            combatAmountType = CombatAmountType.NormalDamage;
            float calculatingTotalDamage = 0f;
            foreach (DamageElement damageElement in damageAmounts.Keys)
            {
                calculatingTotalDamage += damageElement.GetDamageReducedByResistance(CachedData.Resistances, CachedData.Armors,
                    CurrentGameInstance.GameplayRule.RandomAttackDamage(fromPosition, attackerCharacter, this, damageElement, damageAmounts[damageElement], weapon, skill, skillLevel, randomSeed));
            }

            if (attackerCharacter != null)
            {
                // If critical occurs
                if (isCritical)
                {
                    calculatingTotalDamage = CurrentGameInstance.GameplayRule.GetCriticalDamage(attackerCharacter, this, calculatingTotalDamage);
                    combatAmountType = CombatAmountType.CriticalDamage;
                }
                // If block occurs
                if (isBlocked)
                {
                    calculatingTotalDamage = CurrentGameInstance.GameplayRule.GetBlockDamage(attackerCharacter, this, calculatingTotalDamage);
                    combatAmountType = CombatAmountType.BlockedDamage;
                }
            }

            // Apply damages
            totalDamage = CurrentGameInstance.GameplayRule.GetTotalDamage(fromPosition, instigator, this, calculatingTotalDamage, weapon, skill, skillLevel);
            if (totalDamage < 0)
                totalDamage = 0;
            CurrentHp -= totalDamage;
        }

        public override void ReceivedDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, int skillLevel, CharacterBuff buff, bool isDamageOverTime = false)
        {
            base.ReceivedDamage(position, fromPosition, instigator, damageAmounts, combatAmountType, totalDamage, weapon, skill, skillLevel, buff, isDamageOverTime);
            instigator.TryGetEntity(out BaseCharacterEntity attackerCharacter);
            CurrentGameInstance.GameplayRule.OnCharacterReceivedDamage(attackerCharacter, this, combatAmountType, totalDamage, weapon, skill, skillLevel, buff, isDamageOverTime);

            if (combatAmountType == CombatAmountType.Miss)
                return;

            // Interrupt casting skill when receive damage
            UseSkillComponent.InterruptCastingSkill();

            // Do something when character dead
            if (this.IsDead())
            {
                AttackComponent.CancelAttack();
                UseSkillComponent.CancelSkill();
                ReloadComponent.CancelReload();

                // Call killed function, this should be called only once when dead
                ValidateRecovery(instigator);
            }
            else
            {
                // Do something with buffs when attacked
                SkillAndBuffComponent.OnAttacked();
                // Apply debuff if character is not dead
                if (buff == null && skill != null && skill.IsDebuff)
                    ApplyBuff(skill.DataId, BuffType.SkillDebuff, skillLevel, instigator, weapon);
            }
        }
    }
}
