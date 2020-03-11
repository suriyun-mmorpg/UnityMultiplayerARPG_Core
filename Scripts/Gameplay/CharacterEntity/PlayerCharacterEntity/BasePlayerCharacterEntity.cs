using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public abstract partial class BasePlayerCharacterEntity : BaseCharacterEntity, IPlayerCharacterData
    {
        public WarpPortalEntity WarpingPortal { get; set; }
        public NpcDialog CurrentNpcDialog { get; set; }

        [Header("Player Character Settings")]
        [Tooltip("The title which will be shown in create scene")]
        [SerializeField]
        protected string characterTitle;
        [Tooltip("Character titles by language keys")]
        [SerializeField]
        protected LanguageData[] characterTitles;
        
        [Tooltip("This is list which used as choice of character classes when create character")]
        [SerializeField]
        protected PlayerCharacter[] playerCharacters;
        [Tooltip("Leave this empty to use GameInstance's controller prefab")]
        [SerializeField]
        protected BasePlayerCharacterController controllerPrefab;

        public string CharacterTitle
        {
            get { return Language.GetText(characterTitles, characterTitle); }
        }

        public PlayerCharacter[] PlayerCharacters
        {
            get { return playerCharacters; }
        }

        public BasePlayerCharacterController ControllerPrefab
        {
            get { return controllerPrefab; }
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.playerTag;
        }

        protected override void EntityUpdate()
        {
            Profiler.BeginSample("BasePlayerCharacterEntity - Update");
            base.EntityUpdate();
            if (IsDead())
            {
                StopMove();
                SetTargetEntity(null);
                return;
            }
            Profiler.EndSample();
        }

        public override void Respawn()
        {
            if (!IsServer || !IsDead())
                return;
            base.Respawn();
            CurrentGameManager.RespawnCharacter(this);
        }

        public override sealed void Killed(IGameEntity lastAttacker)
        {
            float expLostPercentage = CurrentGameInstance.GameplayRule.GetExpLostPercentageWhenDeath(this);
            GuildData guildData;
            if (CurrentGameManager.TryGetGuild(GuildId, out guildData))
                expLostPercentage -= expLostPercentage * guildData.DecreaseExpLostPercentage;
            if (expLostPercentage <= 0f)
                expLostPercentage = 0f;
            int exp = Exp;
            exp -= (int)(this.GetNextLevelExp() * expLostPercentage / 100f);
            if (exp <= 0)
                exp = 0;
            Exp = exp;

            base.Killed(lastAttacker);
            CurrentNpcDialog = null;
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
                            {
                                // Amount is 0, remove it from inventory
                                if (CurrentGameInstance.IsLimitInventorySlot)
                                    nonEquipItems[i] = CharacterItem.Empty;
                                else
                                    nonEquipItems.RemoveAt(i);
                            }
                            else
                            {
                                // Update amount
                                nonEquipItems[i] = nonEquipItem;
                            }
                        }
                        tempDealingItems.RemoveAt(j);
                        break;
                    }
                }
            }
            this.FillEmptySlots();
            DealingCharacter.FillEmptySlots();
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