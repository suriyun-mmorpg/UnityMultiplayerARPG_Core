using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICraftItems : UIBase
    {
        public UICraftItem uiCraftItemDialog;
        public UICraftItem uiCraftItemPrefab;
        public Transform uiCraftItemContainer;

        private UIList cacheItemList;
        public UIList CacheItemList
        {
            get
            {
                if (cacheItemList == null)
                {
                    cacheItemList = gameObject.AddComponent<UIList>();
                    cacheItemList.uiPrefab = uiCraftItemPrefab.gameObject;
                    cacheItemList.uiContainer = uiCraftItemContainer;
                }
                return cacheItemList;
            }
        }

        private UICraftItemSelectionManager cacheItemSelectionManager;
        public UICraftItemSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = GetComponent<UICraftItemSelectionManager>();
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = gameObject.AddComponent<UICraftItemSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        public CrafterType CrafterType { get; private set; }
        public BaseGameEntity TargetEntity { get; private set; }

        public override void Show()
        {
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCraftItem);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelectCraftItem);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCraftItem);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselectCraftItem);
            if (uiCraftItemDialog != null)
                uiCraftItemDialog.onHide.AddListener(OnItemDialogHide);
            base.Show();
        }

        public void Show(CrafterType crafterType, BaseGameEntity targetEntity)
        {
            CrafterType = crafterType;
            TargetEntity = targetEntity;
            switch (crafterType)
            {
                case CrafterType.Character:
                    BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
                    List<ItemCraft> itemCrafts = new List<ItemCraft>();
                    foreach (CharacterSkill characterSkill in owningCharacter.Skills)
                    {
                        if (characterSkill == null ||
                            characterSkill.GetSkill() == null ||
                            !characterSkill.GetSkill().IsCraftItem())
                            continue;
                        itemCrafts.Add(characterSkill.GetSkill().GetItemCraft());
                    }
                    UpdateData(itemCrafts);
                    break;
                case CrafterType.Workbench:
                    if (targetEntity && targetEntity is WorkbenchEntity)
                        UpdateData((targetEntity as WorkbenchEntity).itemCrafts);
                    break;
            }
            Show();
        }

        public override void Hide()
        {
            if (uiCraftItemDialog != null)
                uiCraftItemDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnItemDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCraftItem(UICraftItem ui)
        {
            if (uiCraftItemDialog != null)
            {
                uiCraftItemDialog.selectionManager = CacheItemSelectionManager;
                uiCraftItemDialog.Setup(CrafterType, TargetEntity, ui.Data);
                uiCraftItemDialog.Show();
            }
        }

        protected void OnDeselectCraftItem(UICraftItem ui)
        {
            if (uiCraftItemDialog != null)
            {
                uiCraftItemDialog.onHide.RemoveListener(OnItemDialogHide);
                uiCraftItemDialog.Hide();
                uiCraftItemDialog.onHide.AddListener(OnItemDialogHide);
            }
        }

        protected void UpdateData(IList<ItemCraft> itemCrafts)
        {
            int selectedIdx = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.IndexOf(CacheItemSelectionManager.SelectedUI) : -1;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();

            UICraftItem tempUiCraftItem;
            CacheItemList.Generate(itemCrafts, (index, craftItem, ui) =>
            {
                tempUiCraftItem = ui.GetComponent<UICraftItem>();
                tempUiCraftItem.Setup(CrafterType, TargetEntity, craftItem);
                tempUiCraftItem.Show();
                CacheItemSelectionManager.Add(tempUiCraftItem);
                if (selectedIdx == index)
                    tempUiCraftItem.OnClickSelect();
            });
        }
    }
}
