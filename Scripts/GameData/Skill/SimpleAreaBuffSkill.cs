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
            // TODO: Spawn area entity
        }

        public override float GetAttackDistance(BaseCharacterEntity skillUser, bool isLeftHand, short skillLevel)
        {
            return castRadius;
        }

        public override float GetAttackFov(BaseCharacterEntity skillUser, bool isLeftHand, short skillLevel)
        {
            return 360f;
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackDamages(ICharacterData skillUser, bool isLeftHand, short skillLevel)
        {
            return new Dictionary<DamageElement, MinMaxFloat>();
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, bool isLeftHand, short skillLevel)
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
