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
        /// <param name="triggerIndex"></param>
        /// <param name="damageAmounts">Damage amounts</param>
        /// <param name="skill">Which skill?</param>
        /// <param name="skillLevel">Which skill level?</param>
        /// <param name="randomSeed">Launch random seed</param>
        /// <param name="aimPosition">Aim position</param>
        /// <param name="stagger">Stagger</param>
        /// <param name="onOriginPrepared">Action when origin prepared. TriggerIndex(int), Position(Vector3), Direction(Vector3), Rotation(Quaternion)</param>
        /// <param name="onHit">Action when hit. TriggerIndex(int), ObjectID(uint), HitboxIndex(int)</param>
        void LaunchDamageEntity(
            BaseCharacterEntity attacker,
            bool isLeftHand,
            CharacterItem weapon,
            int triggerIndex,
            Dictionary<DamageElement, MinMaxFloat> damageAmounts,
            BaseSkill skill,
            int skillLevel,
            int randomSeed,
            AimPosition aimPosition,
            Vector3 stagger,
            System.Action<int, Vector3, Vector3, Quaternion> onOriginPrepared,
            System.Action<int, uint, int> onHit);
        Transform GetDamageTransform(BaseCharacterEntity attacker, bool isLeftHand);
        float GetDistance();
        float GetFov();
    }
}
