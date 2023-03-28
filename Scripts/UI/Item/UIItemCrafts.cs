using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UIItemCrafts : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        [FormerlySerializedAs("uiCraftItemDialog")]
        public UIItemCraft uiDialog;
        [FormerlySerializedAs("uiCraftItemPrefab")]
        public UIItemCraft uiPrefab;
        [FormerlySerializedAs("uiCraftItemContainer")]
        public Transform uiContainer;
        public bool selectFirstEntryByDefault;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiPrefab.gameObject;
                    cacheList.uiContainer = uiContainer;
                }
                return cacheList;
            }
        }

        private UIItemCraftSelectionManager cacheSelectionManager;
        public UIItemCraftSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIItemCraftSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
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
                        if (characterSkill == null || characterSkill.GetSkill() == null || !characterSkill.GetSkill().IsCraftItem)
                            continue;
                        itemCrafts.Add(characterSkill.GetSkill().ItemCraft);
                    }
                    UpdateData(itemCrafts);
                    break;
                case CrafterType.Workbench:
                    if (targetEntity && targetEntity is WorkbenchEntity workbenchEntity)
                        UpdateData(workbenchEntity.ItemCrafts);
                    break;
            }
            Show();
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselected.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselected.AddListener(OnDeselect);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnDialogHide);
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UIItemCraft ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Setup(CrafterType, TargetEntity, ui.Data);
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UIItemCraft ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        protected virtual void UpdateData(IList<ItemCraft> itemCrafts)
        {
            int selectedDataId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.CraftingItem.DataId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            UIItemCraft tempUI;
            CacheList.Generate(itemCrafts, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIItemCraft>();
                tempUI.Setup(CrafterType, TargetEntity, data);
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if ((selectFirstEntryByDefault && index == 0) || selectedDataId == data.CraftingItem.DataId)
                    tempUI.OnClickSelect();
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(itemCrafts.Count == 0);
        }
    }
}
