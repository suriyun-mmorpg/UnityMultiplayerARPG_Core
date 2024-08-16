using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.SIMPLE_AREA_BUFF_SKILL_FILE, menuName = GameDataMenuConsts.SIMPLE_AREA_BUFF_SKILL_MENU, order = GameDataMenuConsts.SIMPLE_AREA_BUFF_SKILL_ORDER)]
    public partial class SimpleAreaBuffSkill : BaseAreaSkill
    {
        [Category("Area Settings")]
#if UNITY_EDITOR || !EXCLUDE_PREFAB_REFS
        [SerializeField]
        [AddressableAssetConversion(nameof(addressableAreaBuffEntity))]
        private AreaBuffEntity areaBuffEntity;
#endif
        public AreaBuffEntity AreaBuffEntity
        {
            get
            {
#if !EXCLUDE_PREFAB_REFS
                return areaBuffEntity;
#else
                return null;
#endif
            }
        }

        [SerializeField]
        private AssetReferenceAreaBuffEntity addressableAreaBuffEntity;
        public AssetReferenceAreaBuffEntity AddressableAreaBuffEntity
        {
            get { return addressableAreaBuffEntity; }
        }

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
            List<Dictionary<DamageElement, MinMaxFloat>> damageAmounts,
            uint targetObjectId,
            AimPosition aimPosition)
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
                skillUser.Teleport(aimPosition.position, skillUser.MovementTransform.rotation, false);
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            areaBuffEntity.InitPrefab();
            GameInstance.AddOtherNetworkObjects(areaBuffEntity.Identity);
        }

        public override bool TryGetBuff(out Buff buff)
        {
            buff = this.buff;
            return true;
        }
    }
}
