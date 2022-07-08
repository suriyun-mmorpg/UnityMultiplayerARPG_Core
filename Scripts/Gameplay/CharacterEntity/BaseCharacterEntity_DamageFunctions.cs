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
            if (CurrentHp > this.GetCaches().MaxHp)
                CurrentHp = this.GetCaches().MaxHp;
            // Validate Mp
            if (CurrentMp < 0)
                CurrentMp = 0;
            if (CurrentMp > this.GetCaches().MaxMp)
                CurrentMp = this.GetCaches().MaxMp;
            // Validate Stamina
            if (CurrentStamina < 0)
                CurrentStamina = 0;
            if (CurrentStamina > this.GetCaches().MaxStamina)
                CurrentStamina = this.GetCaches().MaxStamina;
            // Validate Food
            if (CurrentFood < 0)
                CurrentFood = 0;
            if (CurrentFood > this.GetCaches().MaxFood)
                CurrentFood = this.GetCaches().MaxFood;
            // Validate Water
            if (CurrentWater < 0)
                CurrentWater = 0;
            if (CurrentWater > this.GetCaches().MaxWater)
                CurrentWater = this.GetCaches().MaxWater;

            if (this.IsDead())
                Killed(instigator);
        }

        public virtual void Killed(EntityInfo lastAttacker)
        {
            StopAllCoroutines();
            for (int i = buffs.Count - 1; i >= 0; --i)
            {
                if (!buffs[i].GetBuff().doNotRemoveOnDead)
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
            lastGrounded = true;
            lastGroundedPosition = CacheTransform.position;
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
                GameInstance.ServerGameMessageHandlers.NotifyRewardExp(ConnectionId, rewardedExp);
                return;
            }
            GameInstance.ServerGameMessageHandlers.NotifyRewardExp(ConnectionId, rewardedExp);
            CallAllOnLevelUp();
        }

        public void RewardCurrencies(Reward reward, float multiplier, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;
            int rewardedGold;
            CurrentGameplayRule.RewardCurrencies(this, reward, multiplier, rewardGivenType, out rewardedGold);
            GameInstance.ServerGameMessageHandlers.NotifyRewardGold(ConnectionId, rewardedGold);
            if (reward.currencies != null && reward.currencies.Length > 0)
            {
                foreach (CurrencyAmount currency in reward.currencies)
                {
                    if (currency.currency == null || currency.amount == 0)
                        continue;
                    GameInstance.ServerGameMessageHandlers.NotifyRewardCurrency(ConnectionId, currency.currency.DataId, currency.amount);
                }
            }
        }

        protected override void ApplyReceiveDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, short skillLevel, int randomSeed, out CombatAmountType combatAmountType, out int totalDamage)
        {
            BaseCharacterEntity attackerCharacter;
            if (instigator.TryGetEntity(out attackerCharacter))
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
                calculatingTotalDamage += damageElement.GetDamageReducedByResistance(this.GetCaches().Resistances, this.GetCaches().Armors,
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

        public override void ReceivedDamage(HitBoxPosition position, Vector3 fromPosition, EntityInfo instigator, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CombatAmountType combatAmountType, int totalDamage, CharacterItem weapon, BaseSkill skill, short skillLevel)
        {
            base.ReceivedDamage(position, fromPosition, instigator, damageAmounts, combatAmountType, totalDamage, weapon, skill, skillLevel);
            BaseCharacterEntity attackerCharacter;
            instigator.TryGetEntity(out attackerCharacter);
            CurrentGameInstance.GameplayRule.OnCharacterReceivedDamage(attackerCharacter, this, combatAmountType, totalDamage, weapon, skill, skillLevel);

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
                // Apply debuff if character is not dead
                if (skill != null && skill.IsDebuff)
                    ApplyBuff(skill.DataId, BuffType.SkillDebuff, skillLevel, instigator, weapon);
            }
        }
    }
}
