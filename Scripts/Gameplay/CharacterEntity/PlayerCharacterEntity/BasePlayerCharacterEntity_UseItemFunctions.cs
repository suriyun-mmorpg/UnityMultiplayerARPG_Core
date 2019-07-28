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
        /// <param name="hasAimPosition"></param>
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
        /// <param name="hasAimPosition"></param>
        /// <param name="aimPosition"></param>
        protected void NetFuncUseSkillItem(short itemIndex, bool isLeftHand, bool hasAimPosition, Vector3 aimPosition)
        {
            if (!CanUseItem())
                return;

            if (itemIndex >= nonEquipItems.Count)
                return;

            CharacterItem characterItem = nonEquipItems[itemIndex];
            if (characterItem.IsLock())
                return;

            Item tempItem;
            // Use skill reset
            tempItem = characterItem.GetSkillItem();
            if (tempItem != null)
                UseItemSkill(itemIndex, tempItem, isLeftHand, hasAimPosition, aimPosition);
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
            Attribute attributeData;
            if (!GameInstance.Attributes.TryGetValue(dataId, out attributeData))
                return;

            CharacterAttribute atttribute;
            int index = this.IndexOfSkill(dataId);
            if (index < 0)
            {
                atttribute = CharacterAttribute.Create(attributeData, 0);
                if (!this.DecreaseItemsByIndex(itemIndex, 1))
                    return;
                atttribute.amount += 1;
                Attributes.Add(atttribute);
            }
            else
            {
                atttribute = Attributes[index];
                if (!this.DecreaseItemsByIndex(itemIndex, 1))
                    return;
                atttribute.amount += 1;
                Attributes[index] = atttribute;
            }
        }

        protected void UseItemAttributeReset(short itemIndex, Item item)
        {
            if (!CanUseItem() || item == null || !this.DecreaseItemsByIndex(itemIndex, 1))
                return;

            short countStatPoint = 0;
            CharacterAttribute attribute;
            for (int i = 0; i < Attributes.Count; ++i)
            {
                attribute = Attributes[i];
                countStatPoint += attribute.amount;
                attribute.amount = 0;
                Attributes[i] = attribute;
            }
            StatPoint += countStatPoint;
        }

        protected void UseItemSkillLearn(short itemIndex, Item item)
        {
            if (!CanUseItem() || item == null || item.skillLevel.skill == null)
                return;

            int dataId = item.skillLevel.skill.DataId;

            Skill skillData;
            if (!GameInstance.Skills.TryGetValue(dataId, out skillData))
                return;

            CharacterSkill skill;
            int index = this.IndexOfSkill(dataId);
            if (index < 0)
            {
                skill = CharacterSkill.Create(skillData, 0);
                if (!skill.CanLevelUp(this) || !this.DecreaseItemsByIndex(itemIndex, 1))
                    return;
                skill.level += 1;
                Skills.Add(skill);
            }
            else
            {
                skill = Skills[index];
                if (!skill.CanLevelUp(this) || !this.DecreaseItemsByIndex(itemIndex, 1))
                    return;
                skill.level += 1;
                Skills[index] = skill;
            }
        }

        protected void UseItemSkillReset(short itemIndex, Item item)
        {
            if (!CanUseItem() || item == null || !this.DecreaseItemsByIndex(itemIndex, 1))
                return;

            short countSkillPoint = 0;
            CharacterSkill skill;
            for (int i = 0; i < Skills.Count; ++i)
            {
                skill = Skills[i];
                countSkillPoint += skill.level;
                skill.level = 0;
                Skills[i] = skill;
            }
            SkillPoint += countSkillPoint;
        }

        protected void UseItemSkill(short itemIndex, Item item, bool isLeftHand, bool hasAimPosition, Vector3 aimPosition)
        {
            if (!CanUseItem() || !CanUseSkill() || item == null || item.skillLevel.skill == null || !this.DecreaseItemsByIndex(itemIndex, 1))
                return;

            Skill skill = item.skillLevel.skill;
            short level = item.skillLevel.level;

            // Prepare requires data
            AnimActionType animActionType;
            int skillOrWeaponTypeDataId;
            int animationIndex;
            CharacterItem weapon;
            float triggerDuration;
            float totalDuration;
            DamageInfo damageInfo;
            Dictionary<DamageElement, MinMaxFloat> allDamageAmounts;

            GetUsingSkillData(
                skill,
                level,
                ref isLeftHand,
                out animActionType,
                out skillOrWeaponTypeDataId,
                out animationIndex,
                out weapon,
                out triggerDuration,
                out totalDuration,
                out damageInfo,
                out allDamageAmounts);

            // Validate ammo
            if (skill.skillAttackType != SkillAttackType.None && !ValidateAmmo(weapon))
                return;

            // Call on cast skill to extend skill functionality while casting skills
            // Quit function when on cast skill will override default cast skill functionality
            if (skill.OnCastSkill(this, level, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition))
                return;

            // Start use skill routine
            isAttackingOrUsingSkill = true;
            StartCoroutine(UseSkillRoutine(skill, level, animActionType, skillOrWeaponTypeDataId, animationIndex, triggerDuration, totalDuration, isLeftHand, weapon, damageInfo, allDamageAmounts, hasAimPosition, aimPosition));
        }
    }
}
