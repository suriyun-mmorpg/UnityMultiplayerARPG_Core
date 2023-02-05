using UnityEngine;
using Cysharp.Text;
using System.Collections.Generic;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UIRefineItem : UIBaseOwningCharacterItem
    {
        public IEquipmentItem EquipmentItem { get { return CharacterItem != null ? CharacterItem.GetEquipmentItem() : null; } }
        public bool CanRefine { get; private set; }
        public bool ReachedMaxLevel { get; private set; }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD);
        [Tooltip("Format => {0} = {Current Gold Amount}, {1} = {Target Amount}")]
        public UILocaleKeySetting formatKeyRequireGoldNotEnough = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REQUIRE_GOLD_NOT_ENOUGH);
        [Tooltip("Format => {0} = {Target Amount}")]
        public UILocaleKeySetting formatKeySimpleRequireGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Rate * 100}")]
        public UILocaleKeySetting formatKeySuccessRate = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REFINE_SUCCESS_RATE);
        [Tooltip("Format => {0} = {Refining Level}")]
        public UILocaleKeySetting formatKeyRefiningLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_REFINING_LEVEL);

        [Header("UI Elements for UI Refine Item")]
        // TODO: This is deprecated
        [HideInInspector]
        public UICharacterItem uiRefiningItem;
        public UIItemAmounts uiRequireItemAmounts;
        public UICurrencyAmounts uiRequireCurrencyAmounts;
        public TextWrapper uiTextRequireGold;
        public TextWrapper uiTextSimpleRequireGold;
        public TextWrapper uiTextSuccessRate;
        public TextWrapper uiTextRefiningLevel;
        public UICharacterItems uiRefineEnhancerItems;
        public UICharacterItems uiAppliedRefineEnhancerItems;

        protected bool activated;
        protected string activeItemId;
        protected List<int> enhancerDataIds = new List<int>();

        protected override void Awake()
        {
            base.Awake();
            if (uiCharacterItem == null && uiRefiningItem != null)
                uiCharacterItem = uiRefiningItem;
        }

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (uiCharacterItem == null && uiRefiningItem != null)
            {
                uiCharacterItem = uiRefiningItem;
                EditorUtility.SetDirty(this);
            }
#endif
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

            CanRefine = false;
            ReachedMaxLevel = false;
            ItemRefineLevel? refineLevel = null;
            float increaseSuccessRate = 0f;
            float decreaseRequireGoldRate = 0f;
            float chanceToNotDecreaseLevels = 0f;
            float chanceToNotDestroyItem = 0f;
            if (!characterItem.IsEmptySlot())
            {
                UITextKeys gameMessage = UITextKeys.UI_ERROR_CANNOT_REFINE;
                CanRefine = EquipmentItem != null && characterItem.GetItem().CanRefine(GameInstance.PlayingCharacter, Level, enhancerDataIds.ToArray(), out gameMessage);
                if (CanRefine)
                {
                    refineLevel = EquipmentItem.ItemRefine.Levels[Level - 1];
                }
                else
                {
                    switch (gameMessage)
                    {
                        case UITextKeys.UI_ERROR_REFINE_ITEM_REACHED_MAX_LEVEL:
                            ReachedMaxLevel = true;
                            break;
                        case UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD:
                        case UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS:
                        case UITextKeys.UI_ERROR_NOT_ENOUGH_CURRENCY_AMOUNTS:
                            refineLevel = EquipmentItem.ItemRefine.Levels[Level - 1];
                            break;
                    }
                }
            }

            if (uiRefineEnhancerItems != null)
            {
                uiRefineEnhancerItems.inventoryType = InventoryType.Unknow;
                uiRefineEnhancerItems.CacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                List<CharacterItem> characterItems = new List<CharacterItem>();
                if (refineLevel.HasValue)
                {
                    for (int i = 0; i < GameInstance.PlayingCharacter.NonEquipItems.Count; ++i)
                    {
                        for (int j = 0; j < refineLevel.Value.AvailableEnhancers.Length; ++j)
                        {
                            if (refineLevel.Value.AvailableEnhancers[j].item == GameInstance.PlayingCharacter.NonEquipItems[i].GetItem())
                                characterItems.Add(GameInstance.PlayingCharacter.NonEquipItems[i].Clone());
                        }
                    }
                    for (int i = 0; i < enhancerDataIds.Count; ++i)
                    {
                        characterItems.DecreaseItems(enhancerDataIds[i], 1, false, out _);
                    }
                }
                uiRefineEnhancerItems.UpdateData(GameInstance.PlayingCharacter, characterItems);
            }

            if (uiAppliedRefineEnhancerItems != null)
            {
                uiAppliedRefineEnhancerItems.inventoryType = InventoryType.Unknow;
                uiAppliedRefineEnhancerItems.CacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                List<CharacterItem> characterItems = new List<CharacterItem>();
                if (refineLevel.HasValue)
                {
                    for (int i = 0; i < enhancerDataIds.Count; ++i)
                    {
                        characterItems.Add(CharacterItem.Create(enhancerDataIds[i]));
                        for (int j = 0; j < refineLevel.Value.AvailableEnhancers.Length; ++j)
                        {
                            if (refineLevel.Value.AvailableEnhancers[j].item.DataId == enhancerDataIds[i])
                            {
                                increaseSuccessRate += refineLevel.Value.AvailableEnhancers[j].increaseSuccessRate;
                                decreaseRequireGoldRate += refineLevel.Value.AvailableEnhancers[j].decreaseRequireGoldRate;
                                chanceToNotDecreaseLevels += refineLevel.Value.AvailableEnhancers[j].chanceToNotDecreaseLevels;
                                chanceToNotDestroyItem += refineLevel.Value.AvailableEnhancers[j].chanceToNotDestroyItem;
                            }
                        }
                    }
                }
                uiAppliedRefineEnhancerItems.UpdateData(GameInstance.PlayingCharacter, characterItems);
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

            if (uiRequireItemAmounts != null)
            {
                if (!refineLevel.HasValue || refineLevel.Value.RequireItems == null || refineLevel.Value.RequireItems.Length == 0)
                {
                    uiRequireItemAmounts.Hide();
                }
                else
                {
                    uiRequireItemAmounts.displayType = UIItemAmounts.DisplayType.Requirement;
                    uiRequireItemAmounts.Show();
                    uiRequireItemAmounts.Data = GameDataHelpers.CombineItems(refineLevel.Value.RequireItems, null);
                }
            }

            if (uiRequireCurrencyAmounts != null)
            {
                if (!refineLevel.HasValue || refineLevel.Value.RequireCurrencies == null || refineLevel.Value.RequireCurrencies.Length == 0)
                {
                    uiRequireCurrencyAmounts.Hide();
                }
                else
                {
                    uiRequireCurrencyAmounts.displayType = UICurrencyAmounts.DisplayType.Requirement;
                    uiRequireCurrencyAmounts.Show();
                    uiRequireCurrencyAmounts.Data = GameDataHelpers.CombineCurrencies(refineLevel.Value.RequireCurrencies, null);
                }
            }

            if (uiTextRequireGold != null)
            {
                if (!refineLevel.HasValue)
                {
                    uiTextRequireGold.text = ZString.Format(
                        LanguageManager.GetText(formatKeyRequireGold),
                        "0",
                        "0");
                }
                else
                {
                    uiTextRequireGold.text = ZString.Format(
                        GameInstance.PlayingCharacter.Gold >= refineLevel.Value.RequireGold ?
                            LanguageManager.GetText(formatKeyRequireGold) :
                            LanguageManager.GetText(formatKeyRequireGoldNotEnough),
                        GameInstance.PlayingCharacter.Gold.ToString("N0"),
                        GameInstance.Singleton.GameplayRule.GetRefineItemRequireGold(GameInstance.PlayingCharacter, refineLevel.Value, decreaseRequireGoldRate).ToString("N0"));
                }
            }

            if (uiTextSimpleRequireGold != null)
                uiTextSimpleRequireGold.text = ZString.Format(LanguageManager.GetText(formatKeySimpleRequireGold), !refineLevel.HasValue ? "0" : refineLevel.Value.RequireGold.ToString("N0"));

            if (uiTextSuccessRate != null)
            {
                if (!refineLevel.HasValue)
                {
                    uiTextSuccessRate.text = ZString.Format(
                        LanguageManager.GetText(formatKeySuccessRate),
                        0.ToString("N2"));
                }
                else
                {
                    uiTextSuccessRate.text = ZString.Format(
                        LanguageManager.GetText(formatKeySuccessRate),
                        ((refineLevel.Value.SuccessRate + increaseSuccessRate) * 100).ToString("N2"));
                }
            }

            if (uiTextRefiningLevel != null)
            {
                if (!refineLevel.HasValue)
                {
                    uiTextRefiningLevel.text = ZString.Format(
                        LanguageManager.GetText(formatKeyRefiningLevel),
                        (Level - 1).ToString("N0"));
                }
                else
                {
                    uiTextRefiningLevel.text = ZString.Format(
                        LanguageManager.GetText(formatKeyRefiningLevel),
                        Level.ToString("N0"));
                }
            }
        }

        public override void Show()
        {
            base.Show();
            activated = false;
            enhancerDataIds.Clear();
            OnUpdateCharacterItems();
        }

        public override void Hide()
        {
            base.Hide();
            Data = new UIOwningCharacterItemData(InventoryType.NonEquipItems, -1);
        }

        public void OnClickRefine()
        {
            if (CharacterItem.IsEmptySlot())
                return;
            activated = true;
            activeItemId = CharacterItem.id;
            GameInstance.ClientInventoryHandlers.RequestRefineItem(new RequestRefineItemMessage()
            {
                inventoryType = InventoryType,
                index = IndexOfData,
                enhancerDataIds = enhancerDataIds.ToArray(),
            }, ResponseRefineItem);
        }

        protected void ResponseRefineItem(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseRefineItemMessage response)
        {
            ClientInventoryActions.ResponseRefineItem(requestHandler, responseCode, response);
            enhancerDataIds.Clear();
            OnUpdateCharacterItems();
        }

        public void OnClickAddRefineEnhancer()
        {
            List<UICharacterItem> selectedUIs = uiRefineEnhancerItems.CacheSelectionManager.GetSelectedUIs();
            if (selectedUIs.Count == 0)
                return;
            foreach (UICharacterItem selectedUI in selectedUIs)
            {
                enhancerDataIds.Add(selectedUI.CharacterItem.dataId);
            }
            OnUpdateCharacterItems();
        }

        public void OnClickRemoveRefineEnhancer()
        {
            List<UICharacterItem> selectedUIs = uiAppliedRefineEnhancerItems.CacheSelectionManager.GetSelectedUIs();
            if (selectedUIs.Count == 0)
                return;
            foreach (UICharacterItem selectedUI in selectedUIs)
            {
                enhancerDataIds.Remove(selectedUI.CharacterItem.dataId);
            }
            OnUpdateCharacterItems();
        }
    }
}
