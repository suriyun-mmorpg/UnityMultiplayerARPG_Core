using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UIGuildIconEditing : UIBase
    {
        [Header("UI Elements")]
        public GameObject listEmptyObject;
        public UIGuildIcon uiPrefab;
        public Transform uiContainer;
        public UIGuildIcon[] selectedIcons;

        [Header("Options")]
        [FormerlySerializedAs("updateGuildOptionsOnSelectIcon")]
        public bool updateGuildOptionsOnSelect = false;

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

        private UIGuildIconSelectionManager cacheSelectionManager;
        public UIGuildIconSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UIGuildIconSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        private UIGuildIconUpdater guildIconUpdater;
        public UIGuildIconUpdater GuildIconUpdater
        {
            get
            {
                if (guildIconUpdater == null)
                    guildIconUpdater = gameObject.GetOrAddComponent<UIGuildIconUpdater>();
                return guildIconUpdater;
            }
        }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            if (GameInstance.JoinedGuild != null)
            {
                // Get current guild options before modify and save
                GuildOptions options = new GuildOptions();
                if (!string.IsNullOrEmpty(GameInstance.JoinedGuild.options))
                    options = JsonConvert.DeserializeObject<GuildOptions>(GameInstance.JoinedGuild.options);
                UpdateData(options.iconDataId);
            }
            else
            {
                UpdateData();
            }
        }

        protected virtual void OnSelect(UIGuildIcon ui)
        {
            UpdateSelectedIcons();
            if (updateGuildOptionsOnSelect)
                UpdateGuildOptions();
        }

        public void UpdateData()
        {
            int selectedDataId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.DataId : 0;
            UpdateData(selectedDataId);
        }

        public virtual void UpdateData(int selectedDataId)
        {
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            List<GuildIcon> list = new List<GuildIcon>(GameInstance.GuildIcons.Values);
            if (list.Count == 0)
            {
                CacheList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            if (listEmptyObject != null)
                listEmptyObject.SetActive(false);

            UIGuildIcon tempUI;
            CacheList.Generate(list, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UIGuildIcon>();
                tempUI.Data = data;
                tempUI.Show();
                CacheSelectionManager.Add(tempUI);
                if (index == 0 || selectedDataId == data.DataId)
                    tempUI.OnClickSelect();
            });
        }

        public virtual void UpdateSelectedIcons()
        {
            GuildIcon guildIcon = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data : null;
            if (selectedIcons != null && selectedIcons.Length > 0)
            {
                foreach (UIGuildIcon selectedIcon in selectedIcons)
                {
                    selectedIcon.Data = guildIcon;
                }
            }
        }

        public virtual void UpdateGuildOptions()
        {
            if (GameInstance.JoinedGuild == null)
            {
                // No joined guild data, so it can't update guild data
                return;
            }
            GuildIconUpdater.UpdateData(CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data : null);
        }
    }
}
