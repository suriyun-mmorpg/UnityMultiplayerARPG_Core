using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class BasePlayerCharacterEntity
    {
        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="aimPosition"></param>
        [ServerRpc]
        protected void ServerUseItem(short itemIndex)
        {
#if !CLIENT_BUILD
            if (!CanUseItem())
                return;

            if (itemIndex >= nonEquipItems.Count)
                return;

            CharacterItem characterItem = nonEquipItems[itemIndex];
            if (characterItem.IsLock())
                return;

            IUsableItem usableItem = characterItem.GetUsableItem();
            if (usableItem == null)
                return;

            usableItem.UseItem(this, itemIndex, characterItem);
#endif
        }

        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        [ServerRpc]
        protected void ServerUseSkillItem(short itemIndex, bool isLeftHand)
        {
#if !CLIENT_BUILD
            UseItemSkill(itemIndex, isLeftHand, null);
#endif
        }

        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="aimPosition"></param>
        [ServerRpc]
        protected void ServerUseSkillItemWithAimPosition(short itemIndex, bool isLeftHand, Vector3 aimPosition)
        {
#if !CLIENT_BUILD
            UseItemSkill(itemIndex, isLeftHand, aimPosition);
#endif
        }

        protected void UseItemSkill(short itemIndex, bool isLeftHand, Vector3? aimPosition)
        {
            GameMessage.Type gameMessageType;
            if (!CanUseItem() || !CanUseSkill())
                return;

            if (itemIndex >= nonEquipItems.Count)
                return;

            CharacterItem characterItem = nonEquipItems[itemIndex];
            if (characterItem.IsLock())
                return;

            ISkillItem item = characterItem.GetSkillItem();
            if (!CanUseItem() || !CanUseSkill() || item == null || item.UsingSkill == null)
                return;

            // Validate mp amount, skill level
            if (!item.UsingSkill.CanUse(this, item.UsingSkillLevel, isLeftHand, out gameMessageType, true))
                return;

            // Prepare requires data and get skill data
            AnimActionType animActionType;
            int animActionDataId;
            CharacterItem weapon;
            GetUsingSkillData(
                item.UsingSkill,
                ref isLeftHand,
                out animActionType,
                out animActionDataId,
                out weapon);

            // Validate ammo
            if (item.UsingSkill.IsAttack() && !ValidateAmmo(weapon))
                return;

            // Prepare requires data and get animation data
            int animationIndex;
            float animSpeedRate;
            float[] triggerDurations;
            float totalDuration;
            GetRandomAnimationData(
                animActionType,
                animActionDataId,
                out animationIndex,
                out animSpeedRate,
                out triggerDurations,
                out totalDuration);

            // Validate skill item
            if (!this.DecreaseItemsByIndex(itemIndex, 1))
                return;
            this.FillEmptySlots();

            // Start use skill routine
            IsAttackingOrUsingSkill = true;

            // Play animations
            if (!aimPosition.HasValue)
                RequestPlaySkillAnimation(isLeftHand, (byte)animationIndex, item.UsingSkill.DataId, item.UsingSkillLevel);
            else
                RequestPlaySkillAnimationWithAimPosition(isLeftHand, (byte)animationIndex, item.UsingSkill.DataId, item.UsingSkillLevel, aimPosition.Value);
        }
    }
}
