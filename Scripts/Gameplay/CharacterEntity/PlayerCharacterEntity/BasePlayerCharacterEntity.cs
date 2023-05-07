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
        protected PlayerCharacter[] characterDatabases;
        [Tooltip("Leave this empty to use GameInstance's controller prefab")]
        [SerializeField]
        protected BasePlayerCharacterController controllerPrefab;

        public PlayerCharacter[] CharacterDatabases
        {
            get { return characterDatabases; }
            set { characterDatabases = value; }
        }

        public BasePlayerCharacterController ControllerPrefab
        {
            get { return controllerPrefab; }
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

        public PlayerCharacterNpcActionComponent NpcAction
        {
            get; private set;
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
            NpcAction = gameObject.GetOrAddComponent<PlayerCharacterNpcActionComponent>();
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
            return !IsOwnerClient && !this.IsHideOrDead() && CanReceiveDamageFrom(GameInstance.PlayingCharacterEntity.GetInfo());
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
