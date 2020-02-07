using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public class ButtonsActivatorForUICharacterItem : MonoBehaviour
    {
        public bool canRefineItemByOwnerCharacter;
        public bool canDismantleItemByOwnerCharacter;
        public Button buttonEquip;
        public Button buttonUnEquip;
        public Button buttonRefine;
        public Button buttonDismantle;
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
            ui.onRefineItemDialogAppear.AddListener(OnRefineItemDialogAppear);
            ui.onRefineItemDialogDisappear.AddListener(OnRefineItemDialogDisappear);
            ui.onDismantleItemDialogAppear.AddListener(OnDismantleItemDialogAppear);
            ui.onDismantleItemDialogDisappear.AddListener(OnDismantleItemDialogDisappear);
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
            if (buttonEquip)
                buttonEquip.gameObject.SetActive(false);
            if (buttonUnEquip)
                buttonUnEquip.gameObject.SetActive(false);
            if (buttonRefine)
                buttonRefine.gameObject.SetActive(false);
            if (buttonDismantle)
                buttonDismantle.gameObject.SetActive(false);
            if (buttonSocketEnhance)
                buttonSocketEnhance.gameObject.SetActive(false);
            if (buttonSell)
                buttonSell.gameObject.SetActive(false);
            if (buttonOffer)
                buttonOffer.gameObject.SetActive(false);
            if (buttonMoveToStorage)
                buttonMoveToStorage.gameObject.SetActive(false);
            if (buttonMoveFromStorage)
                buttonMoveFromStorage.gameObject.SetActive(false);
            if (buttonDrop)
                buttonDrop.gameObject.SetActive(false);
        }

        public void OnSetEquippedData()
        {
            DeactivateAllButtons();
            if (buttonUnEquip)
                buttonUnEquip.gameObject.SetActive(true);
            if (buttonRefine)
                buttonRefine.gameObject.SetActive(canRefineItemByOwnerCharacter);
            if (buttonSocketEnhance)
                buttonSocketEnhance.gameObject.SetActive(true);
        }

        public void OnSetUnEquippedData()
        {
            DeactivateAllButtons();
            if (buttonEquip)
                buttonEquip.gameObject.SetActive(true);
            if (buttonRefine)
                buttonRefine.gameObject.SetActive(canRefineItemByOwnerCharacter);
            if (buttonDismantle)
                buttonDismantle.gameObject.SetActive(canDismantleItemByOwnerCharacter);
            if (buttonSocketEnhance)
                buttonSocketEnhance.gameObject.SetActive(true);
            if (buttonDrop)
                buttonDrop.gameObject.SetActive(true);
        }

        public void OnSetUnEquippableData()
        {
            DeactivateAllButtons();
            if (buttonDrop)
                buttonDrop.gameObject.SetActive(true);
        }

        public void OnSetStorageItemData()
        {
            DeactivateAllButtons();
            if (buttonMoveFromStorage)
                buttonMoveFromStorage.gameObject.SetActive(true);
        }

        public void OnRefineItemDialogAppear()
        {
            if (canRefineItemByOwnerCharacter)
                return;

            DeactivateAllButtons();
            if (buttonRefine)
                buttonRefine.gameObject.SetActive(true);
        }

        public void OnRefineItemDialogDisappear()
        {
            if (canRefineItemByOwnerCharacter)
                return;

            DeactivateAllButtons();
            if (buttonRefine)
                buttonRefine.gameObject.SetActive(false);
        }

        public void OnDismantleItemDialogAppear()
        {
            if (canDismantleItemByOwnerCharacter)
                return;

            DeactivateAllButtons();
            if (buttonDismantle)
                buttonDismantle.gameObject.SetActive(true);
        }

        public void OnDismantleItemDialogDisappear()
        {
            if (canDismantleItemByOwnerCharacter)
                return;

            DeactivateAllButtons();
            if (buttonDismantle)
                buttonDismantle.gameObject.SetActive(false);
        }

        public void OnNpcSellItemDialogAppear()
        {
            if (buttonSell)
                buttonSell.gameObject.SetActive(true);
        }

        public void OnNpcSellItemDialogDisappear()
        {
            if (buttonSell)
                buttonSell.gameObject.SetActive(false);
        }

        public void OnStorageDialogAppear()
        {
            if (buttonMoveToStorage)
                buttonMoveToStorage.gameObject.SetActive(true);
        }

        public void OnStorageDialogDisappear()
        {
            if (buttonMoveToStorage)
                buttonMoveToStorage.gameObject.SetActive(false);
        }

        public void OnEnterDealingState()
        {
            if (buttonOffer)
                buttonOffer.gameObject.SetActive(true);
        }

        public void OnExitDealingState()
        {
            if (buttonOffer)
                buttonOffer.gameObject.SetActive(false);
        }
    }
}
