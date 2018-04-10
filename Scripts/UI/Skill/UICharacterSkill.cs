using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterSkill : UIDataForCharacter<KeyValuePair<CharacterSkill, int>>
{
    [Header("Generic Info Format")]
    [Tooltip("Title Format => {0} = {Title}")]
    public string titleFormat = "{0}";
    [Tooltip("Description Format => {0} = {Description}")]
    public string descriptionFormat = "{0}";
    [Tooltip("Level Format => {0} = {Level}")]
    public string levelFormat = "Lv: {0}";
    [Tooltip("Consume Mp Format => {0} = {Consume Mp amount}")]
    public string consumeMpFormat = "Consume Mp: {0}";
    [Tooltip("Cool Down Duration Format => {0} = {Duration}")]
    public string coolDownDurationFormat = "Cooldown: {0}";
    [Tooltip("Cool Down Remains Duration Format => {0} = {Remains duration}")]
    public string coolDownRemainsDurationFormat = "{0}";

    [Header("Attack Format")]
    [Tooltip("Inflict Rate Format => {0} = {Rate * 100f}")]
    public string inflictRateFormat = "Inflict {0}%";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Text textLevel;
    public Image imageIcon;
    public Text textConsumeMp;
    public Text textCoolDownDuration;
    public Text textCoolDownRemainsDuration;
    public Image imageCoolDownGage;
    public UISkillRequirement uiRequirement;

    [Header("Attack as Pure Skill Damage - UI Elements")]
    public UIDamageElementAmount uiDamageAttribute;

    [Header("Attack as Weapon Damage Inflict - UI Elements")]
    public Text textInflictRate;

    [Header("Attack additional attributes")]
    public UIDamageElementAmounts uiAdditionalDamageAttributes;

    [Header("Buff/Debuff")]
    public UISkillBuff uiSkillBuff;
    public UISkillBuff uiSkillDebuff;

    [Header("Events")]
    public UnityEvent onSetLevelZeroData;
    public UnityEvent onSetNonLevelZeroData;
    public UnityEvent onAbleToLevelUp;
    public UnityEvent onUnableToLevelUp;

    [Header("Options")]
    public UICharacterSkill uiNextLevelSkill;
    public bool hideRemainsDurationWhenIsZero;
    
    private void Update()
    {
        var characterSkill = Data.Key;
        var skill = characterSkill.GetSkill();
        var level = Data.Value;
        
        if (IsOwningCharacter() && characterSkill.CanLevelUp(PlayerCharacterController.OwningCharacter))
            onAbleToLevelUp.Invoke();
        else
            onUnableToLevelUp.Invoke();

        // Update duration
        if (character != null && indexOfData >= 0 && indexOfData < character.Skills.Count)
            characterSkill = character.Skills[indexOfData];

        var coolDownRemainDuration = characterSkill.coolDownRemainsDuration;
        var coolDownDuration = skill.GetCoolDownDuration(level);

        if (textCoolDownDuration != null)
            textCoolDownDuration.text = string.Format(coolDownDurationFormat, coolDownDuration.ToString("N0"));

        if (textCoolDownRemainsDuration != null)
        {
            var remainsDurationString = "";
            if (!hideRemainsDurationWhenIsZero || characterSkill.coolDownRemainsDuration > 0)
            {
                if (skill == null)
                    remainsDurationString = string.Format(coolDownRemainsDurationFormat, "0");
                else
                    remainsDurationString = string.Format(coolDownRemainsDurationFormat, coolDownRemainDuration.ToString("N0"));
            }
            textCoolDownRemainsDuration.text = remainsDurationString;
        }

        if (imageCoolDownGage != null)
            imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 0 : coolDownRemainDuration / coolDownDuration;
    }

    protected override void UpdateData()
    {
        var characterSkill = Data.Key;
        var skill = characterSkill.GetSkill();
        var level = Data.Value;

        if (level <= 0)
            onSetLevelZeroData.Invoke();
        else
            onSetNonLevelZeroData.Invoke();

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, skill == null ? "Unknow" : skill.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, skill == null ? "N/A" : skill.description);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, level.ToString("N0"));

        if (imageIcon != null)
        {
            var iconSprite = skill == null ? null : skill.icon;
            imageIcon.sprite = iconSprite;
            imageIcon.gameObject.SetActive(iconSprite != null);
        }

        if (textConsumeMp != null)
            textConsumeMp.text = string.Format(consumeMpFormat, skill == null || level <= 0 ? "N/A" : skill.GetConsumeMp(level).ToString("N0"));
        
        if (uiRequirement != null)
        {
            if (skill == null || (skill.GetRequireCharacterLevel(level) == 0 && skill.CacheRequireSkillLevels.Count == 0))
                uiRequirement.Hide();
            else
            {
                uiRequirement.Show();
                uiRequirement.Data = new KeyValuePair<Skill, int>(skill, level);
            }
        }

        var isAttackPure = skill != null && skill.IsAttack() && skill.skillAttackType == SkillAttackType.PureSkillDamage;
        var isAttackWeaponInflict = skill != null && skill.IsAttack() && skill.skillAttackType == SkillAttackType.WeaponDamageInflict;

        if (uiDamageAttribute != null)
        {
            if (!isAttackPure)
                uiDamageAttribute.Hide();
            else
            {
                uiDamageAttribute.Show();
                uiDamageAttribute.Data = skill.GetDamageAttribute(level, 0f, 1f);
            }
        }

        if (textInflictRate != null)
        {
            if (!isAttackWeaponInflict)
                textInflictRate.gameObject.SetActive(false);
            else
            {
                textInflictRate.text = string.Format(inflictRateFormat, (skill.GetInflictRate(level) * 100f).ToString("N0"));
                textInflictRate.gameObject.SetActive(true);
            }
        }

        if (uiAdditionalDamageAttributes != null)
        {
            var additionalDamageAttributes = skill.GetAdditionalDamageAttributes(level);
            if (!isAttackPure || additionalDamageAttributes == null || additionalDamageAttributes.Count == 0)
                uiAdditionalDamageAttributes.Hide();
            else
            {
                uiAdditionalDamageAttributes.Show();
                uiAdditionalDamageAttributes.Data = additionalDamageAttributes;
            }
        }

        if (uiSkillBuff != null)
        {
            if (!skill.IsBuff())
                uiSkillBuff.Hide();
            else
            {
                uiSkillBuff.Show();
                uiSkillBuff.Data = new KeyValuePair<SkillBuff, int>(skill.buff, level);
            }
        }

        if (uiSkillDebuff != null)
        {
            if (!skill.IsDebuff())
                uiSkillDebuff.Hide();
            else
            {
                uiSkillDebuff.Show();
                uiSkillDebuff.Data = new KeyValuePair<SkillBuff, int>(skill.debuff, level);
            }
        }

        if (uiNextLevelSkill != null)
        {
            if (level + 1 > skill.maxLevel)
                uiNextLevelSkill.Hide();
            else
            {
                uiNextLevelSkill.Setup(new KeyValuePair<CharacterSkill, int>(characterSkill, level + 1), character, indexOfData);
                uiNextLevelSkill.Show();
            }
        }
    }

    public void OnClickAdd()
    {
        if (!IsOwningCharacter())
            return;

        if (selectionManager != null)
            selectionManager.DeselectSelectedUI();

        var owningCharacter = PlayerCharacterController.OwningCharacter;
        if (owningCharacter != null)
            owningCharacter.RequestAddSkill(indexOfData, 1);
    }
}

[System.Serializable]
public class UICharacterSkillEvent : UnityEvent<UICharacterSkill> { }
