using UnityEngine;

namespace MultiplayerARPG
{
    public abstract partial class BaseMapInfo : BaseGameData
    {
        [Category("Map Info Settings")]
        [SerializeField]
        private UnityScene scene = default(UnityScene);
        public virtual UnityScene Scene { get { return scene; } }

        [Tooltip("This will be used when new character has been created, and this map data is the start map")]
        [SerializeField]
        private Vector3 startPosition = Vector3.zero;
        public virtual Vector3 StartPosition { get { return startPosition; } }

        [Tooltip("This will be used when new character has been created, and this map data is the start map")]
        [SerializeField]
        private Vector3 startRotation = Vector3.zero;
        public virtual Vector3 StartRotation { get { return startRotation; } }

        [Tooltip("When character fall to this position, character will dead")]
        [SerializeField]
        private float deadY = -100f;
        public virtual float DeadY { get { return deadY; } }

        [Tooltip("When character dead, it will drop equipping weapons or not?")]
        [SerializeField]
        private bool playerDeadDropsEquipWeapons = false;
        public virtual bool PlayerDeadDropsEquipWeapons { get { return playerDeadDropsEquipWeapons; } }

        [Tooltip("When character dead, it will drop equipping items or not?")]
        [SerializeField]
        private bool playerDeadDropsEquipItems = false;
        public virtual bool PlayerDeadDropsEquipItems { get { return playerDeadDropsEquipItems; } }

        [Tooltip("When character dead, it will drop non equipping items or not?")]
        [SerializeField]
        private bool playerDeadDropsNonEquipItems = false;
        public virtual bool PlayerDeadDropsNonEquipItems { get { return playerDeadDropsNonEquipItems; } }

        public virtual bool AutoRespawnWhenDead { get { return false; } }
        public virtual bool SaveCurrentMapPosition { get { return true; } }

        public virtual void GetRespawnPoint(IPlayerCharacterData playerCharacterData, out string mapName, out Vector3 position)
        {
            mapName = playerCharacterData.RespawnMapName;
            position = playerCharacterData.RespawnPosition;
        }

        public bool IsAlly(BaseCharacterEntity character, EntityInfo targetEntityInfo)
        {
            if (!string.IsNullOrEmpty(targetEntityInfo.Id) && targetEntityInfo.Id.Equals(character.Id))
                return true;
            if (character is BasePlayerCharacterEntity)
                return IsPlayerAlly(character as BasePlayerCharacterEntity, targetEntityInfo);
            if (character is BaseMonsterCharacterEntity)
                return IsMonsterAlly(character as BaseMonsterCharacterEntity, targetEntityInfo);
            return false;
        }

        public bool IsEnemy(BaseCharacterEntity character, EntityInfo targetEntityInfo)
        {
            if (!string.IsNullOrEmpty(targetEntityInfo.Id) && targetEntityInfo.Id.Equals(character.Id))
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
