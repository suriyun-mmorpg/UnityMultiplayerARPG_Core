using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UIGuildIconEditing : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UIGuildIcon uiPrefab;
        public Transform uiContainer;
        public UIGuildIcon[] selectedIcons;

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

        private UIGuildIconEditingSelectionManager cacheSelectionManager;
        public UIGuildIconEditingSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                {
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIGuildIconEditingSelectionManager>();
                    cacheSelectionManager.selectionMode = UISelectionMode.Toggle;
                }
                return cacheSelectionManager;
            }
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
        }

        protected virtual void OnDisable()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UIGuildIcon ui)
        {
            if (selectedIcons != null && selectedIcons.Length > 0)
            {
                foreach (UIGuildIcon selectedIcon in selectedIcons)
                {
                    selectedIcon.Data = ui.Data;
                }
            }
        }

        public void UpdateData()
        {
            int selectedId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.DataId : 0;
            UpdateData(selectedId);
        }

        public virtual void UpdateData(int selectedId)
        {
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            CacheList.Generate(GameInstance.GuildIcons.Values, (index, data, ui) =>
            {
                UIGuildIcon uiGuildIcon = ui.GetComponent<UIGuildIcon>();
                uiGuildIcon.Data = data;
                uiGuildIcon.Show();
                CacheSelectionManager.Add(uiGuildIcon);
                if (selectedId == data.DataId)
                    uiGuildIcon.OnClickSelect();
            });
        }
    }
}
