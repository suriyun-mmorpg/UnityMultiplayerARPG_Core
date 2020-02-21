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
        [SerializeField]
        protected SyncFieldFloat turnOnElapsed = new SyncFieldFloat();

        public bool IsTurnOn
        {
            get { return isTurnOn.Value; }
            set { isTurnOn.Value = value; }
        }

        public float TurnOnElapsed
        {
            get { return turnOnElapsed.Value; }
            set { turnOnElapsed.Value = value; }
        }

        private float tempDeltaTime;
        protected readonly Dictionary<int, float> convertRemainsDuration = new Dictionary<int, float>();

        protected Dictionary<int, ConvertItem> cacheFuelItems;
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

        protected Dictionary<int, ConvertItem> cacheConvertItems;
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
        }

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            isTurnOn.deliveryMethod = DeliveryMethod.ReliableOrdered;
            isTurnOn.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            turnOnElapsed.deliveryMethod = DeliveryMethod.Sequenced;
            turnOnElapsed.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
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

            if (!IsTurnOn)
            {
                if (convertRemainsDuration.Count > 0)
                    convertRemainsDuration.Clear();
                return;
            }

            if (!CanTurnOn())
            {
                IsTurnOn = false;
                TurnOnElapsed = 0f;
                return;
            }

            // Consume fuel and convert item
            tempDeltaTime = Time.unscaledDeltaTime;
            TurnOnElapsed += tempDeltaTime;

            ConvertItem convertData;
            List<CharacterItem> items = new List<CharacterItem>(CurrentGameManager.GetStorageEntityItems(this));
            foreach (CharacterItem item in items)
            {
                if (!CacheConvertItems.ContainsKey(item.dataId))
                    continue;

                convertData = CacheConvertItems[item.dataId];
                if (item.amount < convertData.item.amount)
                {
                    if (convertRemainsDuration.ContainsKey(item.dataId))
                        convertRemainsDuration.Remove(item.dataId);
                    continue;
                }

                if (!convertRemainsDuration.ContainsKey(item.dataId))
                    convertRemainsDuration.Add(item.dataId, convertData.convertInterval);

                convertRemainsDuration[item.dataId] -= tempDeltaTime;

                if (convertRemainsDuration[item.dataId] <= 0f)
                {
                    convertRemainsDuration[item.dataId] = convertData.convertInterval;
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
                    if (!success)
                    {
                        // Cannot add item to storage, so drop to ground
                        ItemDropEntity.DropItem(this, convertedItem, new uint[0]);
                    }
                });
            }
        }

        public void TurnOn()
        {
            if (!CanTurnOn())
                return;
            IsTurnOn = true;
            TurnOnElapsed = 0f;
        }

        public void TurnOff()
        {
            IsTurnOn = false;
            TurnOnElapsed = 0f;
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
                    item.amount >= CacheFuelItems[item.dataId].item.amount)
                {
                    return true;
                }
            }
            return false;
        }

        public override void PrepareRelatesData()
        {
            List<BaseItem> items = new List<BaseItem>();
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
