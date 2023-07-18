using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class PlayerCharacterVendingComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>
    {
        [SerializeField]
        protected SyncFieldVendingData data = new SyncFieldVendingData();

        public VendingData Data => data.Value;

        protected PlayerCharacterVendingComponent _store;
        protected HashSet<PlayerCharacterVendingComponent> _customers = new HashSet<PlayerCharacterVendingComponent>();
        protected VendingItems _items = new VendingItems();

        public event System.Action<VendingItems> onUpdateItems;
        public event System.Action<VendingData> onVendingDataChange;

        public override void OnSetup()
        {
            base.OnSetup();
            data.onChange += OnDataChange;
        }

        public override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            data.onChange -= OnDataChange;
        }

        protected void OnDataChange(bool isInitial, VendingData data)
        {
            if (onVendingDataChange != null)
                onVendingDataChange.Invoke(data);
        }

        public void StartVending(string title, StartVendingItems items)
        {
            RPC(ServerStartVending, title, items);
        }

        [ServerRpc]
        protected void ServerStartVending(string title, StartVendingItems items)
        {
            data.Value = new VendingData()
            {
                isStarted = true,
                title = title,
            };
            _items.Clear();
            foreach (StartVendingItem item in items)
            {
                if (string.IsNullOrEmpty(item.id) || item.amount <= 0)
                    continue;
                int index = Entity.NonEquipItems.IndexOf(item.id);
                if (index < 0)
                    continue;
                CharacterItem storeItem = Entity.NonEquipItems[index].Clone(false);
                storeItem.amount = item.amount;
                _items.Add(new VendingItem()
                {
                    item = storeItem,
                    price = item.price,
                });
            }
        }

        public void StopVending()
        {
            RPC(ServerStopVending);
        }

        [ServerRpc]
        protected void ServerStopVending()
        {
            data.Value = new VendingData();
            _items.Clear();
            foreach (PlayerCharacterVendingComponent customer in _customers)
            {
                if (customer == null)
                    continue;
            }
            _customers.Clear();
        }

        public void Subscribe(uint objectId)
        {
            RPC(ServerSubscribe, objectId);
        }

        [ServerRpc]
        protected void ServerSubscribe(uint objectId)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            if (!Manager.TryGetEntityByObjectId(objectId, out playerCharacterEntity))
                return;
            if (!playerCharacterEntity.Vending.Data.isStarted)
                return;
            ServerUnsubscribe();
            _store = playerCharacterEntity.Vending;
            _store.AddCustomer(this);
        }

        protected void AddCustomer(PlayerCharacterVendingComponent customer)
        {
            if (_customers.Add(customer))
                NotifyItems(customer.ConnectionId);
        }

        public void Unsubscribe()
        {
            RPC(ServerUnsubscribe);
        }

        [ServerRpc]
        protected void ServerUnsubscribe()
        {
            if (_store == null)
                return;
            _store.RemoveCustomer(this);
            _store = null;
        }

        protected void RemoveCustomer(PlayerCharacterVendingComponent customer)
        {
            _customers.Remove(customer);
        }

        protected void NotifyItems()
        {
            foreach (PlayerCharacterVendingComponent comp in _customers)
            {
                if (comp == null)
                    continue;
                NotifyItems(comp.ConnectionId);
            }
            NotifyItems(ConnectionId);
        }

        protected void NotifyItems(long connectionId)
        {
            RPC(TargetNotifyItems, connectionId, _items);
        }

        [TargetRpc]
        protected void TargetNotifyItems(VendingItems items)
        {
            if (onUpdateItems != null)
                onUpdateItems.Invoke(items);
        }

        public void BuyItem(int index)
        {
            RPC(ServerBuyItem, index);
        }

        [ServerRpc]
        protected void ServerBuyItem(int index)
        {
            _store.SellItem(this, index);
        }

        protected void SellItem(PlayerCharacterVendingComponent buyer, int index)
        {
            if (buyer == null || index < 0 || index >= _items.Count)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(buyer.ConnectionId, UITextKeys.UI_ERROR_INVALID_ITEM_INDEX);
                return;
            }
            int price = _items[index].price;
            if (buyer.Entity.Gold < price)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(buyer.ConnectionId, UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD);
                return;
            }
            CharacterItem sellingItem = _items[index].item;
            int inventoryItemIndex = Entity.NonEquipItems.IndexOf(sellingItem.id);
            if (inventoryItemIndex < 0)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(buyer.ConnectionId, UITextKeys.UI_ERROR_INVALID_ITEM_DATA);
                return;
            }
            if (!buyer.Entity.IncreaseItems(sellingItem))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(buyer.ConnectionId, UITextKeys.UI_ERROR_WILL_OVERWHELMING);
                return;
            }
            Entity.DecreaseItemsByIndex(inventoryItemIndex, sellingItem.amount, true);
            GameInstance.ServerGameMessageHandlers.NotifyRewardItem(buyer.ConnectionId, RewardGivenType.Vending, sellingItem.dataId, sellingItem.amount);

            buyer.Entity.Gold -= price;
            Entity.Gold = Entity.Gold.Increase(price);
            GameInstance.ServerGameMessageHandlers.NotifyRewardGold(ConnectionId, RewardGivenType.Vending, price);
            _items.RemoveAt(index);
            if (_items.Count <= 0)
            {
                // No item to sell anymore
                ServerStopVending();
            }
            else
            {
                // Update items to customer
                NotifyItems();
            }
        }
    }
}