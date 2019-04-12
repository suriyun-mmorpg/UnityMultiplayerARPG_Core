using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class ButtonsActivatorForUICharacterItem : MonoBehaviour
    {
        public Button buttonEquip;
        public Button buttonUnEquip;
        public Button buttonRefine;
        public Button buttonSocketEnhance;
        public Button buttonSell;
        public Button buttonOffer;
        public Button buttonMoveToStorage;
        public Button buttonMoveFromStorage;
        public Button buttonDrop;

        private void Start()
        {
            UICharacterItem ui = GetComponent<UICharacterItem>();
            ui.onSetEquippedData.AddListener(OnSetEquippedData);
            ui.onSetUnEquippedData.AddListener(OnSetUnEquippedData);
            ui.onSetUnEquippableData.AddListener(OnSetUnEquippableData);
            ui.onSetStorageItemData.AddListener(OnSetStorageItemData);
            ui.onNpcSellItemDialogAppear.AddListener(OnNpcSellItemDialogAppear);
            ui.onNpcSellItemDialogDisappear.AddListener(OnNpcSellItemDialogDisappear);
            ui.onStorageDialogAppear.AddListener(OnStorageDialogAppear);
            ui.onStorageDialogDisappear.AddListener(OnStorageDialogDisappear);
            ui.onEnterDealingState.AddListener(OnEnterDealingState);
            ui.onExitDealingState.AddListener(OnExitDealingState);
            // Refresh UI data to applies events
            ui.ForceUpdate();
        }

        public void DeactivateAllButtons()
        {
            buttonEquip.gameObject.SetActive(false);
            buttonUnEquip.gameObject.SetActive(false);
            buttonRefine.gameObject.SetActive(false);
            buttonSocketEnhance.gameObject.SetActive(false);
            buttonSell.gameObject.SetActive(false);
            buttonOffer.gameObject.SetActive(false);
            buttonMoveToStorage.gameObject.SetActive(false);
            buttonMoveFromStorage.gameObject.SetActive(false);
            buttonDrop.gameObject.SetActive(false);
        }

        public void OnSetEquippedData()
        {
            DeactivateAllButtons();
            buttonUnEquip.gameObject.SetActive(true);
            buttonRefine.gameObject.SetActive(true);
            buttonSocketEnhance.gameObject.SetActive(true);
        }

        public void OnSetUnEquippedData()
        {
            DeactivateAllButtons();
            buttonEquip.gameObject.SetActive(true);
            buttonRefine.gameObject.SetActive(true);
            buttonSocketEnhance.gameObject.SetActive(true);
            buttonDrop.gameObject.SetActive(true);
        }

        public void OnSetUnEquippableData()
        {
            DeactivateAllButtons();
            buttonDrop.gameObject.SetActive(true);
        }

        public void OnSetStorageItemData()
        {
            DeactivateAllButtons();
            buttonMoveFromStorage.gameObject.SetActive(true);
        }

        public void OnNpcSellItemDialogAppear()
        {
            buttonSell.gameObject.SetActive(true);
        }

        public void OnNpcSellItemDialogDisappear()
        {
            buttonSell.gameObject.SetActive(false);
        }

        public void OnStorageDialogAppear()
        {
            buttonMoveToStorage.gameObject.SetActive(true);
        }

        public void OnStorageDialogDisappear()
        {
            buttonMoveToStorage.gameObject.SetActive(false);
        }

        public void OnEnterDealingState()
        {
            buttonOffer.gameObject.SetActive(true);
        }

        public void OnExitDealingState()
        {
            buttonOffer.gameObject.SetActive(false);
        }
    }
}
