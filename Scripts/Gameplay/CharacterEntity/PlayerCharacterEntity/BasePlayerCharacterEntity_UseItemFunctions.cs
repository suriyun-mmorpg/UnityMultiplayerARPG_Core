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

            Item tempItem;
            // Use potion item
            tempItem = characterItem.GetPotionItem();
            if (tempItem != null)
                UseItemPotion(itemIndex, tempItem, characterItem.level);
            // Use pet item
            tempItem = characterItem.GetPetItem();
            if (tempItem != null)
                UseItemPetSummon(itemIndex, tempItem, characterItem.level, characterItem.exp);
            // Use mount item
            tempItem = characterItem.GetMountItem();
            if (tempItem != null)
                UseItemMount(itemIndex, tempItem, characterItem.level);
            // Use attribute increase
            tempItem = characterItem.GetAttributeIncreaseItem();
            if (tempItem != null)
                UseItemAttributeIncrease(itemIndex, tempItem);
            // Use attribute reset
            tempItem = characterItem.GetAttributeResetItem();
            if (tempItem != null)
                UseItemAttributeReset(itemIndex, tempItem);
            // Use skill learn
            tempItem = characterItem.GetSkillLearnItem();
            if (tempItem != null)
                UseItemSkillLearn(itemIndex, tempItem);
            // Use skill reset
            tempItem = characterItem.GetSkillResetItem();
            if (tempItem != null)
                UseItemSkillReset(itemIndex, tempItem);
        }

        /// <summary>
        /// This function will be called at server to order character to use item
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="aimPosition"></param>
        protected void NetFuncUseSkillItem(short itemIndex, bool isLeftHand, Vector3 aimPosition)
        {
            if (!CanUseItem() || !CanUseSkill())
                return;

            if (itemIndex >= nonEquipItems.Count)
                return;

            CharacterItem characterItem = nonEquipItems[itemIndex];
            if (characterItem.IsLock())
                return;

            Item tempItem;
            // Use skill
            tempItem = characterItem.GetSkillItem();
            if (tempItem != null)
                UseItemSkill(itemIndex, tempItem, isLeftHand, aimPosition);
        }

        protected void UseItemPotion(short itemIndex, Item item, short level)
        {
            if (!CanUseItem() || item == null || level <= 0 || !this.DecreaseItemsByIndex(itemIndex, 1))
                return;
            ApplyBuff(item.DataId, BuffType.PotionBuff, level);
        }

        protected void UseItemPetSummon(short itemIndex, Item item, short level, int exp)
        {
            if (!CanUseItem() || item == null || level <= 0 || !this.DecreaseItemsByIndex(itemIndex, 1))
                return;
            // Clear all summoned pets
            CharacterSummon tempSummon;
            for (int i = 0; i < Summons.Count; ++i)
            {
                tempSummon = summons[i];
                if (tempSummon.type != SummonType.Pet)
                    continue;
                summons.RemoveAt(i);
                tempSummon.UnSummon(this);
            }
            // Summon new pet
            CharacterSummon newSummon = CharacterSummon.Create(SummonType.Pet, item.DataId);
            newSummon.Summon(this, level, 0f, exp);
            summons.Add(newSummon);
        }

        protected void UseItemMount(short itemIndex, Item item, short level)
        {
            if (!CanUseItem() || item == null || level <= 0)
                return;

            Mount(item.mountEntity);
        }

        protected void UseItemAttributeIncrease(short itemIndex, Item item)
        {
            if (!CanUseItem() || item == null || item.attributeAmount.attribute == null)
                return;

            int dataId = item.attributeAmount.attribute.DataId;
            Attribute attribute;
            if (!GameInstance.Attributes.TryGetValue(dataId, out attribute))
                return;

            CharacterAttribute characterAtttribute;
            int index = this.IndexOfSkill(dataId);
            if (index < 0)
            {
                characterAtttribute = CharacterAttribute.Create(attribute, 0);
                if (!this.DecreaseItemsByIndex(itemIndex, 1))
                    return;
                characterAtttribute.amount += 1;
                Attributes.Add(characterAtttribute);
            }
            else
            {
                characterAtttribute = Attributes[index];
                if (!this.DecreaseItemsByIndex(itemIndex, 1))
                    return;
                characterAtttribute.amount += 1;
                Attributes[index] = characterAtttribute;
            }
        }

        protected void UseItemAttributeReset(short itemIndex, Item item)
        {
            if (!CanUseItem() || item == null || !this.DecreaseItemsByIndex(itemIndex, 1))
                return;

            short countStatPoint = 0;
            CharacterAttribute characterAttribute;
            for (int i = 0; i < Attributes.Count; ++i)
            {
                characterAttribute = Attributes[i];
                countStatPoint += characterAttribute.amount;
            }
            Attributes.Clear();
            StatPoint += countStatPoint;
        }

        protected void UseItemSkillLearn(short itemIndex, Item item)
        {
            if (!CanUseItem() || item == null || item.skillLevel.skill == null)
                return;

            int dataId = item.skillLevel.skill.DataId;

            BaseSkill skill;
            if (!GameInstance.Skills.TryGetValue(dataId, out skill))
                return;

            CharacterSkill characterSkill;
            int index = this.IndexOfSkill(dataId);
            if (index < 0)
            {
                characterSkill = CharacterSkill.Create(skill, 0);
                if (!characterSkill.CanLevelUp(this, false) || !this.DecreaseItemsByIndex(itemIndex, 1))
                    return;
                characterSkill.level += 1;
                Skills.Add(characterSkill);
            }
            else
            {
                characterSkill = Skills[index];
                if (!characterSkill.CanLevelUp(this, false) || !this.DecreaseItemsByIndex(itemIndex, 1))
                    return;
                characterSkill.level += 1;
                Skills[index] = characterSkill;
            }
        }

        protected void UseItemSkillReset(short itemIndex, Item item)
        {
            if (!CanUseItem() || item == null || !this.DecreaseItemsByIndex(itemIndex, 1))
                return;

            short countSkillPoint = 0;
            CharacterSkill characterSkill;
            for (int i = 0; i < Skills.Count; ++i)
            {
                characterSkill = Skills[i];
                countSkillPoint += characterSkill.level;
            }
            Skills.Clear();
            SkillPoint += countSkillPoint;
        }

        protected void UseItemSkill(short itemIndex, Item item, bool isLeftHand, Vector3 aimPosition)
        {
            if (!CanUseItem() || !CanUseSkill() || item == null || item.skillLevel.skill == null || !this.DecreaseItemsByIndex(itemIndex, 1))
                return;

            BaseSkill skill = item.skillLevel.skill;
            short skillLevel = item.skillLevel.level;

            // Validate mp amount, skill level, 
            GameMessage.Type gameMessageType;
            if (!skill.CanUse(this, skillLevel, out gameMessageType, true))
                return;

            // Prepare requires data and get skill data
            AnimActionType animActionType;
            int animatonDataId;
            CharacterItem weapon;
            GetUsingSkillData(
                skill,
                ref isLeftHand,
                out animActionType,
                out animatonDataId,
                out weapon);

            // Validate ammo
            if (skill.IsAttack() && !ValidateAmmo(weapon))
                return;

            // Prepare requires data and get animation data
            int animationIndex;
            float triggerDuration;
            float totalDuration;
            GetRandomAnimationData(
                animActionType,
                animatonDataId,
                out animationIndex,
                out triggerDuration,
                out totalDuration);

            // Start use skill routine
            isAttackingOrUsingSkill = true;

            // Play animations
            RequestPlaySkillAnimation(isLeftHand, (byte)animationIndex, skill.DataId, skillLevel, aimPosition);
        }
    }
}
