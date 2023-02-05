using System.Collections.Generic;
using Cysharp.Text;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public class UIEnhanceSocketItem : UIBaseOwningCharacterItem
    {
        public IEquipmentItem EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public byte MaxSocket { get { return GameInstance.Singleton.GameplayRule.GetItemMaxSocket(GameInstance.PlayingCharacter, CharacterItem); } }
        public bool CanEnhance { get { return MaxSocket > 0 && CharacterItem.Sockets.Count < MaxSocket; } }
        public int SelectedEnhancerId
        {
            get
            {
                if (uiSocketEnhancerItems.CacheSelectionManager.SelectedUI != null &&
                    uiSocketEnhancerItems.CacheSelectionManager.SelectedUI.SocketEnhancerItem != null)
                    return uiSocketEnhancerItems.CacheSelectionManager.SelectedUI.SocketEnhancerItem.DataId;
                return 0;
            }
        }

        public int SelectedSocketIndex
        {
            get
            {
                if (uiAppliedSocketEnhancerItems.CacheSelectionManager.SelectedUI != null)
                    return uiAppliedSocketEnhancerItems.CacheSelectionManager.SelectedUI.IndexOfData;
                return -1;
            }
        }

        [Header("String Formats")]
        [FormerlySerializedAs("formatKeyRemoveRequireGold")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [FormerlySerializedAs("formatKeyRemoveRequireGoldNotEnough")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Target Amount}")]
        public UILocaleKeySetting formatKeySimpleRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);

        [Header("UI Elements for UI Enhance Socket Item")]
        public UINonEquipItems uiSocketEnhancerItems;
        public UICharacterItems uiAppliedSocketEnhancerItems;
        public UIItemAmounts uiRequireItemAmounts;
        public UICurrencyAmounts uiRequireCurrencyAmounts;
        [FormerlySerializedAs("uiTextRemoveRequireGold")]
        public TextWrapper uiTextRequireGold;
        public TextWrapper uiTextSimpleRequireGold;

        protected bool activated;
        protected string activeItemId;

        protected override void Update()
        {
            base.Update();

            if (uiRequireItemAmounts != null)
            {
                if (SelectedEnhancerId == 0 || GameInstance.Singleton.enhancerRemoval.RequireItems == null || GameInstance.Singleton.enhancerRemoval.RequireItems.Length == 0)
                {
                    uiRequireItemAmounts.Hide();
                }
                else
                {
                    uiRequireItemAmounts.displayType = UIItemAmounts.DisplayType.Requirement;
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = GameDataHelpers.CombineItems(GameInstance.Singleton.enhancerRemoval.RequireItems, null);
                }
            }

            if (uiRequireCurrencyAmounts != null)
            {
                if (SelectedEnhancerId == 0 || GameInstance.Singleton.enhancerRemoval.RequireCurrencies == null || GameInstance.Singleton.enhancerRemoval.RequireCurrencies.Length == 0)
                {
                    uiRequireCurrencyAmounts.Hide();
                }
                else
                {
                    uiRequireCurrencyAmounts.displayType = UICurrencyAmounts.DisplayType.Requirement;
                    uiRequireCurrencyAmounts.Show();
                    uiRequireCurrencyAmounts.Data = GameDataHelpers.CombineCurrencies(GameInstance.Singleton.enhancerRemoval.RequireCurrencies, null);
                }
            }

            if (uiTextRequireGold != null)
            {
                if (SelectedEnhancerId == 0)
                {
                    uiTextRequireGold.text = ZString.Format(
                        LanguageManager.GetText(formatKeyRequireGold),
                        "0",
                        "0");
                }
                else
                {
                    uiTextRequireGold.text = ZString.Format(
                        GameInstance.PlayingCharacter.Gold >= GameInstance.Singleton.enhancerRemoval.RequireGold ?
                            LanguageManager.GetText(formatKeyRequireGold) :
                            LanguageManager.GetText(formatKeyRequireGoldNotEnough),
                        GameInstance.PlayingCharacter.Gold.ToString("N0"),
                        GameInstance.Singleton.enhancerRemoval.RequireGold.ToString("N0"));
                }
            }

            if (uiTextSimpleRequireGold != null)
                uiTextSimpleRequireGold.text = string.Format(LanguageManager.GetText(formatKeySimpleRequireGold), SelectedEnhancerId == 0 ? "0" : GameInstance.Singleton.enhancerRemoval.RequireGold.ToString("N0"));
        }

        public override void OnUpdateCharacterItems()
        {
            if (!IsVisible())
                return;

            // Store data to variable so it won't lookup for data from property again
            CharacterItem characterItem = CharacterItem;

            if (activated && (characterItem.IsEmptySlot() || !characterItem.id.Equals(activeItemId)))
            {
                // Item's ID is difference to active item ID, so the item may be destroyed
                // So clear data
                Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
                return;
            }

            if (uiCharacterItem != null)
            {
                if (characterItem.IsEmptySlot())
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    uiCharacterItem.Setup(new UICharacterItemData(characterItem, InventoryType), GameInstance.PlayingCharacter, IndexOfData);
                    uiCharacterItem.Show();
                }
            }

            if (uiSocketEnhancerItems != null)
            {
                uiSocketEnhancerItems.filterItemTypes = new List<ItemType>() { ItemType.SocketEnhancer };
                uiSocketEnhancerItems.CacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                uiSocketEnhancerItems.UpdateData(GameInstance.PlayingCharacter);
            }

            if (uiAppliedSocketEnhancerItems != null)
            {
                uiAppliedSocketEnhancerItems.inventoryType = InventoryType.Unknow;
                uiAppliedSocketEnhancerItems.CacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                uiAppliedSocketEnhancerItems.filterItemTypes = new List<ItemType>() { ItemType.SocketEnhancer };
                List<CharacterItem> characterItems = new List<CharacterItem>();
                if (EquipmentItem != null)
                {
                    for (int i = 0; i < characterItem.Sockets.Count; ++i)
                    {
                        if (characterItem.Sockets[i] == 0)
                            characterItems.Add(CharacterItem.CreateEmptySlot());
                        else
                            characterItems.Add(CharacterItem.Create(characterItem.Sockets[i]));
                    }
                }
                uiAppliedSocketEnhancerItems.UpdateData(GameInstance.PlayingCharacter, characterItems);
            }
        }

        public override void Show()
        {
            base.Show();
            activated = false;
            OnUpdateCharacterItems();
        }

        public override void Hide()
        {
            base.Hide();
            Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickEnhanceSocket()
        {
            if (CharacterItem.IsEmptySlot() || SelectedEnhancerId == 0)
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            GameInstance.ClientInventoryHandlers.RequestEnhanceSocketItem(new RequestEnhanceSocketItemMessage()
            {
                inventoryType = InventoryType,
                index = IndexOfData,
                enhancerId = SelectedEnhancerId,
                socketIndex = -1,
            }, ClientInventoryActions.ResponseEnhanceSocketItem);
        }

        public void OnClickRemoveEnhancer()
        {
            if (CharacterItem.IsEmptySlot() || SelectedSocketIndex < 0)
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            GameInstance.ClientInventoryHandlers.RequestRemoveEnhancerFromItem(new RequestRemoveEnhancerFromItemMessage()
            {
                inventoryType = InventoryType,
                index = IndexOfData,
                socketIndex = SelectedSocketIndex,
            }, ClientInventoryActions.ResponseRemoveEnhancerFromItem);
        }
    }
}
