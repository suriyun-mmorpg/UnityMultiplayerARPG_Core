using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    // TODO: This is not finished yet
    public class AreaDamageType : BaseCustomActiveSkillType
    {
        public AreaDamageEntity damageEntity;

        public override void LaunchDamageEntity(BaseCharacterEntity baseCharacterEntity, bool isLeftHand, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterBuff debuff, BaseSkill skill, short skillLevel, Vector3 aimPosition, Vector3 stagger)
        {

        }

        public override Vector3? UpdateAimControls(BaseSkill causingSkill, short causingSkillLevel)
        {
            return null;
        }

        public override bool HasCustomAimControls()
        {
            return true;
        }
    }
}
