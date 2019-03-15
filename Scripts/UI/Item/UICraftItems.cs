using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICraftItems : UIBase
    {
        public CrafterType CrafterType { get; private set; }
        public uint BuildingObjectId { get; protected set; }

        public UICraftItem uiCraftItemDialog;
        public UICraftItem uiCraftItemPrefab;
        public Transform uiCraftItemContainer;

        private UIList cacheCraftItemList;
        public UIList CacheCraftItemList
        {
            get
            {
                if (cacheCraftItemList == null)
                {
                    cacheCraftItemList = gameObject.AddComponent<UIList>();
                    cacheCraftItemList.uiPrefab = uiCraftItemPrefab.gameObject;
                    cacheCraftItemList.uiContainer = uiCraftItemContainer;
                }
                return cacheCraftItemList;
            }
        }

        private UICraftItemSelectionManager cacheCraftItemSelectionManager;
        public UICraftItemSelectionManager CacheCraftItemSelectionManager
        {
            get
            {
                if (cacheCraftItemSelectionManager == null)
                    cacheCraftItemSelectionManager = GetComponent<UICraftItemSelectionManager>();
                if (cacheCraftItemSelectionManager == null)
                    cacheCraftItemSelectionManager = gameObject.AddComponent<UICraftItemSelectionManager>();
                cacheCraftItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCraftItemSelectionManager;
            }
        }

        public override void Show()
        {
            CacheCraftItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCraftItem);
            CacheCraftItemSelectionManager.eventOnSelected.AddListener(OnSelectCraftItem);
            CacheCraftItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCraftItem);
            CacheCraftItemSelectionManager.eventOnDeselected.AddListener(OnDeselectCraftItem);
            base.Show();
        }

        public override void Hide()
        {
            CacheCraftItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCraftItem(UICraftItem ui)
        {
            if (uiCraftItemDialog != null)
            {
                uiCraftItemDialog.selectionManager = CacheCraftItemSelectionManager;
                uiCraftItemDialog.Data = ui.Data;
                uiCraftItemDialog.Show();
            }
        }

        protected void OnDeselectCraftItem(UICraftItem ui)
        {
            if (uiCraftItemDialog != null)
                uiCraftItemDialog.Hide();
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
                    characterSkill.GetSkill().skillType != SkillType.CraftItem)
                    continue;
                itemCrafts.Add(characterSkill.GetSkill().itemCraft);
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
            int selectedIdx = CacheCraftItemSelectionManager.SelectedUI != null ? CacheCraftItemSelectionManager.IndexOf(CacheCraftItemSelectionManager.SelectedUI) : -1;
            CacheCraftItemSelectionManager.DeselectSelectedUI();
            CacheCraftItemSelectionManager.Clear();

            CacheCraftItemList.Generate(itemCrafts, (index, craftItem, ui) =>
            {
                UICraftItem uiCraftItem = ui.GetComponent<UICraftItem>();
                switch (CrafterType)
                {
                    case CrafterType.Character:
                        uiCraftItem.SetupForCharacter(craftItem);
                        break;
                    case CrafterType.Npc:
                        uiCraftItem.SetupForNpc(craftItem);
                        break;
                    case CrafterType.Workbench:
                        uiCraftItem.SetupForWorkbench(BuildingObjectId, craftItem);
                        break;
                }
                uiCraftItem.Show();
                CacheCraftItemSelectionManager.Add(uiCraftItem);
                if (selectedIdx == index)
                    uiCraftItem.OnClickSelect();
            });
        }
    }
}
