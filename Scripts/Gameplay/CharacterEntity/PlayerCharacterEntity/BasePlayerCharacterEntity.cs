using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(PlayerCharacterBuildingComponent))]
    [RequireComponent(typeof(PlayerCharacterCraftingComponent))]
    [RequireComponent(typeof(PlayerCharacterDealingComponent))]
    [RequireComponent(typeof(PlayerCharacterNpcActionComponent))]
    public abstract partial class BasePlayerCharacterEntity : BaseCharacterEntity, IPlayerCharacterData, IActivatableEntity
    {

        [Category("Character Settings")]
        [Tooltip("This is list which used as choice of character classes when create character")]
        [SerializeField]
        [FormerlySerializedAs("playerCharacters")]
        protected PlayerCharacter[] characterDatabases = new PlayerCharacter[0];
        public PlayerCharacter[] CharacterDatabases
        {
            get
            {
                if (TryGetMetaData(out PlayerCharacterEntityMetaData metaData))
                    return metaData.CharacterDatabases;
                return characterDatabases;
            }
            set { characterDatabases = value; }
        }

#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [Tooltip("Leave this empty to use GameInstance's controller prefab")]
        [SerializeField]
        protected BasePlayerCharacterController controllerPrefab;
#endif
        public BasePlayerCharacterController ControllerPrefab
        {
            get
            {
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
                if (TryGetMetaData(out PlayerCharacterEntityMetaData metaData))
                    return metaData.ControllerPrefab;
                return controllerPrefab;
#else
                return null;
#endif
            }
            set
            {
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
                controllerPrefab = value;
#endif
            }
        }

        [Tooltip("Leave this empty to use GameInstance's controller prefab")]
        [SerializeField]
        protected AssetReferenceBasePlayerCharacterController addressableControllerPrefab;
        public AssetReferenceBasePlayerCharacterController AddressableControllerPrefab
        {
            get
            {
                if (TryGetMetaData(out PlayerCharacterEntityMetaData metaData))
                    return metaData.AddressableControllerPrefab;
                return addressableControllerPrefab;
            }
            set { addressableControllerPrefab = value; }
        }

        public PlayerCharacterBuildingComponent Building
        {
            get; private set;
        }

        public PlayerCharacterCraftingComponent Crafting
        {
            get; private set;
        }

        public PlayerCharacterDealingComponent Dealing
        {
            get; private set;
        }

        public PlayerCharacterDuelingComponent Dueling
        {
            get; private set;
        }

        public PlayerCharacterVendingComponent Vending
        {
            get; private set;
        }

        public PlayerCharacterNpcActionComponent NpcAction
        {
            get; private set;
        }

        public PlayerCharacterPkComponent Pk
        {
            get; private set;
        }

        public override CharacterRace Race
        {
            get
            {
                if (TryGetMetaData(out PlayerCharacterEntityMetaData metaData))
                    return metaData.Race;
                return base.Race;
            }
            set { base.Race = value; }
        }

        public bool TryGetMetaData(out PlayerCharacterEntityMetaData metaData)
        {
            metaData = null;
            if (!MetaDataId.HasValue || !GameInstance.PlayerCharacterEntityMetaDataList.TryGetValue(MetaDataId.Value, out metaData))
                return false;
            return true;
        }

        public int IndexOfCharacterDatabase(int dataId)
        {
            for (int i = 0; i < CharacterDatabases.Length; ++i)
            {
                if (CharacterDatabases[i] != null && CharacterDatabases[i].DataId == dataId)
                    return i;
            }
            return -1;
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddCharacters(CharacterDatabases);
        }

        public override EntityInfo GetInfo()
        {
            return new EntityInfo(
                EntityTypes.Player,
                ObjectId,
                Id,
                DataId,
                FactionId,
                PartyId,
                GuildId,
                IsInSafeArea);
        }

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = CurrentGameInstance.playerTag;
            gameObject.layer = CurrentGameInstance.playerLayer;
        }

        public override void OnSetOwnerClient(bool isOwnerClient)
        {
            base.OnSetOwnerClient(isOwnerClient);
            gameObject.layer = isOwnerClient ? CurrentGameInstance.playingLayer : CurrentGameInstance.playerLayer;
        }

        public override void InitialRequiredComponents()
        {
            base.InitialRequiredComponents();
            Building = gameObject.GetOrAddComponent<PlayerCharacterBuildingComponent>();
            Crafting = gameObject.GetOrAddComponent<PlayerCharacterCraftingComponent>();
            Dealing = gameObject.GetOrAddComponent<PlayerCharacterDealingComponent>();
            Dueling = gameObject.GetOrAddComponent<PlayerCharacterDuelingComponent>();
            Vending = gameObject.GetOrAddComponent<PlayerCharacterVendingComponent>();
            NpcAction = gameObject.GetOrAddComponent<PlayerCharacterNpcActionComponent>();
            Pk = gameObject.GetOrAddComponent<PlayerCharacterPkComponent>();
            gameObject.GetOrAddComponent<PlayerCharacterItemLockAndExpireComponent>();
        }

        protected override void EntityUpdate()
        {
            base.EntityUpdate();
            Profiler.BeginSample("BasePlayerCharacterEntity - Update");
            if (this.IsDead())
            {
                StopMove();
                SetTargetEntity(null);
                return;
            }
            Profiler.EndSample();
        }

        public virtual float GetActivatableDistance()
        {
            return GameInstance.Singleton.conversationDistance;
        }

        public virtual bool ShouldClearTargetAfterActivated()
        {
            return false;
        }

        public virtual bool ShouldBeAttackTarget()
        {
            return !IsOwnerClient && !this.IsDeadOrHideFrom(GameInstance.PlayingCharacterEntity) && CanReceiveDamageFrom(GameInstance.PlayingCharacterEntity.GetInfo());
        }

        public virtual bool ShouldNotActivateAfterFollowed()
        {
            return true;
        }

        public virtual bool CanActivate()
        {
            return !IsOwnerClient;
        }

        public virtual void OnActivate()
        {
            BaseUISceneGameplay.Singleton.SetActivePlayerCharacter(this);
        }
    }
}
