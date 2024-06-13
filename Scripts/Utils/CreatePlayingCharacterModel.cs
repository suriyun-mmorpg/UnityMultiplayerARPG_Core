using UnityEngine;

namespace MultiplayerARPG
{
    public class CreatePlayingCharacterModel : MonoBehaviour
    {
        public Transform container;

        private void OnEnable()
        {
            GameInstance.OnSetPlayingCharacterEvent += GameInstance_onSetPlayingCharacter;
            GameInstance_onSetPlayingCharacter(GameInstance.PlayingCharacter);
        }

        private void OnDisable()
        {
            GameInstance.OnSetPlayingCharacterEvent -= GameInstance_onSetPlayingCharacter;
            container.RemoveChildren();
        }

        private void GameInstance_onSetPlayingCharacter(IPlayerCharacterData playerCharacter)
        {
            if (playerCharacter == null)
                return;
            container.RemoveChildren();
            BaseCharacterModel characterModel = playerCharacter.InstantiateModel(container);
            characterModel.SetupModelBodyParts(playerCharacter);
            characterModel.SetEquipItemsImmediately(playerCharacter.EquipItems, playerCharacter.SelectableWeaponSets, playerCharacter.EquipWeaponSet, false);
        }
    }
}