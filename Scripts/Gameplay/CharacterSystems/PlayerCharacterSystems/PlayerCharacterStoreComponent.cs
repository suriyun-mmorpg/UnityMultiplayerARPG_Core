using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class PlayerCharacterStoreComponent : BaseNetworkedGameEntityComponent<BasePlayerCharacterEntity>
    {
        [SerializeField]
        protected SyncFieldPlayerStoreData data = new SyncFieldPlayerStoreData();

        public PlayerStoreData Data => data.Value;

        protected PlayerCharacterStoreComponent _store;
        protected HashSet<PlayerCharacterStoreComponent> _customers = new HashSet<PlayerCharacterStoreComponent>();
        protected PlayerStoreItems _items = new PlayerStoreItems();

        public event System.Action<PlayerStoreItems> onUpdateItems;

        public void OpenStore(string title, PlayerStoreOpenItems items)
        {
            RPC(ServerOpenStore, title, items);
        }

        [ServerRpc]
        protected void ServerOpenStore(string title, PlayerStoreOpenItems items)
        {
            data.Value = new PlayerStoreData()
            {
                isOpen = true,
                title = title,
            };
            _items.Clear();
            foreach (PlayerStoreOpenItem item in items)
            {
                if (string.IsNullOrEmpty(item.id) || item.amount <= 0 || item.price <= 0)
                    continue;
                int index = Entity.NonEquipItems.IndexOf(item.id);
                if (index < 0)
                    continue;
                CharacterItem storeItem = Entity.NonEquipItems[index].Clone(false);
                storeItem.amount = item.amount;
                _items.Add(new PlayerStoreItem()
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
            data.Value = new PlayerStoreData();
            _items.Clear();
            foreach (PlayerCharacterStoreComponent customer in _customers)
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
            if (!playerCharacterEntity.Store.Data.isOpen)
                return;
            ServerUnsubscribe();
            _store = playerCharacterEntity.Store;
            _store.AddCustomer(this);
        }

        protected void AddCustomer(PlayerCharacterStoreComponent customer)
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

        protected void RemoveCustomer(PlayerCharacterStoreComponent customer)
        {
            _customers.Remove(customer);
        }

        protected void NotifyItems()
        {
            foreach (PlayerCharacterStoreComponent comp in _customers)
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
        protected void TargetNotifyItems(uint objectId, PlayerStoreItems items)
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

        protected void SellItem(PlayerCharacterStoreComponent buyer, int index)
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