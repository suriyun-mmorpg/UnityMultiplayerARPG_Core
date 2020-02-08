using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using LiteNetLib;

namespace MultiplayerARPG
{
    public class CampFireEntity : StorageEntity
    {
        [Header("Campfire data")]
        public ConvertItem[] convertItems;
        public UnityEvent onInitialTurnOn;
        public UnityEvent onInitialTurnOff;
        public UnityEvent onTurnOn;
        public UnityEvent onTurnOff;
        [SerializeField]
        protected SyncFieldBool isTurnOn = new SyncFieldBool();

        private float tempDeltaTime;
        protected readonly Dictionary<int, float> convertElapsed = new Dictionary<int, float>();

        protected Dictionary<int, ConvertItem> cacheFuelItems = new Dictionary<int, ConvertItem>();
        public Dictionary<int, ConvertItem> CacheFuelItems
        {
            get
            {
                if (cacheFuelItems == null)
                {
                    cacheFuelItems = new Dictionary<int, ConvertItem>();
                    if (convertItems != null && convertItems.Length > 0)
                    {
                        foreach (ConvertItem convertItem in convertItems)
                        {
                            if (convertItem.item.item == null || !convertItem.isFuel) continue;
                            cacheFuelItems[convertItem.item.item.DataId] = convertItem;
                        }
                    }
                }
                return cacheFuelItems;
            }
        }

        protected Dictionary<int, ConvertItem> cacheConvertItems = new Dictionary<int, ConvertItem>();
        public Dictionary<int, ConvertItem> CacheConvertItems
        {
            get
            {
                if (cacheConvertItems == null)
                {
                    cacheConvertItems = new Dictionary<int, ConvertItem>();
                    if (convertItems != null && convertItems.Length > 0)
                    {
                        foreach (ConvertItem convertItem in convertItems)
                        {
                            if (convertItem.item.item == null) continue;
                            cacheConvertItems[convertItem.item.item.DataId] = convertItem;
                        }
                    }
                }
                return cacheConvertItems;
            }
        }

        public override void OnSetup()
        {
            base.OnSetup();
            isTurnOn.onChange += OnIsTurnOnChange;
            RegisterNetFunction(NetFuncTurnOn);
            RegisterNetFunction(NetFuncTurnOff);
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            isTurnOn.deliveryMethod = DeliveryMethod.ReliableOrdered;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            isTurnOn.onChange -= OnIsTurnOnChange;
        }

        private void OnIsTurnOnChange(bool isInitial, bool isTurnOn)
        {
            if (isInitial)
            {
                if (isTurnOn)
                    onInitialTurnOn.Invoke();
                else
                    onInitialTurnOff.Invoke();
            }
            else
            {
                if (isTurnOn)
                    onTurnOn.Invoke();
                else
                    onTurnOff.Invoke();
            }
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            if (!IsServer)
                return;

            if (!isTurnOn.Value)
            {
                if (convertElapsed.Count > 0)
                    convertElapsed.Clear();
                return;
            }

            if (!CanTurnOn())
                isTurnOn.Value = false;

            // Consume fuel and convert item
            tempDeltaTime = Time.unscaledDeltaTime;

            ConvertItem convertData;
            List<CharacterItem> items = new List<CharacterItem>(CurrentGameManager.GetStorageEntityItems(this));
            foreach (CharacterItem item in items)
            {
                if (!CacheConvertItems.ContainsKey(item.dataId))
                    continue;

                convertData = CacheConvertItems[item.dataId];
                if (convertData.item.amount > item.amount)
                {
                    if (convertElapsed.ContainsKey(item.dataId))
                        convertElapsed.Remove(item.dataId);
                    continue;
                }

                if (!convertElapsed.ContainsKey(item.dataId))
                    convertElapsed.Add(item.dataId, convertData.convertInterval);

                convertElapsed[item.dataId] -= tempDeltaTime;

                if (convertElapsed[item.dataId] <= 0f)
                {
                    convertElapsed[item.dataId] = convertData.convertInterval;
                    ConvertItem(convertData);
                }
            }
        }

        protected void ConvertItem(ConvertItem convertData)
        {
            StorageId storageId = new StorageId(StorageType.Building, Id);
            ItemAmount tempItemAmount = convertData.item;
            CurrentGameManager.DecreaseStorageItems(storageId, tempItemAmount.item.DataId, tempItemAmount.amount, null);
            if (convertData.convertedItem.item != null)
            {
                tempItemAmount = convertData.convertedItem;
                CharacterItem convertedItem = CharacterItem.Create(tempItemAmount.item.DataId, 1, tempItemAmount.amount);
                CurrentGameManager.IncreaseStorageItems(storageId, convertedItem, (success) =>
                {
                    ItemDropEntity.DropItem(this, convertedItem, new uint[0]);
                });
            }
        }

        protected void NetFuncTurnOn()
        {
            isTurnOn.Value = true;
        }

        protected void NetFuncTurnOff()
        {
            isTurnOn.Value = false;
        }

        public void RequestTurnOn()
        {
            CallNetFunction(NetFuncTurnOn, FunctionReceivers.Server);
        }

        public void RequestTurnOff()
        {
            CallNetFunction(NetFuncTurnOff, FunctionReceivers.Server);
        }

        public bool CanTurnOn()
        {
            if (CacheFuelItems.Count == 0)
            {
                // Not require fuel
                return true;
            }
            List<CharacterItem> items = CurrentGameManager.GetStorageEntityItems(this);
            foreach (CharacterItem item in items)
            {
                if (CacheFuelItems.ContainsKey(item.dataId) &&
                    CacheFuelItems[item.dataId].item.amount <= item.amount)
                    return true;
            }
            return true;
        }

        public override void PrepareRelatesData()
        {
            List<Item> items = new List<Item>();
            if (convertItems != null && convertItems.Length > 0)
            {
                foreach (ConvertItem convertItem in convertItems)
                {
                    items.Add(convertItem.item.item);
                    items.Add(convertItem.convertedItem.item);
                }
            }
            GameInstance.AddItems(items);
        }
    }
}
