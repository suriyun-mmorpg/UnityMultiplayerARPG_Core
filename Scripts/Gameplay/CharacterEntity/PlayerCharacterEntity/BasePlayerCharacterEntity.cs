using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(PlayerCharacterCraftingComponent))]
    public abstract partial class BasePlayerCharacterEntity : BaseCharacterEntity, IPlayerCharacterData
    {
        public BaseNpcDialog CurrentNpcDialog { get; set; }

        [Category("Character Settings")]
        [Tooltip("This is list which used as choice of character classes when create character")]
        [SerializeField]
        [FormerlySerializedAs("playerCharacters")]
        protected PlayerCharacter[] characterDatabases;
        [Tooltip("Leave this empty to use GameInstance's controller prefab")]
        [SerializeField]
        protected BasePlayerCharacterController controllerPrefab;

        public PlayerCharacter[] CharacterDatabases
        {
            get { return characterDatabases; }
            set { characterDatabases = value; }
        }

        public BasePlayerCharacterController ControllerPrefab
        {
            get { return controllerPrefab; }
        }

        public PlayerCharacterCraftingComponent Crafting { get; private set; }

        public int IndexOfCharacterDatabase(int dataId)
        {
            for (int i = 0; i < CharacterDatabases.Length; ++i)
            {
                if (CharacterDatabases[i] != null && CharacterDatabases[i].DataId == dataId)
                    return i;
            }
            return -1;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddCharacters(CharacterDatabases);
        }

        public override EntityInfo GetInfo()
        {
            return new EntityInfo(
                EntityTypes.Player,
                ObjectId,
                Id,
                DataId,
                FactionId,
                PartyId,
                GuildId,
                IsInSafeArea);
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.playerTag;
        }

        public override void InitialRequiredComponents()
        {
            base.InitialRequiredComponents();
            Crafting = gameObject.GetOrAddComponent<PlayerCharacterCraftingComponent>();
            gameObject.GetOrAddComponent<PlayerCharacterItemLockAndExpireComponent>();
        }

        protected override void EntityUpdate()
        {
            Profiler.BeginSample("BasePlayerCharacterEntity - Update");
            base.EntityUpdate();
            if (this.IsDead())
            {
                StopMove();
                SetTargetEntity(null);
                return;
            }
            Profiler.EndSample();
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
                            GameInstance.ServerGameMessageHandlers.NotifyRewardItem(DealingCharacter.ConnectionId, dealingItem.characterItem.dataId, dealingItem.characterItem.amount);
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
            DealingCharacter.Gold = DealingCharacter.Gold.Increase(gold);
            GameInstance.ServerGameMessageHandlers.NotifyRewardGold(DealingCharacter.ConnectionId, DealingGold);
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