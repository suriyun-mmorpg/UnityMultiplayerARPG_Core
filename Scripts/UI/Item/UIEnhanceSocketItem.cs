using System.Collections.Generic;
using UnityEngine;

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
                if (uiSocketEnhancerItems.CacheSelectionManager != null &&
                    uiSocketEnhancerItems.CacheSelectionManager.SelectedUI != null &&
                    uiSocketEnhancerItems.CacheSelectionManager.SelectedUI.SocketEnhancerItem != null)
                    return uiSocketEnhancerItems.CacheSelectionManager.SelectedUI.SocketEnhancerItem.DataId;
                return 0;
            }
        }

        public int SelectedSocketIndex
        {
            get
            {
                if (uiAppliedSocketEnhancerItems.CacheSelectionManager != null &&
                    uiAppliedSocketEnhancerItems.CacheSelectionManager.SelectedUI != null)
                    return uiAppliedSocketEnhancerItems.CacheSelectionManager.SelectedUI.IndexOfData;
                return -1;
            }
        }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRemoveRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRemoveRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);

        [Header("UI Elements for UI Enhance Socket Item")]
        public UINonEquipItems uiSocketEnhancerItems;
        public UICharacterItems uiAppliedSocketEnhancerItems;
        public TextWrapper uiTextRemoveRequireGold;

        protected bool activated;
        protected string activeItemId;

        protected override void Update()
        {
            base.Update();

            if (uiTextRemoveRequireGold != null)
            {
                if (SelectedEnhancerId == 0)
                {
                    uiTextRemoveRequireGold.text = string.Format(
                        LanguageManager.GetText(formatKeyRemoveRequireGold),
                        "0",
                        "0");
                }
                else
                {
                    uiTextRemoveRequireGold.text = string.Format(
                        GameInstance.PlayingCharacter.Gold >= GameInstance.Singleton.enhancerRemoval.RequireGold ?
                            LanguageManager.GetText(formatKeyRemoveRequireGold) :
                            LanguageManager.GetText(formatKeyRemoveRequireGoldNotEnough),
                        GameInstance.PlayingCharacter.Gold.ToString("N0"),
                        GameInstance.Singleton.enhancerRemoval.RequireGold.ToString("N0"));
                }
            }
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
                uiSocketEnhancerItems.filterCategories = new List<string>();
                uiSocketEnhancerItems.UpdateData(GameInstance.PlayingCharacter);
            }

            if (uiAppliedSocketEnhancerItems != null)
            {
                uiAppliedSocketEnhancerItems.inventoryType = InventoryType.Unknow;
                uiAppliedSocketEnhancerItems.filterItemTypes = new List<ItemType>() { ItemType.SocketEnhancer };
                uiAppliedSocketEnhancerItems.filterCategories = new List<string>();
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
                index = (short)IndexOfData,
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
                index = (short)IndexOfData,
                socketIndex = (short)SelectedSocketIndex,
            }, ClientInventoryActions.ResponseRemoveEnhancerFromItem);
        }
    }
}
