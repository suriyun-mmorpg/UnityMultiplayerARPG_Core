using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IDamageInfo
    {
        /// <summary>
        /// Launch damage entity to attack enemy
        /// </summary>
        /// <param name="attacker">Who is attacking?</param>
        /// <param name="isLeftHand">Which hand?, Left-hand or not?</param>
        /// <param name="weapon">Which weapon?</param>
        /// <param name="simulateSeed">Launch random seed</param>
        /// <param name="triggerIndex"></param>
        /// <param name="spreadIndex"></param>
        /// <param name="fireStagger"></param>
        /// <param name="damageAmounts">Damage amounts</param>
        /// <param name="skill">Which skill?</param>
        /// <param name="skillLevel">Which skill level?</param>
        /// <param name="aimPosition">Aim position</param>
        /// <param name="onOriginPrepared">Action when origin prepared/param>
        /// <param name="onHit">Action when hit</param>
        void LaunchDamageEntity(
            BaseCharacterEntity attacker,
            bool isLeftHand,
            CharacterItem weapon,
            int simulateSeed,
            byte triggerIndex,
            byte spreadIndex,
            Vector3 fireStagger,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            AimPosition aimPosition,
            DamageOriginPreparedDelegate onOriginPrepared,
            DamageHitDelegate onHit);
        Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand);
        float GetDistance();
        float GetFov();
    }
}
