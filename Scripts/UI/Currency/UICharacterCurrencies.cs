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
            CacheSelectionManager.eventOnSelected.RemoveListener(OnSelect);
            CacheSelectionManager.eventOnSelected.AddListener(OnSelect);
            CacheSelectionManager.eventOnDeselected.RemoveListener(OnDeselect);
            CacheSelectionManager.eventOnDeselected.AddListener(OnDeselect);
            if (uiCurrencyDialog != null)
                uiCurrencyDialog.onHide.AddListener(OnDialogHide);
        }

        protected virtual void OnDisable()
        {
            if (uiCurrencyDialog != null)
                uiCurrencyDialog.onHide.RemoveListener(OnDialogHide);
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnDialogHide()
        {
            CacheSelectionManager.DeselectSelectedUI();
        }

        protected virtual void OnSelect(UICharacterCurrency ui)
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

        protected virtual void OnDeselect(UICharacterCurrency ui)
        {
            if (uiCurrencyDialog != null && CacheSelectionManager.selectionMode == UISelectionMode.SelectSingle)
            {
                uiCurrencyDialog.onHide.RemoveListener(OnDialogHide);
                uiCurrencyDialog.Hide();
                uiCurrencyDialog.onHide.AddListener(OnDialogHide);
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
                    uiCurrencyDialog.onHide.RemoveListener(OnDialogHide);
                    uiCurrencyDialog.Hide();
                    uiCurrencyDialog.onHide.AddListener(OnDialogHide);
                }
            }
        }
    }
}
