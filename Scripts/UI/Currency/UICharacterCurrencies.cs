using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterCurrencies : UIBase
    {
        public UICharacterCurrency uiCurrencyDialog;
        public UICharacterCurrency uiPrefab;
        public Transform uiContainer;

        public virtual ICharacterData Character { get; set; }

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

        private UICharacterCurrencySelectionManager cacheSelectionManager;
        public UICharacterCurrencySelectionManager CacheSelectionManager
        {
            get
            {
                if (cacheSelectionManager == null)
                    cacheSelectionManager = gameObject.GetOrAddComponent<UICharacterCurrencySelectionManager>();
                return cacheSelectionManager;
            }
        }

        private UISelectionMode dirtySelectionMode;

        protected virtual void OnEnable()
        {
            CacheSelectionManager.selectionMode = UISelectionMode.SelectSingle;
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelectCharacterItem);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelectCharacterItem);
            CacheSelectionManager.eventOnDeselected.RemoveListener(OnDeselectCharacterItem);
            CacheSelectionManager.eventOnDeselected.AddListener(OnDeselectCharacterItem);
            if (uiCurrencyDialog != null)
                uiCurrencyDialog.onHide.AddListener(OnItemDialogHide);
        }

        protected virtual void OnDisable()
        {
            if (uiCurrencyDialog != null)
                uiCurrencyDialog.onHide.RemoveListener(OnItemDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnItemDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelectCharacterItem(UICharacterCurrency ui)
        {
            if (ui.Data.characterCurrency.GetCurrency() == null)
            {
                CacheSelectionManager.DeselectSelectedUI();
                return;
            }
            if (uiCurrencyDialog != null && CacheSelectionManager.selectionMode == UISelectionMode.SelectSingle)
            {
                uiCurrencyDialog.selectionManager = CacheSelectionManager;
                uiCurrencyDialog.Setup(ui.Data, Character, ui.IndexOfData);
                uiCurrencyDialog.Show();
            }
        }

        protected virtual void OnDeselectCharacterItem(UICharacterCurrency ui)
        {
            if (uiCurrencyDialog != null && CacheSelectionManager.selectionMode == UISelectionMode.SelectSingle)
            {
                uiCurrencyDialog.onHide.RemoveListener(OnItemDialogHide);
                uiCurrencyDialog.Hide();
                uiCurrencyDialog.onHide.AddListener(OnItemDialogHide);
            }
        }

        public virtual void UpdateData(ICharacterData character, IList<CharacterCurrency> characterCurrencies)
        {
            Character = character;
            int selectedId = CacheSelectionManager.SelectedUI != null ? CacheSelectionManager.SelectedUI.Data.characterCurrency.dataId : 0;
            CacheSelectionManager.DeselectSelectedUI();
            CacheSelectionManager.Clear();

            if (character == null || characterCurrencies == null || characterCurrencies.Count == 0)
            {
                CacheList.HideAll();
                return;
            }

            UICharacterCurrency tempUiCharacterCurrency;
            CacheList.Generate(characterCurrencies, (index, characterCurrency, ui) =>
            {
                tempUiCharacterCurrency = ui.GetComponent<UICharacterCurrency>();
                tempUiCharacterCurrency.Setup(new UICharacterCurrencyData(characterCurrency, characterCurrency.amount), Character, index);
                tempUiCharacterCurrency.Show();
                CacheSelectionManager.Add(tempUiCharacterCurrency);
                if (selectedId != 0 && selectedId == characterCurrency.dataId)
                    tempUiCharacterCurrency.OnClickSelect();
            });
        }

        protected virtual void Update()
        {
            if (CacheSelectionManager.selectionMode != dirtySelectionMode)
            {
                CacheSelectionManager.DeselectAll();
                dirtySelectionMode = CacheSelectionManager.selectionMode;
                if (uiCurrencyDialog != null)
                {
                    uiCurrencyDialog.onHide.RemoveListener(OnItemDialogHide);
                    uiCurrencyDialog.Hide();
                    uiCurrencyDialog.onHide.AddListener(OnItemDialogHide);
                }
            }
        }
    }
}
