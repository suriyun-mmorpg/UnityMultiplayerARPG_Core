using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG
{
    public enum DealingState : byte
    {
        None,
        Dealing,
        Lock,
        Confirm,
    }

    [RequireComponent(typeof(LiteNetLibTransform))]
    public abstract partial class BasePlayerCharacterEntity : BaseCharacterEntity, IPlayerCharacterData
    {
        [Header("Relates Components")]
        public BasePlayerCharacterController controllerPrefab;

        [HideInInspector]
        public WarpPortalEntity warpingPortal;
        [HideInInspector]
        public NpcDialog currentNpcDialog;
        [HideInInspector]
        public BasePlayerCharacterEntity coPlayerCharacterEntity;

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

        protected override void EntityAwake()
        {
            base.EntityAwake();
            gameObject.tag = gameInstance.playerTag;
        }

        protected override void ApplySkill(CharacterSkill characterSkill, Vector3 position, bool isAttack, CharacterItem weapon, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts)
        {
            base.ApplySkill(characterSkill, position, isAttack, weapon, damageInfo, allDamageAmounts);

            var skill = characterSkill.GetSkill();
            switch (skill.skillType)
            {
                case SkillType.CraftItem:
                    if (!skill.itemCraft.CanCraft(this))
                    {
                        // TODO: may warn that cannot craft
                    }
                    else
                        skill.itemCraft.CraftItem(this);
                    break;
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
            if (characterEntity == null)
                return false;
            var manager = Manager as BaseGameNetworkManager;
            if (manager == null)
                return false;
            if (characterEntity is BaseMonsterCharacterEntity)
                return true;
            if (!IsAlly(characterEntity) && manager.CurrentMapInfo.canPvp)
                return true;
            return false;
        }

        public override bool IsAlly(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            return false;
        }

        public override bool IsEnemy(BaseCharacterEntity characterEntity)
        {
            // TODO: May implement this for party/guild battle purposes
            if (characterEntity == null)
                return false;
            if (characterEntity is BaseMonsterCharacterEntity)
                return true;
            if (!IsAlly(characterEntity))
                return true;
            return false;
        }

        public override void Killed(BaseCharacterEntity lastAttacker)
        {
            base.Killed(lastAttacker);
            currentNpcDialog = null;
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

        public virtual void ExchangeDealingItemsAndGold()
        {
            if (coPlayerCharacterEntity == null)
                return;
            var tempDealingItems = new List<DealingCharacterItem>(DealingItems);
            for (var i = nonEquipItems.Count - 1; i >= 0; --i)
            {
                var nonEquipItem = nonEquipItems[i];
                for (var j = tempDealingItems.Count - 1; j >= 0; --j)
                {
                    var dealingItem = tempDealingItems[j];
                    if (dealingItem.nonEquipIndex == i && nonEquipItem.amount >= dealingItem.amount)
                    {
                        nonEquipItem.amount -= dealingItem.amount;
                        if (nonEquipItem.amount == 0)
                            nonEquipItems.RemoveAt(i);
                        else
                            nonEquipItems[i] = nonEquipItem;
                        coPlayerCharacterEntity.IncreaseItems(dealingItem.dataId, dealingItem.level, dealingItem.amount, dealingItem.durability);
                        tempDealingItems.RemoveAt(j);
                        break;
                    }
                }
            }
            Gold -= DealingGold;
            coPlayerCharacterEntity.Gold += DealingGold;
        }

        public virtual void ClearDealingData()
        {
            DealingState = DealingState.None;
            DealingGold = 0;
            DealingItems.Clear();
        }

        public override bool CanMoveOrDoActions()
        {
            return base.CanMoveOrDoActions() && DealingState == DealingState.None;
        }

        public abstract float StoppingDistance { get; }
        public abstract bool IsMoving();
        public abstract void StopMove();
        public abstract void KeyMovement(Vector3 direction, bool isJump);
        public abstract void PointClickMovement(Vector3 position);
    }
}