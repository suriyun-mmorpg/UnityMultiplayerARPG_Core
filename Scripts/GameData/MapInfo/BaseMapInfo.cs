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

        public bool IsAlly(BaseCharacterEntity characterA, BaseCharacterEntity characterB)
        {
            if (characterA == characterB)
                return true;
            if (characterA is BasePlayerCharacterEntity)
                return IsPlayerAlly(characterA as BasePlayerCharacterEntity, characterB);
            if (characterA is BaseMonsterCharacterEntity)
                return IsMonsterAlly(characterA as BaseMonsterCharacterEntity, characterB);
            return false;
        }

        public bool IsEnemy(BaseCharacterEntity characterA, BaseCharacterEntity characterB)
        {
            if (characterA == characterB)
                return false;
            if (characterA is BasePlayerCharacterEntity)
                return IsPlayerEnemy(characterA as BasePlayerCharacterEntity, characterB);
            if (characterA is BaseMonsterCharacterEntity)
                return IsMonsterEnemy(characterA as BaseMonsterCharacterEntity, characterB);
            return false;
        }

        protected abstract bool IsPlayerAlly(BasePlayerCharacterEntity playerCharacter, BaseCharacterEntity targetCharacter);
        protected abstract bool IsMonsterAlly(BaseMonsterCharacterEntity monsterCharacter, BaseCharacterEntity targetCharacter);
        protected abstract bool IsPlayerEnemy(BasePlayerCharacterEntity playerCharacter, BaseCharacterEntity targetCharacter);
        protected abstract bool IsMonsterEnemy(BaseMonsterCharacterEntity monsterCharacter, BaseCharacterEntity targetCharacter);
    }
}
