using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(LiteNetLibTransform))]
    public abstract partial class BasePlayerCharacterEntity : BaseCharacterEntity, IPlayerCharacterData
    {
        [Header("Movement AI")]
        [Range(0.01f, 1f)]
        public float stoppingDistance = 0.1f;

        [Header("Relates Components")]
        public BasePlayerCharacterController controllerPrefab;

        [HideInInspector]
        public WarpPortalEntity warpingPortal;
        [HideInInspector]
        public NpcDialog currentNpcDialog;

        public bool isJumping { get; protected set; }
        public bool isGrounded { get; protected set; }
        
        private LiteNetLibTransform cacheNetTransform;
        public LiteNetLibTransform CacheNetTransform
        {
            get
            {
                if (cacheNetTransform == null)
                    cacheNetTransform = GetComponent<LiteNetLibTransform>();
                return cacheNetTransform;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            gameObject.tag = gameInstance.playerTag;
        }

        protected override void Start()
        {
            base.Start();
            if (IsOwnerClient)
            {
                if (BasePlayerCharacterController.Singleton == null)
                {
                    var controller = Instantiate(controllerPrefab);
                    controller.PlayerCharacterEntity = this;
                }
            }
        }

        public override void Respawn()
        {
            if (!IsServer || !IsDead())
                return;
            base.Respawn();
            var manager = Manager as BaseGameNetworkManager;
            if (manager != null)
                manager.WarpCharacter(this, RespawnMapName, RespawnPosition);
        }

        public override bool CanReceiveDamageFrom(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            return characterEntity != null && characterEntity is BaseMonsterCharacterEntity;
        }

        public override bool IsAlly(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            return false;
        }

        public override bool IsEnemy(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            return characterEntity != null && characterEntity is BaseMonsterCharacterEntity;
        }

        public override void Killed(BaseCharacterEntity lastAttacker)
        {
            base.Killed(lastAttacker);
            currentNpcDialog = null;
        }

        public virtual void IncreaseGold(int gold)
        {
            if (!IsServer)
                return;
            Gold += gold;
        }

        public virtual void OnKillMonster(BaseMonsterCharacterEntity monsterCharacterEntity)
        {
            if (!IsServer || monsterCharacterEntity == null)
                return;

            for (var i = 0; i < Quests.Count; ++i)
            {
                var quest = Quests[i];
                if (quest.AddKillMonster(monsterCharacterEntity, 1))
                    quests[i] = quest;
            }
        }

        public abstract bool IsMoving();
        public abstract void StopMove();
        public abstract void KeyMovement(Vector3 direction, bool isJump);
        public abstract void PointClickMovement(Vector3 position);
    }
}