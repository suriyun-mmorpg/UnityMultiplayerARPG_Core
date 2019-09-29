using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class AreaDamageType : BaseCustomDamageType
    {
        public AreaDamageEntity damageEntity;

        public override float GetDistance()
        {
            return 0f;
        }

        public override float GetFov()
        {
            return 0f;
        }

        public override void LaunchDamageEntity(BaseCharacterEntity baseCharacterEntity, bool isLeftHand, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterBuff debuff, Skill skill, short skillLevel, Vector3 aimPosition, Vector3 stagger)
        {

        }

        public override Vector3? UpdateAimControls(Skill causingSkill, short causingSkillLevel)
        {
            return null;
        }

        public override bool HasCustomAimControls()
        {
            return true;
        }
    }
}
