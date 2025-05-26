using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UINonEquipItems : UICharacterItems
    {
        [Header("Represent Items")]
        public bool showRepresentExp;
        public bool showRepresentGold;
        public bool showRepresentCurrencies;

        protected IPlayerCharacterData _updatedPlayerCharacterData = null;

        protected override void OnEnable()
        {
            base.OnEnable();
            UpdateOwningCharacterData();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _updatedPlayerCharacterData = null;
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncListOp operation, int index, CharacterItem oldItem, CharacterItem newItem)
        {
            UpdateOwningCharacterData();
        }

        public void UpdateOwningCharacterData()
        {
            if (GameInstance.PlayingCharacter == null) return;
            UpdateData(GameInstance.PlayingCharacter);
        }

        public void UpdateData(ICharacterData character)
        {
            _updatedPlayerCharacterData = character as IPlayerCharacterData;
            inventoryType = InventoryType.NonEquipItems;
            UpdateData(character, character.NonEquipItems);
        }

        protected override void OnListFiltered(List<KeyValuePair<int, CharacterItem>> filteredList)
        {
            if (_updatedPlayerCharacterData == null)
                return;

            if (showRepresentCurrencies)
            {
                Dictionary<int, int> currencies = _updatedPlayerCharacterData.GetCurrenciesByDataId();
                foreach (KeyValuePair<int, BaseItem> kv in GameInstance.CurrencyDropRepresentItems)
                {
                    int amount = 0;
                    if (!currencies.TryGetValue(kv.Key, out amount))
                        continue;
                    filteredList.Insert(0, new KeyValuePair<int, CharacterItem>(-1, CharacterItem.Create(kv.Value, amount: amount)));
                }
            }

            if (showRepresentGold)
            {
                filteredList.Insert(0, new KeyValuePair<int, CharacterItem>(-1, CharacterItem.Create(GameInstance.Singleton.GoldDropRepresentItem, amount: _updatedPlayerCharacterData.Gold)));
            }

            if (showRepresentExp)
            {
                filteredList.Insert(0, new KeyValuePair<int, CharacterItem>(-1, CharacterItem.Create(GameInstance.Singleton.ExpDropRepresentItem, amount: _updatedPlayerCharacterData.Exp)));
            }
        }

        public void OnClickSort()
        {
            GameInstance.ClientInventoryHandlers.RequestSortItems(new RequestSortItemsMessage(), ClientInventoryActions.ResponseSortItems);
        }
    }
}
