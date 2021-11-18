using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterBuffs : UIBase
    {
        [FormerlySerializedAs("uiBuffDialog")]
        public UICharacterBuff uiDialog;
        [FormerlySerializedAs("uiCharacterBuffPrefab")]
        public UICharacterBuff uiPrefab;
        [FormerlySerializedAs("uiCharacterBuffContainer")]
        public Transform uiContainer;

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

        private UICharacterBuffSelectionManager cacheSelectionManager;
        public UICharacterBuffSelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UICharacterBuffSelectionManager>();
                cacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSelectionManager;
            }
        }

        public virtual ICharacterData Character { get; protected set; }

        protected virtual void OnEnable()
        {
            CacheSelectionManager.eventOnSelect.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelect.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselect.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselect.AddListener(OnDeselect);
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

        protected virtual void OnSelect(UICharacterBuff ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSelectionManager;
                uiDialog.Setup(ui.Data, Character, ui.IndexOfData);
                uiDialog.Show();
            }
        }

        protected virtual void OnDeselect(UICharacterBuff ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnDialogHide);
            }
        }

        public virtual void UpdateData(ICharacterData character)
        {
            this.Character = character;
            string selectedId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.CharacterBuff.id : string.Empty;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            if (character == null || character.CurrentHp <= 0)
            {
                CacheList.HideAll();
                return;
            }

            UICharacterBuff tempUI;
            CacheList.Generate(character.Buffs, (index, data, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterBuff>();
                if (data.buffRemainsDuration > 0)
                {
                    tempUI.Setup(data, character, index);
                    tempUI.Show();
                    CacheSelectionManager.Add(tempUI);
                    if (!string.IsNullOrEmpty(selectedId) && selectedId.Equals(data.id))
                        tempUI.OnClickSelect();
                }
                else
                {
                    tempUI.Hide();
                }
            });
        }
    }
}
