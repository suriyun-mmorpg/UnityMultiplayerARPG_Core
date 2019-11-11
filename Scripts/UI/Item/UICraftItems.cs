using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICraftItems : UIBase
    {
        public CrafterType CrafterType { get; private set; }
        public uint BuildingObjectId { get; private set; }

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
                switch (CrafterType)
                {
                    case CrafterType.Character:
                        uiCraftItemDialog.SetupForCharacter(ui.Data);
                        break;
                    case CrafterType.Npc:
                        uiCraftItemDialog.SetupForNpc(ui.Data);
                        break;
                    case CrafterType.Workbench:
                        uiCraftItemDialog.SetupForWorkbench(BuildingObjectId, ui.Data);
                        break;
                }
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

        public void UpdateDataForCharacter()
        {
            CrafterType = CrafterType.Character;
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            List<ItemCraft> itemCrafts = new List<ItemCraft>();
            foreach (CharacterSkill characterSkill in owningCharacter.Skills)
            {
                if (characterSkill == null ||
                    characterSkill.GetSkill() == null ||
                    characterSkill.GetSkill().GetSkillType() != SkillType.CraftItem)
                    continue;
                itemCrafts.Add(characterSkill.GetSkill().GetItemCraft());
            }
            UpdateData(itemCrafts);
        }

        public void UpdateDataForWorkbench(WorkbenchEntity workbenchEntity)
        {
            CrafterType = CrafterType.Workbench;
            BuildingObjectId = workbenchEntity.ObjectId;
            UpdateData(workbenchEntity.itemCrafts);
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
                switch (CrafterType)
                {
                    case CrafterType.Character:
                        tempUiCraftItem.SetupForCharacter(craftItem);
                        break;
                    case CrafterType.Npc:
                        tempUiCraftItem.SetupForNpc(craftItem);
                        break;
                    case CrafterType.Workbench:
                        tempUiCraftItem.SetupForWorkbench(BuildingObjectId, craftItem);
                        break;
                }
                tempUiCraftItem.Show();
                CacheItemSelectionManager.Add(tempUiCraftItem);
                if (selectedIdx == index)
                    tempUiCraftItem.OnClickSelect();
            });
        }
    }
}
