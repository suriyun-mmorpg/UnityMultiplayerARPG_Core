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
        protected void NetFuncUseItem(short itemIndex)
        {
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
        }

        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        protected void NetFuncUseSkillItem(short itemIndex, bool isLeftHand)
        {
            UseItemSkill(itemIndex, isLeftHand, null);
        }

        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="aimPosition"></param>
        protected void NetFuncUseSkillItemWithAimPosition(short itemIndex, bool isLeftHand, Vector3 aimPosition)
        {
            UseItemSkill(itemIndex, isLeftHand, aimPosition);
        }

        protected void UseItemSkill(short itemIndex, bool isLeftHand, Vector3? aimPosition)
        {
            if (!CanUseItem() || !CanUseSkill())
                return;

            if (itemIndex >= nonEquipItems.Count)
                return;

            CharacterItem characterItem = nonEquipItems[itemIndex];
            if (characterItem.IsLock())
                return;

            ISkillItem item = characterItem.GetSkillItem();
            if (!CanUseItem() || !CanUseSkill() || item == null || item.UsingSkill == null || !this.DecreaseItemsByIndex(itemIndex, 1))
                return;

            // Validate mp amount, skill level, 
            GameMessage.Type gameMessageType;
            if (!item.UsingSkill.CanUse(this, item.UsingSkillLevel, isLeftHand, out gameMessageType, true))
                return;

            // Prepare requires data and get skill data
            AnimActionType animActionType;
            int animatonDataId;
            CharacterItem weapon;
            GetUsingSkillData(
                item.UsingSkill,
                ref isLeftHand,
                out animActionType,
                out animatonDataId,
                out weapon);

            // Validate ammo
            if (item.UsingSkill.IsAttack() && !ValidateAmmo(weapon))
                return;

            // Prepare requires data and get animation data
            int animationIndex;
            float[] triggerDurations;
            float totalDuration;
            GetRandomAnimationData(
                animActionType,
                animatonDataId,
                out animationIndex,
                out triggerDurations,
                out totalDuration);

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
