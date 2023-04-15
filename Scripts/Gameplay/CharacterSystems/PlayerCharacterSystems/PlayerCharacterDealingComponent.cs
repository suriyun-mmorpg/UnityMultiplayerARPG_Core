using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public partial class PlayerCharacterDealingComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>
    {
        protected DealingState _dealingState = DealingState.None;
        public DealingState DealingState
        {
            get { return _dealingState; }
            set
            {
                _dealingState = value;
                CallOwnerUpdateDealingState(value);
                if (DealingCharacter != null)
                    DealingCharacter.Dealing.CallOwnerUpdateAnotherDealingState(value);
            }
        }

        protected int _dealingGold = 0;
        public int DealingGold
        {
            get { return _dealingGold; }
            set
            {
                _dealingGold = value;
                CallOwnerUpdateDealingGold(value);
                if (DealingCharacter != null)
                    DealingCharacter.Dealing.CallOwnerUpdateAnotherDealingGold(value);
            }
        }

        protected DealingCharacterItems _dealingItems = new DealingCharacterItems();
        public DealingCharacterItems DealingItems
        {
            get { return _dealingItems; }
            set
            {
                _dealingItems = value;
                CallOwnerUpdateDealingItems(value);
                if (DealingCharacter != null)
                    DealingCharacter.Dealing.CallOwnerUpdateAnotherDealingItems(value);
            }
        }

        private BasePlayerCharacterEntity _dealingCharacter;
        public BasePlayerCharacterEntity DealingCharacter
        {
            get
            {
                if (DealingState == DealingState.None && Time.unscaledTime - DealingCharacterTime >= CurrentGameInstance.dealingRequestDuration)
                    _dealingCharacter = null;
                return _dealingCharacter;
            }
            set
            {
                _dealingCharacter = value;
                DealingCharacterTime = Time.unscaledTime;
            }
        }

        /// <summary>
        /// Action: BasePlayerCharacterEntity anotherCharacter
        /// </summary>
        public event System.Action<BasePlayerCharacterEntity> onShowDealingRequestDialog;
        /// <summary>
        /// Action: BasePlayerCharacterEntity anotherCharacter
        /// </summary>
        public event System.Action<BasePlayerCharacterEntity> onShowDealingDialog;
        public event System.Action<DealingState> onUpdateDealingState;
        public event System.Action<DealingState> onUpdateAnotherDealingState;
        public event System.Action<int> onUpdateDealingGold;
        public event System.Action<int> onUpdateAnotherDealingGold;
        public event System.Action<DealingCharacterItems> onUpdateDealingItems;
        public event System.Action<DealingCharacterItems> onUpdateAnotherDealingItems;
        public float DealingCharacterTime { get; private set; }

        public bool DisableDealing
        {
            get
            {
                return CurrentGameInstance.disableDealing || BaseGameNetworkManager.CurrentMapInfo.DisableDealing;
            }
        }

        public bool ExchangingDealingItemsWillOverwhelming()
        {
            if (DealingCharacter == null)
                return true;
            List<ItemAmount> itemAmounts = new List<ItemAmount>();
            for (int i = 0; i < DealingItems.Count; ++i)
            {
                if (DealingItems[i].IsEmptySlot()) continue;
                itemAmounts.Add(new ItemAmount()
                {
                    item = DealingItems[i].GetItem(),
                    amount = DealingItems[i].amount,
                });
            }
            return DealingCharacter.IncreasingItemsWillOverwhelming(itemAmounts);
        }

        public void ExchangeDealingItemsAndGold()
        {
            if (DealingCharacter == null)
                return;
            List<CharacterItem> tempDealingItems = new List<CharacterItem>(DealingItems);
            CharacterItem nonEquipItem;
            CharacterItem dealingItem;
            int i, j;
            for (i = Entity.NonEquipItems.Count - 1; i >= 0; --i)
            {
                nonEquipItem = Entity.NonEquipItems[i];
                for (j = tempDealingItems.Count - 1; j >= 0; --j)
                {
                    dealingItem = tempDealingItems[j];
                    if (nonEquipItem.id == dealingItem.id && nonEquipItem.amount >= dealingItem.amount)
                    {
                        if (DealingCharacter.IncreaseItems(dealingItem))
                        {
                            GameInstance.ServerGameMessageHandlers.NotifyRewardItem(DealingCharacter.ConnectionId, RewardGivenType.Dealing, dealingItem.dataId, dealingItem.amount);
                            // Reduce item amount when able to increase item to co character
                            nonEquipItem.amount -= dealingItem.amount;
                            if (nonEquipItem.amount == 0)
                            {
                                // Amount is 0, remove it from inventory
                                if (CurrentGameInstance.IsLimitInventorySlot)
                                    Entity.NonEquipItems[i] = CharacterItem.Empty;
                                else
                                    Entity.NonEquipItems.RemoveAt(i);
                            }
                            else
                            {
                                // Update amount
                                Entity.NonEquipItems[i] = nonEquipItem;
                            }
                        }
                        tempDealingItems.RemoveAt(j);
                        break;
                    }
                }
            }
            Entity.FillEmptySlots();
            DealingCharacter.FillEmptySlots();
            Entity.Gold -= DealingGold;
            DealingCharacter.Gold = DealingCharacter.Gold.Increase(DealingGold);
            GameInstance.ServerGameMessageHandlers.NotifyRewardGold(DealingCharacter.ConnectionId, RewardGivenType.Dealing, DealingGold);
        }

        public void ClearDealingData()
        {
            DealingState = DealingState.None;
            DealingGold = 0;
            DealingItems.Clear();
        }

        public void StopDealing()
        {
            if (DealingCharacter == null)
            {
                ClearDealingData();
                return;
            }
            // Set dealing state/data for co player character entity
            DealingCharacter.Dealing.ClearDealingData();
            DealingCharacter.Dealing.DealingCharacter = null;
            // Set dealing state/data for player character entity
            ClearDealingData();
            DealingCharacter = null;
        }

        public bool CallServerSendDealingRequest(uint objectId)
        {
            RPC(ServerSendDealingRequest, objectId);
            return true;
        }

        [ServerRpc]
        protected void ServerSendDealingRequest(uint objectId)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DisableDealing)
            {
                // Dealing is disabled
                return;
            }
            BasePlayerCharacterEntity targetCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out targetCharacterEntity))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND);
                return;
            }
            if (targetCharacterEntity.Dealing.DealingCharacter != null)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_DEALING);
                return;
            }
            if (!Entity.IsGameEntityInDistance(targetCharacterEntity, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                return;
            }
            DealingCharacter = targetCharacterEntity;
            targetCharacterEntity.Dealing.DealingCharacter = Entity;
            // Send receive dealing request to player
            DealingCharacter.Dealing.CallOwnerReceiveDealingRequest(ObjectId);
#endif
        }

        public bool CallOwnerReceiveDealingRequest(uint objectId)
        {
            RPC(TargetReceiveDealingRequest, ConnectionId, objectId);
            return true;
        }

        [TargetRpc]
        protected void TargetReceiveDealingRequest(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingRequestDialog != null)
                onShowDealingRequestDialog.Invoke(playerCharacterEntity);
        }

        public bool CallServerAcceptDealingRequest()
        {
            RPC(ServerAcceptDealingRequest);
            return true;
        }

        [ServerRpc]
        protected void ServerAcceptDealingRequest()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DealingCharacter == null)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CANNOT_ACCEPT_DEALING_REQUEST);
                StopDealing();
                return;
            }
            if (!Entity.IsGameEntityInDistance(DealingCharacter, CurrentGameInstance.conversationDistance))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR);
                StopDealing();
                return;
            }
            // Set dealing state/data for co player character entity
            DealingCharacter.Dealing.ClearDealingData();
            DealingCharacter.Dealing.DealingState = DealingState.Dealing;
            DealingCharacter.Dealing.CallOwnerAcceptedDealingRequest(ObjectId);
            // Set dealing state/data for player character entity
            ClearDealingData();
            DealingState = DealingState.Dealing;
            CallOwnerAcceptedDealingRequest(DealingCharacter.ObjectId);
#endif
        }

        public bool CallServerDeclineDealingRequest()
        {
            RPC(ServerDeclineDealingRequest);
            return true;
        }

        [ServerRpc]
        protected void ServerDeclineDealingRequest()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DealingCharacter != null)
                GameInstance.ServerGameMessageHandlers.SendGameMessage(DealingCharacter.ConnectionId, UITextKeys.UI_ERROR_DEALING_REQUEST_DECLINED);
            GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_DEALING_REQUEST_DECLINED);
            StopDealing();
#endif
        }

        public bool CallOwnerAcceptedDealingRequest(uint objectId)
        {
            RPC(TargetAcceptedDealingRequest, ConnectionId, objectId);
            return true;
        }

        [TargetRpc]
        protected void TargetAcceptedDealingRequest(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (onShowDealingDialog != null)
                onShowDealingDialog.Invoke(playerCharacterEntity);
        }

        public bool CallServerSetDealingItem(string id, int amount)
        {
            RPC(ServerSetDealingItem, id, amount);
            return true;
        }

        [ServerRpc]
        protected void ServerSetDealingItem(string id, int amount)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DealingState != DealingState.Dealing)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_DEALING_STATE);
                return;
            }

            int indexOfNonEquipItem = Entity.IndexOfNonEquipItem(id);
            if (indexOfNonEquipItem < 0)
                return;

            DealingCharacterItems dealingItems = DealingItems;
            for (int i = dealingItems.Count - 1; i >= 0; --i)
            {
                if (id == dealingItems[i].id)
                {
                    dealingItems.RemoveAt(i);
                    break;
                }
            }
            CharacterItem dealingItem = Entity.NonEquipItems[indexOfNonEquipItem].Clone();
            dealingItem.amount = amount;
            dealingItems.Add(dealingItem);
            // Update to clients
            DealingItems = dealingItems;
#endif
        }

        public bool CallServerSetDealingGold(int dealingGold)
        {
            RPC(ServerSetDealingGold, dealingGold);
            return true;
        }

        [ServerRpc]
        protected void ServerSetDealingGold(int gold)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DealingState != DealingState.Dealing)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_DEALING_STATE);
                return;
            }
            if (gold > Entity.Gold)
                gold = Entity.Gold;
            if (gold < 0)
                gold = 0;
            DealingGold = gold;
#endif
        }

        public bool CallServerLockDealing()
        {
            RPC(ServerLockDealing);
            return true;
        }

        [ServerRpc]
        protected void ServerLockDealing()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DealingState != DealingState.Dealing)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_DEALING_STATE);
                return;
            }
            DealingState = DealingState.LockDealing;
#endif
        }

        public bool CallServerConfirmDealing()
        {
            RPC(ServerConfirmDealing);
            return true;
        }

        [ServerRpc]
        protected void ServerConfirmDealing()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DealingState != DealingState.LockDealing || !(DealingCharacter.Dealing.DealingState == DealingState.LockDealing || DealingCharacter.Dealing.DealingState == DealingState.ConfirmDealing))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_INVALID_DEALING_STATE);
                return;
            }
            DealingState = DealingState.ConfirmDealing;
            if (DealingState == DealingState.ConfirmDealing && DealingCharacter.Dealing.DealingState == DealingState.ConfirmDealing)
            {
                if (ExchangingDealingItemsWillOverwhelming())
                {
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_ANOTHER_CHARACTER_WILL_OVERWHELMING);
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(DealingCharacter.ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                }
                else if (DealingCharacter.Dealing.ExchangingDealingItemsWillOverwhelming())
                {
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                    GameInstance.ServerGameMessageHandlers.SendGameMessage(DealingCharacter.ConnectionId, UITextKeys.UI_ERROR_ANOTHER_CHARACTER_WILL_OVERWHELMING);
                }
                else
                {
                    ExchangeDealingItemsAndGold();
                    DealingCharacter.Dealing.ExchangeDealingItemsAndGold();
                }
                StopDealing();
            }
#endif
        }

        public bool CallServerCancelDealing()
        {
            RPC(ServerCancelDealing);
            return true;
        }

        [ServerRpc]
        protected void ServerCancelDealing()
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (DealingCharacter != null && DealingCharacter.Dealing.DealingState != DealingState.None)
                GameInstance.ServerGameMessageHandlers.SendGameMessage(DealingCharacter.ConnectionId, UITextKeys.UI_ERROR_DEALING_CANCELED);
            if (DealingState != DealingState.None)
                GameInstance.ServerGameMessageHandlers.SendGameMessage(ConnectionId, UITextKeys.UI_ERROR_DEALING_CANCELED);
            StopDealing();
#endif
        }

        public bool CallOwnerUpdateDealingState(DealingState state)
        {
            RPC(TargetUpdateDealingState, ConnectionId, state);
            return true;
        }

        [TargetRpc]
        protected void TargetUpdateDealingState(DealingState dealingState)
        {
            if (onUpdateDealingState != null)
                onUpdateDealingState.Invoke(dealingState);
        }

        public bool CallOwnerUpdateAnotherDealingState(DealingState state)
        {
            RPC(TargetUpdateAnotherDealingState, ConnectionId, state);
            return true;
        }

        [TargetRpc]
        protected void TargetUpdateAnotherDealingState(DealingState dealingState)
        {
            if (onUpdateAnotherDealingState != null)
                onUpdateAnotherDealingState.Invoke(dealingState);
        }

        public bool CallOwnerUpdateDealingGold(int gold)
        {
            RPC(TargetUpdateDealingGold, ConnectionId, gold);
            return true;
        }

        [TargetRpc]
        protected void TargetUpdateDealingGold(int gold)
        {
            if (onUpdateDealingGold != null)
                onUpdateDealingGold.Invoke(gold);
        }

        public bool CallOwnerUpdateAnotherDealingGold(int gold)
        {
            RPC(TargetUpdateAnotherDealingGold, ConnectionId, gold);
            return true;
        }

        [TargetRpc]
        protected void TargetUpdateAnotherDealingGold(int gold)
        {
            if (onUpdateAnotherDealingGold != null)
                onUpdateAnotherDealingGold.Invoke(gold);
        }

        public bool CallOwnerUpdateDealingItems(DealingCharacterItems dealingItems)
        {
            RPC(TargetUpdateDealingItems, ConnectionId, dealingItems);
            return true;
        }

        [TargetRpc]
        protected void TargetUpdateDealingItems(DealingCharacterItems items)
        {
            if (onUpdateDealingItems != null)
                onUpdateDealingItems.Invoke(items);
        }

        public bool CallOwnerUpdateAnotherDealingItems(DealingCharacterItems dealingItems)
        {
            RPC(TargetUpdateAnotherDealingItems, ConnectionId, dealingItems);
            return true;
        }

        [TargetRpc]
        protected void TargetUpdateAnotherDealingItems(DealingCharacterItems items)
        {
            if (onUpdateAnotherDealingItems != null)
                onUpdateAnotherDealingItems.Invoke(items);
        }
    }
}
