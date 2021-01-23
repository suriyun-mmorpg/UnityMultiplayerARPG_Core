using UnityEngine;
using System.Collections;

namespace MultiplayerARPG
{
    public abstract partial class BaseMapInfo : BaseGameData
    {
        [Header("Map Info Configs")]
        [SerializeField]
        private UnityScene scene;
        [Tooltip("This will be used when new character have been created, and this map data is start map")]
        [SerializeField]
        private Vector3 startPosition;
        [Tooltip("When character fall to this position, character will dead")]
        [SerializeField]
        private float deadY = -100f;

        public virtual UnityScene Scene { get { return scene; } }
        public virtual Vector3 StartPosition { get { return startPosition; } }
        public virtual float DeadY { get { return deadY; } }

        public virtual bool AutoRespawnWhenDead { get { return false; } }
        public virtual bool SaveCurrentMapPosition { get { return true; } }

        public virtual void GetRespawnPoint(IPlayerCharacterData playerCharacterData, out string mapName, out Vector3 position)
        {
            mapName = playerCharacterData.RespawnMapName;
            position = playerCharacterData.RespawnPosition;
        }

        public bool IsAlly(BaseCharacterEntity character, EntityInfo targetEntityInfo)
        {
            if (!string.IsNullOrEmpty(targetEntityInfo.id) && targetEntityInfo.id.Equals(character.Id))
                return true;
            if (character is BasePlayerCharacterEntity)
                return IsPlayerAlly(character as BasePlayerCharacterEntity, targetEntityInfo);
            if (character is BaseMonsterCharacterEntity)
                return IsMonsterAlly(character as BaseMonsterCharacterEntity, targetEntityInfo);
            return false;
        }

        public bool IsEnemy(BaseCharacterEntity character, EntityInfo targetEntityInfo)
        {
            if (!string.IsNullOrEmpty(targetEntityInfo.id) && targetEntityInfo.id.Equals(character.Id))
                return false;
            if (character is BasePlayerCharacterEntity)
                return IsPlayerEnemy(character as BasePlayerCharacterEntity, targetEntityInfo);
            if (character is BaseMonsterCharacterEntity)
                return IsMonsterEnemy(character as BaseMonsterCharacterEntity, targetEntityInfo);
            return false;
        }

        protected abstract bool IsPlayerAlly(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntityInfo);
        protected abstract bool IsMonsterAlly(BaseMonsterCharacterEntity monsterCharacter, EntityInfo targetEntityInfo);
        protected abstract bool IsPlayerEnemy(BasePlayerCharacterEntity playerCharacter, EntityInfo targetEntityInfo);
        protected abstract bool IsMonsterEnemy(BaseMonsterCharacterEntity monsterCharacter, EntityInfo targetEntityInfo);
    }
}
