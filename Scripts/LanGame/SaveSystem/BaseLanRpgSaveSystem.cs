using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseLanRpgSaveSystem : ScriptableObject
    {
        public abstract void OnServerStart(BaseGameNetworkManager manager);
        public abstract void OnServerOnlineSceneLoaded(BaseGameNetworkManager manager, IPlayerCharacterData hostPlayerCharacterData, Dictionary<string, BuildingEntity> buildingEntities, Dictionary<StorageId, List<CharacterItem>> storageItems);
        public abstract void SaveWorld(BaseGameNetworkManager manager, IPlayerCharacterData hostPlayerCharacterData, Dictionary<string, BuildingEntity> buildingEntities);
        public abstract void SaveStorage(BaseGameNetworkManager manager, IPlayerCharacterData hostPlayerCharacterData, Dictionary<StorageId, List<CharacterItem>> storageItems);
        public abstract void SaveCharacter(BaseGameNetworkManager manager, IPlayerCharacterData playerCharacterData);
        public abstract void SaveCreatingCharacter(IPlayerCharacterData playerCharacterData);
        public abstract List<PlayerCharacterData> LoadCharacters();
    }
}
