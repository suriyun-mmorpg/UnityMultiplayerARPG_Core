using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UIItemCrafts : UIBase
    {
        [FormerlySerializedAs("uiCraftItemDialog")]
        public UIItemCraft uiDialog;
        [FormerlySerializedAs("uiCraftItemPrefab")]
        public UIItemCraft uiPrefab;
        [FormerlySerializedAs("uiCraftItemContainer")]
        public Transform uiContainer;

        private UIList cacheItemList;
        public UIList CacheItemList
        {
            get
            {
                if (cacheItemList == null)
                {
                    cacheItemList = gameObject.AddComponent<UIList>();
                    cacheItemList.uiPrefab = uiPrefab.gameObject;
                    cacheItemList.uiContainer = uiContainer;
                }
                return cacheItemList;
            }
        }

        private UIItemCraftSelectionManager cacheItemSelectionManager;
        public UIItemCraftSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = gameObject.GetOrAddComponent<UIItemCraftSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        public CrafterType CrafterType { get; private set; }
        public BaseGameEntity TargetEntity { get; private set; }

        public void Show(CrafterType crafterType, BaseGameEntity targetEntity)
        {
            CrafterType = crafterType;
            TargetEntity = targetEntity;
            switch (crafterType)
            {
                case CrafterType.Character:
                    BasePlayerCharacterEntity owningCharacter = GameInstance.PlayingCharacterEntity;
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

        protected virtual void OnEnable()
        {
            CacheItemSelectionManager.eventOnSelected.RemoveListener(OnSelectCraftItem);
            CacheItemSelectionManager.eventOnSelected.AddListener(OnSelectCraftItem);
            CacheItemSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCraftItem);
            CacheItemSelectionManager.eventOnDeselected.AddListener(OnDeselectCraftItem);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnItemDialogHide);
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnItemDialogHide()
        {
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCraftItem(UIItemCraft ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheItemSelectionManager;
                uiDialog.Setup(CrafterType, TargetEntity, ui.Data);
                uiDialog.Show();
            }
        }

        protected void OnDeselectCraftItem(UIItemCraft ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnItemDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnItemDialogHide);
            }
        }

        protected void UpdateData(IList<ItemCraft> itemCrafts)
        {
            int selectedIdx = CacheItemSelectionManager.SelectedUI != null ? CacheItemSelectionManager.IndexOf(CacheItemSelectionManager.SelectedUI) : -1;
            CacheItemSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.Clear();

            UIItemCraft tempUiCraftItem;
            CacheItemList.Generate(itemCrafts, (index, craftItem, ui) =>
            {
                tempUiCraftItem = ui.GetComponent<UIItemCraft>();
                tempUiCraftItem.Setup(CrafterType, TargetEntity, craftItem);
                tempUiCraftItem.Show();
                CacheItemSelectionManager.Add(tempUiCraftItem);
                if (selectedIdx == index)
                    tempUiCraftItem.OnClickSelect();
            });
        }
    }
}
