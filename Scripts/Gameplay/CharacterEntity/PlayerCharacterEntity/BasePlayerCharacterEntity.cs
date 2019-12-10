using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public abstract partial class BasePlayerCharacterEntity : BaseCharacterEntity, IPlayerCharacterData
    {
        [HideInInspector, System.NonSerialized]
        public WarpPortalEntity warpingPortal;
        [HideInInspector, System.NonSerialized]
        public NpcDialog currentNpcDialog;

        [Header("Player Character Settings")]
        [Tooltip("This title will be shown in create scene")]
        public string characterTitle;
        [Tooltip("This is list which used as choice of character classes when create character")]
        public PlayerCharacter[] playerCharacters;
        [Tooltip("Leave this empty to use GameInstance's controller prefab")]
        public BasePlayerCharacterController controllerPrefab;

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.playerTag;
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            if (IsDead())
            {
                StopMove();
                SetTargetEntity(null);
                return;
            }
        }

        public override void Respawn()
        {
            if (!IsServer || !IsDead())
                return;
            base.Respawn();
            gameManager.RespawnCharacter(this);
        }

        public override bool IsAlly(BaseCharacterEntity characterEntity)
        {
            if (characterEntity == null)
                return false;

            if (characterEntity is BasePlayerCharacterEntity)
            {
                BasePlayerCharacterEntity playerCharacterEntity = characterEntity as BasePlayerCharacterEntity;
                switch (BaseGameNetworkManager.CurrentMapInfo.pvpMode)
                {
                    case PvpMode.Pvp:
                        return playerCharacterEntity.PartyId == PartyId;
                    case PvpMode.FactionPvp:
                        return playerCharacterEntity.FactionId != 0 && playerCharacterEntity.FactionId == FactionId;
                    case PvpMode.GuildPvp:
                        return playerCharacterEntity.GuildId == GuildId;
                    default:
                        return false;
                }
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
                BasePlayerCharacterEntity playerCharacterEntity = characterEntity as BasePlayerCharacterEntity;
                switch (BaseGameNetworkManager.CurrentMapInfo.pvpMode)
                {
                    case PvpMode.Pvp:
                        return playerCharacterEntity.PartyId != PartyId;
                    case PvpMode.FactionPvp:
                        return playerCharacterEntity.FactionId != 0 && playerCharacterEntity.FactionId != FactionId;
                    case PvpMode.GuildPvp:
                        return playerCharacterEntity.GuildId != GuildId;
                    default:
                        return false;
                }
            }
            if (characterEntity is BaseMonsterCharacterEntity)
            {
                // If this character is not summoner so it is enemy
                BaseMonsterCharacterEntity monsterCharacterEntity = characterEntity as BaseMonsterCharacterEntity;
                return monsterCharacterEntity.Summoner == null || monsterCharacterEntity.Summoner != this;
            }
            return false;
        }

        public override sealed void Killed(IGameEntity lastAttacker)
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

        public void OnKillMonster(BaseMonsterCharacterEntity monsterCharacterEntity)
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

        public bool ExchangingDealingItemsWillOverwhelming()
        {
            if (DealingCharacter == null)
                return true;
            List<ItemAmount> itemAmounts = new List<ItemAmount>();
            for (int i = 0; i < DealingItems.Count; ++i)
            {
                if (DealingItems[i].characterItem.IsEmptySlot()) continue;
                itemAmounts.Add(new ItemAmount()
                {
                    item = DealingItems[i].characterItem.GetItem(),
                    amount = DealingItems[i].characterItem.amount,
                });
            }
            return DealingCharacter.IncreasingItemsWillOverwhelming(itemAmounts);
        }

        public void ExchangeDealingItemsAndGold()
        {
            if (DealingCharacter == null)
                return;
            List<DealingCharacterItem> tempDealingItems = new List<DealingCharacterItem>(DealingItems);
            CharacterItem nonEquipItem;
            DealingCharacterItem dealingItem;
            int i, j;
            for (i = nonEquipItems.Count - 1; i >= 0; --i)
            {
                nonEquipItem = nonEquipItems[i];
                for (j = tempDealingItems.Count - 1; j >= 0; --j)
                {
                    dealingItem = tempDealingItems[j];
                    if (dealingItem.nonEquipIndex == i && nonEquipItem.amount >= dealingItem.characterItem.amount)
                    {
                        if (DealingCharacter.IncreaseItems(dealingItem.characterItem))
                        {
                            // Reduce item amount when able to increase item to co character
                            nonEquipItem.amount -= dealingItem.characterItem.amount;
                            if (nonEquipItem.amount == 0)
                                nonEquipItems.RemoveAt(i);
                            else
                                nonEquipItems[i] = nonEquipItem;
                        }
                        tempDealingItems.RemoveAt(j);
                        break;
                    }
                }
            }
            this.FillEmptySlots();
            Gold -= DealingGold;
            DealingCharacter.Gold += DealingGold;
        }

        public void ClearDealingData()
        {
            DealingState = DealingState.None;
            DealingGold = 0;
            DealingItems.Clear();
        }

        public override bool CanDoActions()
        {
            return base.CanDoActions() && DealingState == DealingState.None;
        }

        public override void NotifyEnemySpotted(BaseCharacterEntity ally, BaseCharacterEntity attacker)
        {
            // TODO: May send data to client
        }
    }
}