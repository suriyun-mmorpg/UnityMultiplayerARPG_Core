using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.SIMPLE_AREA_BUFF_SKILL_FILE, menuName = GameDataMenuConsts.SIMPLE_AREA_BUFF_SKILL_MENU, order = GameDataMenuConsts.SIMPLE_AREA_BUFF_SKILL_ORDER)]
    public partial class SimpleAreaBuffSkill : BaseAreaSkill
    {
        [Category("Area Settings")]
        public AreaBuffEntity areaBuffEntity;

        [Category(3, "Buff")]
        public Buff buff;
        [Tooltip("If this is `TRUE` buffs will applies to everyone including with an enemies")]
        public bool applyBuffToEveryone;

        [Category(4, "Warp Settings")]
        public bool isWarpToAimPosition;

        protected override void ApplySkillImplement(
            BaseCharacterEntity skillUser,
            int skillLevel,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            uint targetObjectId,
            AimPosition aimPosition,
            DamageOriginPreparedDelegate onDamageOriginPrepared,
            DamageHitDelegate onDamageHit)
        {
            if (BaseGameNetworkManager.Singleton.IsServer)
            {
                // Spawn area entity
                // Aim position type always is `Position`
                LiteNetLibIdentity spawnObj = BaseGameNetworkManager.Singleton.Assets.GetObjectInstance(
                    areaBuffEntity.Identity.HashAssetId,
                    aimPosition.position,
                    GameInstance.Singleton.GameplayRule.GetSummonRotation(skillUser));
                AreaBuffEntity entity = spawnObj.GetComponent<AreaBuffEntity>();
                entity.Setup(skillUser.GetInfo(), this, skillLevel, applyBuffToEveryone, areaDuration.GetAmount(skillLevel), applyDuration.GetAmount(skillLevel));
                BaseGameNetworkManager.Singleton.Assets.NetworkSpawn(spawnObj);
            }
            // Teleport to aim position
            if (isWarpToAimPosition)
                skillUser.Teleport(aimPosition.position, skillUser.MovementTransform.rotation);
        }

        public override bool IsBuff
        {
            get { return true; }
        }

        public override Buff Buff
        {
            get { return buff; }
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            areaBuffEntity.InitPrefab();
            GameInstance.AddOtherNetworkObjects(areaBuffEntity.Identity);
        }
    }
}
