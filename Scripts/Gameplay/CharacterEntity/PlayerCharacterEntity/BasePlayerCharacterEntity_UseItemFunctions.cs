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
        /// <param name="aimPosition"></param>
        [ServerRpc]
        protected void ServerUseSkillItem(short itemIndex, bool isLeftHand, AimPosition aimPosition)
        {
#if !CLIENT_BUILD
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
            if (!item.UsingSkill.CanUse(this, item.UsingSkillLevel, isLeftHand, out _, true))
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
            GetRandomAnimationData(
                animActionType,
                animActionDataId,
                out animationIndex,
                out _,
                out _,
                out _);

            // Validate skill item
            if (!this.DecreaseItemsByIndex(itemIndex, 1))
                return;
            this.FillEmptySlots();

            // Start use skill routine
            IsAttackingOrUsingSkill = true;

            // Play animations
            CallAllPlayUseSkillAnimation(isLeftHand, (byte)animationIndex, item.UsingSkill.DataId, item.UsingSkillLevel, aimPosition);
#endif
        }
    }
}
