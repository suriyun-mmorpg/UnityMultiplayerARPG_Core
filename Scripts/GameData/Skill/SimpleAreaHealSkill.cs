using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Skill", menuName = "Create GameData/Skill/Simple Area Heal Skill", order = -4987)]
    public class SimpleAreaHealSkill : BaseSkill
    {
        public float castRadius;
        public GameEffectCollection hitEffects;
        public GameObject targetObjectPrefab;
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

        public override void ApplySkill(BaseCharacterEntity skillUser, short skillLevel, bool isLeftHand, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, Vector3 aimPosition)
        {
            // TODO: Spawn area entity
        }

        public override Dictionary<DamageElement, MinMaxFloat> GetAttackAdditionalDamageAmounts(ICharacterData skillUser, short skillLevel)
        {
            return new Dictionary<DamageElement, MinMaxFloat>();
        }

        public override void GetAttackDamages(ICharacterData skillUser, bool isLeftHand, short skillLevel, out Dictionary<DamageElement, MinMaxFloat> damageAmounts)
        {
            damageAmounts = new Dictionary<DamageElement, MinMaxFloat>();
        }

        public override float GetAttackDistance(BaseCharacterEntity skillUser, bool isLeftHand, short skillLevel)
        {
            return castRadius;
        }

        public override float GetAttackFov(BaseCharacterEntity skillUser, bool isLeftHand, short skillLevel)
        {
            return 360f;
        }

        public override Dictionary<DamageElement, float> GetAttackWeaponDamageInflictions(ICharacterData skillUser, short skillLevel)
        {
            return new Dictionary<DamageElement, float>();
        }

        public override KeyValuePair<DamageElement, MinMaxFloat> GetBaseAttackDamageAmount(ICharacterData skillUser, bool isLeftHand, short skillLevel)
        {
            return new KeyValuePair<DamageElement, MinMaxFloat>();
        }

        public override Buff GetBuff()
        {
            return default(Buff);
        }

        public override Buff GetDebuff()
        {
            return default(Buff);
        }

        public override GameEffectCollection GetHitEffect()
        {
            return hitEffects;
        }

        public override ItemCraft GetItemCraft()
        {
            return default(ItemCraft);
        }

        public override MountEntity GetMountEntity()
        {
            return null;
        }

        public override SkillType GetSkillType()
        {
            return SkillType.Active;
        }

        public override BaseMonsterCharacterEntity GetSummonMonsterEntity()
        {
            return null;
        }

        public override bool HasCustomAimControls()
        {
            return true;
        }

        public override bool IsAttack()
        {
            return true;
        }

        public override bool IsBuff()
        {
            return false;
        }

        public override bool IsDebuff()
        {
            return false;
        }

        public override Vector3? UpdateAimControls(short skillLevel)
        {
            throw new System.NotImplementedException();
        }
    }
}
