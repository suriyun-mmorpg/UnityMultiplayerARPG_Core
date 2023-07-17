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

        public void StartVending(string title, StartVendingItems items)
        {
            RPC(ServerStartVending, title, items);
        }

        [ServerRpc]
        protected void ServerStartVending(string title, StartVendingItems items)
        {
            data.Value = new VendingData()
            {
                isOpen = true,
                title = title,
            };
            _items.Clear();
            foreach (StartVendingItem item in items)
            {
                if (string.IsNullOrEmpty(item.id) || item.amount <= 0 || item.price <= 0)
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
            if (!playerCharacterEntity.Vending.Data.isOpen)
                return;
            ServerUnsubscribe();
            _store = playerCharacterEntity.Vending;
            _store.AddCustomer(this);
        }

        protected void AddCustomer(PlayerCharacterVendingComponent customer)
        {
            if (_customers.Add(customer))
                NotifyItems(customer.ObjectId);
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
                NotifyItems(comp.ObjectId);
            }
            NotifyItems(ObjectId);
        }

        protected void NotifyItems(uint objectId)
        {
            RPC(TargetNotifyItems, objectId, _items);
        }

        [TargetRpc]
        protected void TargetNotifyItems(uint objectId, VendingItems items)
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
            if (buyer.Entity.Gold < _items[index].price)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(buyer.ConnectionId, UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD);
                return;
            }
            _items.RemoveAt(index);
            if (_items.Count <= 0)
                ServerStopVending();
        }
    }
}