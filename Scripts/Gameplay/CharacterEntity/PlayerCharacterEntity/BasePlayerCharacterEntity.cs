using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    [RequireComponent(typeof(LiteNetLibTransform))]
    public abstract partial class BasePlayerCharacterEntity : BaseCharacterEntity, IPlayerCharacterData
    {
        [HideInInspector, System.NonSerialized]
        public WarpPortalEntity warpingPortal;
        [HideInInspector, System.NonSerialized]
        public NpcDialog currentNpcDialog;

        [Header("Player Character Settings")]
        [Tooltip("Leave this empty to use GameInstance's controller prefab")]
        public BasePlayerCharacterController controllerPrefab;

        public float setCoCharacterTime { get; private set; }
        private BasePlayerCharacterEntity coCharacter;
        public BasePlayerCharacterEntity CoCharacter
        {
            get
            {
                if (DealingState == DealingState.None && Time.unscaledTime - setCoCharacterTime >= gameInstance.coCharacterActionDuration)
                    coCharacter = null;
                return coCharacter;
            }
            set
            {
                coCharacter = value;
                setCoCharacterTime = Time.unscaledTime;
            }
        }

        private LiteNetLibTransform cacheNetTransform;
        public LiteNetLibTransform CacheNetTransform
        {
            get
            {
                if (cacheNetTransform == null)
                    cacheNetTransform = GetComponent<LiteNetLibTransform>();
                return cacheNetTransform;
            }
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.playerTag;
        }

#if UNITY_EDITOR
        public override void OnBehaviourValidate()
        {
            base.OnBehaviourValidate();
            if (database == null)
            {
                Debug.LogError("[BasePlayerCharacterEntity] " + name + " Database is empty");
            }
            if (database != null && !(database is PlayerCharacter))
            {
                Debug.LogError("[BasePlayerCharacterEntity] " + name + " Database must be `PlayerCharacter`");
                database = null;
                EditorUtility.SetDirty(this);
            }
        }
#endif

        protected override void ApplySkill(CharacterSkill characterSkill, Vector3 position, SkillAttackType skillAttackType, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            base.ApplySkill(characterSkill, position, skillAttackType, weapon, damageInfo, allDamageAmounts);

            Skill skill = characterSkill.GetSkill();
            switch (skill.skillType)
            {
                case SkillType.CraftItem:
                    GameMessage.Type gameMessageType;
                    if (!skill.itemCraft.CanCraft(this, out gameMessageType))
                        gameManager.SendServerGameMessage(ConnectionId, gameMessageType);
                    else
                        skill.itemCraft.CraftItem(this);
                    break;
            }
        }

        public override void Respawn()
        {
            if (!IsServer || !IsDead())
                return;
            base.Respawn();
            gameManager.RespawnCharacter(this);
        }

        public override bool CanReceiveDamageFrom(IAttackerEntity attacker)
        {
            if (attacker == null)
                return false;

            BaseCharacterEntity characterEntity = attacker as BaseCharacterEntity;
            if (characterEntity == null)
                return false;

            if (isInSafeArea || characterEntity.isInSafeArea)
            {
                // If this character or another character is in safe area so it cannot receive damage
                return false;
            }
            if (characterEntity is BasePlayerCharacterEntity)
            {
                // If not ally while this is Pvp map, assume that it can receive damage
                if (!IsAlly(characterEntity) && gameManager.CurrentMapInfo.canPvp)
                    return true;
            }
            if (characterEntity is BaseMonsterCharacterEntity)
            {
                // If this character is not summoner so it is enemy and also can receive damage
                return !IsAlly(characterEntity);
            }
            return false;
        }

        public override bool IsAlly(BaseCharacterEntity characterEntity)
        {
            if (characterEntity == null)
                return false;

            if (characterEntity is BasePlayerCharacterEntity)
            {
                // If this character is in same party or guild with another character so it is ally
                BasePlayerCharacterEntity playerCharacterEntity = characterEntity as BasePlayerCharacterEntity;
                return (PartyId > 0 && PartyId == playerCharacterEntity.PartyId) ||
                    (GuildId > 0 && GuildId == playerCharacterEntity.GuildId);
            }
            if (characterEntity is BaseMonsterCharacterEntity)
            {
                // If this character is summoner so it is ally
                BaseMonsterCharacterEntity monsterCharacterEntity = characterEntity as BaseMonsterCharacterEntity;
                return monsterCharacterEntity.Summoner != null && monsterCharacterEntity.Summoner == this;
            }
            return false;
        }

        public override bool IsEnemy(BaseCharacterEntity characterEntity)
        {
            if (characterEntity == null)
                return false;

            if (characterEntity is BasePlayerCharacterEntity)
            {
                // If not ally while this is Pvp map, assume that it is enemy while both characters are not in safe zone
                if (!IsAlly(characterEntity) && gameManager.CurrentMapInfo.canPvp)
                    return !isInSafeArea && !characterEntity.isInSafeArea;
            }
            if (characterEntity is BaseMonsterCharacterEntity)
            {
                // If this character is not summoner so it is enemy
                BaseMonsterCharacterEntity monsterCharacterEntity = characterEntity as BaseMonsterCharacterEntity;
                return monsterCharacterEntity.Summoner == null || monsterCharacterEntity.Summoner != this;
            }
            return false;
        }

        public override void Killed(BaseCharacterEntity lastAttacker)
        {
            float expLostPercentage = gameInstance.GameplayRule.GetExpLostPercentageWhenDeath(this);
            GuildData guildData;
            if (gameManager.TryGetGuild(GuildId, out guildData))
                expLostPercentage -= expLostPercentage * guildData.DecreaseExpLostPercentage;
            if (expLostPercentage <= 0f)
                expLostPercentage = 0f;
            int exp = Exp;
            exp -= (int)(this.GetNextLevelExp() * expLostPercentage / 100f);
            if (exp <= 0)
                exp = 0;
            Exp = exp;

            base.Killed(lastAttacker);
            currentNpcDialog = null;
        }

        public override void RewardExp(int exp, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;

            GuildData guildData;
            switch (rewardGivenType)
            {
                case RewardGivenType.KillMonster:
                    if (gameManager.TryGetGuild(GuildId, out guildData))
                        exp += (int)(exp * guildData.IncreaseExpGainPercentage / 100f);
                    break;
                case RewardGivenType.PartyShare:
                    if (gameManager.TryGetGuild(GuildId, out guildData))
                        exp += (int)(exp * guildData.IncreaseShareExpGainPercentage / 100f);
                    break;
            }
            base.RewardExp(exp, rewardGivenType);
        }

        public virtual void RewardGold(int gold, RewardGivenType rewardGivenType)
        {
            if (!IsServer)
                return;
            GuildData guildData;
            switch (rewardGivenType)
            {
                case RewardGivenType.KillMonster:
                    if (gameManager.TryGetGuild(GuildId, out guildData))
                        gold += (int)(gold * guildData.IncreaseGoldGainPercentage / 100f);
                    break;
                case RewardGivenType.PartyShare:
                    if (gameManager.TryGetGuild(GuildId, out guildData))
                        gold += (int)(gold * guildData.IncreaseShareGoldGainPercentage / 100f);
                    break;
            }
            Gold += gold;
        }

        public virtual void OnKillMonster(BaseMonsterCharacterEntity monsterCharacterEntity)
        {
            if (!IsServer || monsterCharacterEntity == null)
                return;

            for (int i = 0; i < Quests.Count; ++i)
            {
                CharacterQuest quest = Quests[i];
                if (quest.AddKillMonster(monsterCharacterEntity, 1))
                    quests[i] = quest;
            }
        }

        public virtual void ExchangeDealingItemsAndGold()
        {
            if (CoCharacter == null)
                return;
            List<DealingCharacterItem> tempDealingItems = new List<DealingCharacterItem>(DealingItems);
            for (int i = nonEquipItems.Count - 1; i >= 0; --i)
            {
                CharacterItem nonEquipItem = nonEquipItems[i];
                for (int j = tempDealingItems.Count - 1; j >= 0; --j)
                {
                    DealingCharacterItem dealingItem = tempDealingItems[j];
                    if (dealingItem.nonEquipIndex == i && nonEquipItem.amount >= dealingItem.amount)
                    {
                        nonEquipItem.amount -= dealingItem.amount;
                        if (nonEquipItem.amount == 0)
                            nonEquipItems.RemoveAt(i);
                        else
                            nonEquipItems[i] = nonEquipItem;
                        CoCharacter.IncreaseItems(dealingItem);
                        tempDealingItems.RemoveAt(j);
                        break;
                    }
                }
            }
            Gold -= DealingGold;
            CoCharacter.Gold += DealingGold;
        }

        public virtual void ClearDealingData()
        {
            DealingState = DealingState.None;
            DealingGold = 0;
            DealingItems.Clear();
        }

        public override bool CanMoveOrDoActions()
        {
            return base.CanMoveOrDoActions() && DealingState == DealingState.None;
        }

        public override void NotifyEnemySpotted(BaseCharacterEntity ally, BaseCharacterEntity attacker)
        {
            // TODO: May send data to client
        }

        public abstract float StoppingDistance { get; }
        public abstract void StopMove();
        public abstract void KeyMovement(Vector3 direction, bool isJump);
        public abstract void PointClickMovement(Vector3 position);
    }
}