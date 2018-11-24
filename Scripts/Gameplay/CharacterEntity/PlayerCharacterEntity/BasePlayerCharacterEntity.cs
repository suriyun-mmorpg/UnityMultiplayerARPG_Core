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
        [HideInInspector]
        public WarpPortalEntity warpingPortal;
        [HideInInspector]
        public NpcDialog currentNpcDialog;

        [Header("Player Character Settings")]
        public BasePlayerCharacterController controllerPrefab;

        public float setCoCharacterTime { get; private set; }
        private BasePlayerCharacterEntity coCharacter;
        public BasePlayerCharacterEntity CoCharacter
        {
            get
            {
                if (DealingState == DealingState.None && Time.unscaledTime - setCoCharacterTime >= GameInstance.coCharacterActionDuration)
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
            gameObject.tag = GameInstance.playerTag;
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
                EditorUtility.SetDirty(gameObject);
            }
        }
#endif

        protected override void ApplySkill(CharacterSkill characterSkill, Vector3 position, SkillAttackType skillAttackType, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            base.ApplySkill(characterSkill, position, skillAttackType, weapon, damageInfo, allDamageAmounts);

            var skill = characterSkill.GetSkill();
            switch (skill.skillType)
            {
                case SkillType.CraftItem:
                    GameMessage.Type gameMessageType;
                    if (!skill.itemCraft.CanCraft(this, out gameMessageType))
                        GameManager.SendServerGameMessage(ConnectionId, gameMessageType);
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
            GameManager.WarpCharacter(this, RespawnMapName, RespawnPosition);
        }

        public override bool CanReceiveDamageFrom(BaseCharacterEntity characterEntity)
        {
            if (characterEntity == null)
                return false;
            if (isInSafeArea || characterEntity.isInSafeArea)
                return false;
            if (characterEntity is MonsterCharacterEntity)
                return true;
            // If not ally while this is Pvp map, assume that it can receive damage
            if (!IsAlly(characterEntity) && GameManager.CurrentMapInfo.canPvp)
                return true;
            return false;
        }

        public override bool IsAlly(BaseCharacterEntity characterEntity)
        {
            if (characterEntity is BasePlayerCharacterEntity)
            {
                var playerCharacterEntity = characterEntity as BasePlayerCharacterEntity;
                return playerCharacterEntity.PartyId == PartyId || playerCharacterEntity.GuildId == GuildId;
            }
            return false;
        }

        public override bool IsEnemy(BaseCharacterEntity characterEntity)
        {
            if (characterEntity == null)
                return false;
            if (characterEntity is BaseMonsterCharacterEntity)
                return true;
            // If character can receive damage from another character, assume that another character is enemy
            // So if this character and another character is in safe area or not ally while this map is pvp map then they are enemy
            return CanReceiveDamageFrom(characterEntity);
        }

        public override void Killed(BaseCharacterEntity lastAttacker)
        {
            var expLostPercentage = GameInstance.GameplayRule.GetExpLostPercentageWhenDeath(this);
            GuildData guildData;
            if (GameManager.TryGetGuild(GuildId, out guildData))
                expLostPercentage -= expLostPercentage * guildData.DecreaseExpLostPercentage;
            if (expLostPercentage <= 0f)
                expLostPercentage = 0f;
            var exp = Exp;
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
                    if (GameManager.TryGetGuild(GuildId, out guildData))
                        exp += (int)(exp * guildData.IncreaseExpGainPercentage / 100f);
                    break;
                case RewardGivenType.PartyShare:
                    if (GameManager.TryGetGuild(GuildId, out guildData))
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
                    if (GameManager.TryGetGuild(GuildId, out guildData))
                        gold += (int)(gold * guildData.IncreaseGoldGainPercentage / 100f);
                    break;
                case RewardGivenType.PartyShare:
                    if (GameManager.TryGetGuild(GuildId, out guildData))
                        gold += (int)(gold * guildData.IncreaseShareGoldGainPercentage / 100f);
                    break;
            }
            Gold += gold;
        }

        public virtual void OnKillMonster(BaseMonsterCharacterEntity monsterCharacterEntity)
        {
            if (!IsServer || monsterCharacterEntity == null)
                return;

            for (var i = 0; i < Quests.Count; ++i)
            {
                var quest = Quests[i];
                if (quest.AddKillMonster(monsterCharacterEntity, 1))
                    quests[i] = quest;
            }
        }

        public virtual void ExchangeDealingItemsAndGold()
        {
            if (CoCharacter == null)
                return;
            var tempDealingItems = new List<DealingCharacterItem>(DealingItems);
            for (var i = nonEquipItems.Count - 1; i >= 0; --i)
            {
                var nonEquipItem = nonEquipItems[i];
                for (var j = tempDealingItems.Count - 1; j >= 0; --j)
                {
                    var dealingItem = tempDealingItems[j];
                    if (dealingItem.nonEquipIndex == i && nonEquipItem.amount >= dealingItem.amount)
                    {
                        nonEquipItem.amount -= dealingItem.amount;
                        if (nonEquipItem.amount == 0)
                            nonEquipItems.RemoveAt(i);
                        else
                            nonEquipItems[i] = nonEquipItem;
                        CoCharacter.IncreaseItems(dealingItem.dataId, dealingItem.level, dealingItem.amount, dealingItem.durability);
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