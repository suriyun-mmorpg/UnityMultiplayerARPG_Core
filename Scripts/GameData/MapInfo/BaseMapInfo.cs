using UnityEngine;
using UnityEngine.Serialization;

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

        [Category("Character Death Rules")]
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

        [SerializeField]
        private Sprite minimapSprite;
        public Sprite MinimapSprite { get { return minimapSprite; } set { minimapSprite = value; } }

        [SerializeField]
        private Vector3 minimapPosition;
        public Vector3 MinimapPosition { get { return minimapPosition; } set { minimapPosition = value; } }

        [SerializeField]
        [FormerlySerializedAs("minimapBoundsSizeX")]
        private float minimapBoundsWidth;
        public float MinimapBoundsWidth { get { return minimapBoundsWidth; } set { minimapBoundsWidth = value; } }

        [SerializeField]
        [FormerlySerializedAs("minimapBoundsSizeZ")]
        private float minimapBoundsLength;
        public float MinimapBoundsLength { get { return minimapBoundsLength; } set { minimapBoundsLength = value; } }

        [SerializeField]
        private float minimapOrthographicSize;
        public float MinimapOrthographicSize { get { return minimapOrthographicSize; } set { minimapOrthographicSize = value; } }

        public virtual bool AutoRespawnWhenDead { get { return false; } }
        public virtual bool SaveCurrentMapPosition { get { return true; } }

        public virtual void GetRespawnPoint(IPlayerCharacterData playerCharacterData, out WarpPortalType portalType, out string mapName, out Vector3 position, out bool overrideRotation, out Vector3 rotation)
        {
            portalType = WarpPortalType.Default;
            mapName = playerCharacterData.RespawnMapName;
            position = playerCharacterData.RespawnPosition;
            overrideRotation = false;
            rotation = Vector3.zero;
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
