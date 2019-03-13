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
            CacheCraftItemSelectionManager.eventOnSelect.RemoveListener(OnSelectCraftItem);
            CacheCraftItemSelectionManager.eventOnSelect.AddListener(OnSelectCraftItem);
            CacheCraftItemSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCraftItem);
            CacheCraftItemSelectionManager.eventOnDeselect.AddListener(OnDeselectCraftItem);
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

        public void UpdateData(IList<ItemCraft> craftItems)
        {
            int selectedIdx = CacheCraftItemSelectionManager.SelectedUI != null ? CacheCraftItemSelectionManager.IndexOf(CacheCraftItemSelectionManager.SelectedUI) : -1;
            CacheCraftItemSelectionManager.DeselectSelectedUI();
            CacheCraftItemSelectionManager.Clear();
            
            CacheCraftItemList.Generate(craftItems, (index, craftItem, ui) =>
            {
                UICraftItem uiCraftItem = ui.GetComponent<UICraftItem>();
                uiCraftItem.Data = craftItem;
                uiCraftItem.Show();
                CacheCraftItemSelectionManager.Add(uiCraftItem);
                if (selectedIdx == index)
                    uiCraftItem.OnClickSelect();
            });
        }
    }
}
