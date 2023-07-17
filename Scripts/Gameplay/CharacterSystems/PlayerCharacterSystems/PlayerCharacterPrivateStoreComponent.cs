using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class PlayerCharacterPrivateStoreComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>
    {
        [SerializeField]
        protected SyncFieldPrivateStoreData data = new SyncFieldPrivateStoreData();

        public PrivateStoreData Data => data.Value;

        protected PlayerCharacterPrivateStoreComponent _store;
        protected HashSet<PlayerCharacterPrivateStoreComponent> _customers = new HashSet<PlayerCharacterPrivateStoreComponent>();
        protected PrivateStoreItems _items = new PrivateStoreItems();

        public event System.Action<PrivateStoreItems> onUpdateItems;

        public void OpenStore(string title, PrivateStoreOpenItems items)
        {
            RPC(ServerOpenStore, title, items);
        }

        [ServerRpc]
        protected void ServerOpenStore(string title, PrivateStoreOpenItems items)
        {
            data.Value = new PrivateStoreData()
            {
                isOpen = true,
                title = title,
            };
            _items.Clear();
            foreach (PrivateStoreOpenItem item in items)
            {
                if (string.IsNullOrEmpty(item.id) || item.amount <= 0 || item.price <= 0)
                    continue;
                int index = Entity.NonEquipItems.IndexOf(item.id);
                if (index < 0)
                    continue;
                CharacterItem storeItem = Entity.NonEquipItems[index].Clone(false);
                storeItem.amount = item.amount;
                _items.Add(new PrivateStoreItem()
                {
                    item = storeItem,
                    price = item.price,
                });
            }
        }

        public void CloseStore()
        {
            RPC(ServerCloseStore);
        }

        [ServerRpc]
        protected void ServerCloseStore()
        {
            data.Value = new PrivateStoreData();
            _items.Clear();
            foreach (PlayerCharacterPrivateStoreComponent customer in _customers)
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
            if (!playerCharacterEntity.PrivateStore.Data.isOpen)
                return;
            ServerUnsubscribe();
            _store = playerCharacterEntity.PrivateStore;
            _store.AddCustomer(this);
        }

        protected void AddCustomer(PlayerCharacterPrivateStoreComponent customer)
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

        protected void RemoveCustomer(PlayerCharacterPrivateStoreComponent customer)
        {
            _customers.Remove(customer);
        }

        protected void NotifyItems()
        {
            foreach (PlayerCharacterPrivateStoreComponent comp in _customers)
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
        protected void TargetNotifyItems(uint objectId, PrivateStoreItems items)
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

        protected void SellItem(PlayerCharacterPrivateStoreComponent buyer, int index)
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
                ServerCloseStore();
        }
    }
}