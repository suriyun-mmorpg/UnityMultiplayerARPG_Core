using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UIWeaponSets : UIBase
    {
        public UIWeaponSet currentWeaponSet;
        public UIWeaponSet[] otherWeaponSets;

        private void OnEnable()
        {
            UpdateData(GameInstance.PlayingCharacter);
            ClientInventoryActions.onResponseSwitchEquipWeaponSet += ResponseSwitchEquipWeaponSet;
        }

        private void OnDisable()
        {
            ClientInventoryActions.onResponseSwitchEquipWeaponSet -= ResponseSwitchEquipWeaponSet;
        }

        public void ChangeWeaponSet(byte index)
        {
            GameInstance.ClientInventoryHandlers.RequestSwitchEquipWeaponSet(new RequestSwitchEquipWeaponSetMessage()
            {
                equipWeaponSet = index,
            }, ClientInventoryActions.ResponseSwitchEquipWeaponSet);
        }

        public void ResponseSwitchEquipWeaponSet(ResponseHandlerData requestHandler, AckResponseCode responseCode, ResponseSwitchEquipWeaponSetMessage response)
        {
            UpdateData(GameInstance.PlayingCharacter);
        }

        public void UpdateData(IPlayerCharacterData playerCharacter)
        {
            byte equipWeaponSet = playerCharacter.EquipWeaponSet;
            currentWeaponSet.SetData(this, equipWeaponSet, playerCharacter.SelectableWeaponSets[equipWeaponSet]);
            byte j = 0;
            for (byte i = 0; i < playerCharacter.SelectableWeaponSets.Count; ++i)
            {
                if (i != equipWeaponSet)
                    otherWeaponSets[j++].SetData(this, i, playerCharacter.SelectableWeaponSets[i]);
            }
        }
    }
}
