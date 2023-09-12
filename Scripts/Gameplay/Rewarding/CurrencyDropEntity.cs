using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CurrencyDropEntity : BaseRewardDropEntity, IPickupActivatableEntity
    {
        [System.Serializable]
        public struct CurrencyAppearanceSetting
        {
            public Currency currency;
            public GameObject[] activatingObjects;
        }

        protected Currency _currency;
        public Currency Currency
        {
            get
            {
                return _currency;
            }
            set
            {
                _currency = value;
                CurrencyDataId = _currency == null ? 0 : _currency.DataId;
            }
        }

        [Category("Relative GameObjects/Transforms")]
        public List<CurrencyAppearanceSetting> currencyAppearanceSettings = new List<CurrencyAppearanceSetting>();

        [Category("Sync Fields")]
        [SerializeField]
        protected SyncFieldInt currencyDataId = new SyncFieldInt();
        public int CurrencyDataId
        {
            get { return currencyDataId.Value; }
            set { currencyDataId.Value = value; }
        }

        private List<GameObject> _allCurrencyActivatingObjects = new List<GameObject>();
        private Dictionary<int, CurrencyAppearanceSetting> _currencyAppearanceSettings = new Dictionary<int, CurrencyAppearanceSetting>();

        protected override void SetupNetElements()
        {
            base.SetupNetElements();
            currencyDataId.deliveryMethod = DeliveryMethod.ReliableOrdered;
            currencyDataId.syncMode = LiteNetLibSyncField.SyncMode.ServerToClients;
            if (currencyAppearanceSettings != null && currencyAppearanceSettings.Count > 0)
            {
                foreach (CurrencyAppearanceSetting setting in currencyAppearanceSettings)
                {
                    if (setting.currency == null || setting.activatingObjects == null || setting.activatingObjects.Length <= 0)
                        continue;
                    _currencyAppearanceSettings[setting.currency.DataId] = setting;
                    foreach (GameObject activatingObject in setting.activatingObjects)
                    {
                        activatingObject.SetActive(false);
                        _allCurrencyActivatingObjects.Add(activatingObject);
                    }
                }
            }
        }

        public override void OnSetup()
        {
            base.OnSetup();
            currencyDataId.onChange += OnCurrencyDataIdChange;
        }

        protected override void EntityOnDestroy()
        {
            base.EntityOnDestroy();
            currencyDataId.onChange -= OnCurrencyDataIdChange;
        }

        protected virtual void OnCurrencyDataIdChange(bool isInitial, int dataId)
        {
            // Instantiate model at clients
            if (!IsClient)
                return;
            if (_allCurrencyActivatingObjects != null && _allCurrencyActivatingObjects.Count > 0)
            {
                foreach (GameObject obj in _allCurrencyActivatingObjects)
                {
                    if (obj.activeSelf)
                        obj.SetActive(false);
                }
            }
            if (_currencyAppearanceSettings.TryGetValue(dataId, out CurrencyAppearanceSetting usingSetting))
            {
                if (usingSetting.activatingObjects != null && usingSetting.activatingObjects.Length > 0)
                {
                    foreach (GameObject obj in usingSetting.activatingObjects)
                    {
                        obj.SetActive(true);
                    }
                }
            }
        }

        public static CurrencyDropEntity Drop(BaseGameEntity dropper, float multiplier, RewardGivenType givenType, int giverLevel, int sourceLevel, Currency currency, int amount, IEnumerable<string> looters)
        {
            CurrencyDropEntity entity = Drop(GameInstance.Singleton.currencyDropEntityPrefab, dropper, multiplier, givenType, giverLevel, sourceLevel, amount, looters, GameInstance.Singleton.itemAppearDuration) as CurrencyDropEntity;
            entity.Currency = currency;
            return entity;
        }

        protected override bool ProceedPickingUpAtServer_Implementation(BaseCharacterEntity characterEntity, out UITextKeys message)
        {
            if (Currency == null)
            {
                message = UITextKeys.UI_ERROR_INVALID_DATA;
                return false;
            }
            BaseCharacterEntity rewardingCharacter = characterEntity;
            if (characterEntity is BaseMonsterCharacterEntity monsterCharacterEntity && monsterCharacterEntity.Summoner is BasePlayerCharacterEntity summonerCharacterEntity)
                rewardingCharacter = summonerCharacterEntity;
            CurrentGameplayRule.RewardCurrencies(rewardingCharacter, new List<CurrencyAmount>()
            {
                new CurrencyAmount()
                {
                    currency = Currency,
                    amount = Amount,
                },
            }, Multiplier, GivenType, GiverLevel, SourceLevel);
            GameInstance.ServerGameMessageHandlers.NotifyRewardCurrency(ConnectionId, GivenType, Currency.DataId, Amount);
            message = UITextKeys.NONE;
            return true;
        }
    }
}
