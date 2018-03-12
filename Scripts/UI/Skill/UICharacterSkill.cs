using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterSkill : UISelectionEntry<CharacterSkill>
{
    [System.NonSerialized]
    public int indexOfData;

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
    public Button addButton;
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

    private void Update()
    {
        var skill = Data.GetSkill();
        var skillLevel = Data.level;

        var owningCharacter = CharacterEntity.OwningCharacter;
        if (addButton != null)
            addButton.interactable = Data.CanLevelUp(owningCharacter);

        var coolDownRemainDuration = Data.coolDownRemainsDuration;
        var coolDownDuration = skill.GetCoolDownDuration(skillLevel);

        if (textCoolDownDuration != null)
            textCoolDownDuration.text = string.Format(coolDownDurationFormat, coolDownDuration.ToString("N0"));

        if (textCoolDownRemainsDuration != null)
            textCoolDownRemainsDuration.text = string.Format(coolDownRemainsDurationFormat, coolDownRemainDuration.ToString("N0"));

        if (imageCoolDownGage != null)
            imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 1 : coolDownRemainDuration / coolDownDuration;
    }

    protected override void UpdateData()
    {
        var skill = Data.GetSkill();
        var skillLevel = Data.level;

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, skill == null ? "Unknow" : skill.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, skill == null ? "N/A" : skill.description);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, skillLevel.ToString("N0"));

        if (imageIcon != null)
        {
            var iconSprite = skill == null ? null : skill.icon;
            imageIcon.sprite = iconSprite;
            imageIcon.gameObject.SetActive(iconSprite != null);
        }

        if (textConsumeMp != null)
            textConsumeMp.text = string.Format(consumeMpFormat, skill.GetConsumeMp(skillLevel).ToString("N0"));
        
        if (uiRequirement != null)
        {
            if (skill == null || (skill.GetRequireCharacterLevel(skillLevel) == 0 && skill.CacheRequireSkillLevels.Count == 0))
                uiRequirement.Hide();
            else
            {
                uiRequirement.Data = new KeyValuePair<Skill, int>(skill, skillLevel);
                uiRequirement.Show();
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
                uiDamageAttribute.Data = skill.GetDamageAttribute(skillLevel, 0f, 1f);
                uiDamageAttribute.Show();
            }
        }

        if (textInflictRate != null)
        {
            if (!isAttackWeaponInflict)
                textInflictRate.gameObject.SetActive(false);
            else
            {
                textInflictRate.text = string.Format(inflictRateFormat, (skill.GetInflictRate(skillLevel) * 100f).ToString("N0"));
                textInflictRate.gameObject.SetActive(true);
            }
        }

        if (uiAdditionalDamageAttributes != null)
        {
            if (!isAttackPure)
                uiAdditionalDamageAttributes.Hide();
            else
            {
                uiAdditionalDamageAttributes.Data = skill.GetAdditionalDamageAttributes(skillLevel);
                uiAdditionalDamageAttributes.Show();
            }
        }

        if (uiSkillBuff != null)
        {
            if (!skill.IsBuff())
                uiSkillBuff.Hide();
            else
            {
                uiSkillBuff.Data = new KeyValuePair<SkillBuff, int>(skill.buff, skillLevel);
                uiSkillBuff.Show();
            }
        }

        if (uiSkillDebuff != null)
        {
            if (!skill.IsDebuff())
                uiSkillDebuff.Hide();
            else
            {
                uiSkillDebuff.Data = new KeyValuePair<SkillBuff, int>(skill.debuff, skillLevel);
                uiSkillDebuff.Show();
            }
        }
    }

    public override void Show()
    {
        base.Show();
        if (addButton != null)
        {
            addButton.onClick.RemoveAllListeners();
            addButton.onClick.AddListener(OnClickAdd);
        }
    }

    private void OnClickAdd()
    {
        var owningCharacter = CharacterEntity.OwningCharacter;
        owningCharacter.AddSkill(indexOfData);
    }
}

[System.Serializable]
public class UICharacterSkillEvent : UnityEvent<UICharacterSkill> { }
