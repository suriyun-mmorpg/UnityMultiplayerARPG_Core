using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    public class UISocialCharacterViewManager : MonoBehaviour
    {
        public static UISocialCharacterViewManager Singleton { get; private set; }

        [System.Serializable]
        public struct Entry
        {
            public Transform container;
            public RenderTexture renderTexture;
        }
        public Entry[] entries = new Entry[0];

        private void Awake()
        {
            Singleton = this;
        }

        public void LoadCharacter(int index, string characterId, out RenderTexture renderTexture, ResponseDelegate<ResponseGetOnlineCharacterDataMessage> callback)
        {
            renderTexture = null;
            if (index < 0 || index >= entries.Length)
                return;
            renderTexture = entries[index].renderTexture;
            ClearContainer(index);
            GameInstance.ClientOnlineCharacterHandlers.RequestGetOnlineCharacterData(new RequestGetOnlineCharacterDataMessage()
            {
                characterId = characterId,
            }, (requestHandler, responseCode, response) =>
            {
                if (callback != null)
                    callback.Invoke(requestHandler, responseCode, response);
                if (responseCode != AckResponseCode.Success)
                    return;
                BaseCharacterModel characterModel = response.character.InstantiateModel(entries[index].container);
                if (characterModel != null)
                {
                    characterModel.SetEquipItems(response.character.EquipItems, response.character.SelectableWeaponSets, response.character.EquipWeaponSet, false);
                    characterModel.gameObject.SetActive(true);
                }
            });
        }

        public void ClearContainer(int index)
        {
            if (index < 0 || index >= entries.Length)
                return;
            entries[index].container.RemoveChildren();
        }
    }
}
