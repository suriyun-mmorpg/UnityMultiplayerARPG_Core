using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public abstract class BaseLanRpgSaveSystem : ScriptableObject
    {
        public abstract void OnServerStart();
        public abstract void OnServerOnlineSceneLoaded(IPlayerCharacterData hostPlayerCharacterData, Dictionary<string, BuildingEntity> buildingEntities, Dictionary<StorageId, List<CharacterItem>> storageItems);
        public abstract void SaveWorld(IPlayerCharacterData hostPlayerCharacterData, Dictionary<string, BuildingEntity> buildingEntities);
        public abstract void SaveStorage(IPlayerCharacterData hostPlayerCharacterData, Dictionary<StorageId, List<CharacterItem>> storageItems);
        public abstract void SaveCharacter(IPlayerCharacterData playerCharacterData);
        public abstract List<PlayerCharacterData> LoadCharacters();
    }
}
