using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill/Simple Area Buff Skill", order = -4987)]
    public class SimpleAreaHealSkill : BaseSkill
    {
        public float castRadius;
        public IncrementalFloat areaDuration;
        public IncrementalFloat applyDuration;
        public AreaBuffEntity areaBuffEntity;
        public GameObject targetObjectPrefab;
        public Buff buff;

        private GameObject cacheTargetObject;
        public GameObject CacheTargetObject
        {
            get
            {
                if (cacheTargetObject == null)
                {
                    cacheTargetObject = Instantiate(targetObjectPrefab);
                    cacheTargetObject.SetActive(false);
                }
                return cacheTargetObject;
            }
        }

        public override SkillType GetSkillType()
        {
            return SkillType.Active;
        }

        public override GameEffectCollection GetHitEffect()
        {
            return default(GameEffectCollection);
        }

        public override void ApplySkill(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, Vector3 aimPosition)
        {
            // Spawn area entity
            // TODO: validate aim position
            AreaBuffEntity buffEntity = Instantiate(areaBuffEntity, aimPosition, skillUser.GetSummonRotation());
            buffEntity.Setup(skillUser, CharacterBuff.Create(BuffType.SkillBuff, DataId, skillLevel), this, skillLevel, areaDuration.GetAmount(skillLevel), applyDuration.GetAmount(skillLevel));
        }

        public override float GetAttackDistance(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand)
        {
            return castRadius;
        }

        public override float GetAttackFov(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand)
        {
            return 360f;
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackDamages(ICharacterData skillUser, short skillLevel, bool isLeftHand)
        {
            return new Dictionary<DamageElement, MinMaxFloat>();
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, short skillLevel, bool isLeftHand)
        {
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, short skillLevel)
        {
            return new Dictionary<DamageElement, MinMaxFloat>();
        }

        public override Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, short skillLevel)
        {
            return new Dictionary<DamageElement, float>();
        }

        public override BaseMonsterCharacterEntity GetSummonMonsterEntity()
        {
            return null;
        }

        public override MountEntity GetMountEntity()
        {
            return null;
        }

        public override ItemCraft GetItemCraft()
        {
            return default(ItemCraft);
        }

        public override bool IsAttack()
        {
            return false;
        }

        public override bool IsBuff()
        {
            return true;
        }

        public override Buff GetBuff()
        {
            return buff;
        }

        public override bool IsDebuff()
        {
            return false;
        }

        public override Buff GetDebuff()
        {
            return default(Buff);
        }

        public override bool HasCustomAimControls()
        {
            return true;
        }

        public override Vector3? UpdateAimControls(short skillLevel)
        {
            throw new System.NotImplementedException();
        }
    }
}
