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

    [Header("Requirement Format")]
    [Tooltip("Require Level Format => {0} = {Level}")]
    public string requireLevelFormat = "Require Level: {0}";

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

    [Header("Skill Requirement - UI Elements")]
    public Text textRequireLevel;
    public UISkillLevels uiRequireSkillLevels;

    [Header("Attack as Pure Skill Damage - UI Elements")]
    public UIDamageElementAmount uiBaseDamageAttribute;
    public UIDamageElementAmounts uiAdditionalDamageAttributes;

    [Header("Attack as Weapon Damage Inflict - UI Elements")]
    public Text textInflictRate;
    public UIDamageElementAmounts uiInflictDamageAttributes;

    [Header("Buff/Debuff")]
    public UISkillBuff uiSkillBuff;
    public UISkillBuff uiSkillDebuff;

    private void Update()
    {
        var owningCharacter = CharacterEntity.OwningCharacter;
        if (addButton != null)
            addButton.interactable = Data != null && Data.CanLevelUp(owningCharacter);

        var coolDownRemainDuration = Data.coolDownRemainsDuration;
        var coolDownDuration = Data.GetCoolDownDuration();

        if (textCoolDownDuration != null)
            textCoolDownDuration.text = string.Format(coolDownDurationFormat, coolDownDuration.ToString("N0"));

        if (textCoolDownRemainsDuration != null)
            textCoolDownRemainsDuration.text = string.Format(coolDownRemainsDurationFormat, coolDownRemainDuration.ToString("N0"));

        if (imageCoolDownGage != null)
            imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 1 : coolDownRemainDuration / coolDownDuration;
    }

    protected override void UpdateData()
    {
        var skillData = Data.GetSkill();

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, skillData == null ? "Unknow" : skillData.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, skillData == null ? "N/A" : skillData.description);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, Data.level.ToString("N0"));

        if (imageIcon != null)
        {
            var iconSprite = skillData == null ? null : skillData.icon;
            imageIcon.sprite = iconSprite;
            imageIcon.gameObject.SetActive(iconSprite != null);
        }

        if (textConsumeMp != null)
            textConsumeMp.text = string.Format(consumeMpFormat, Data.GetConsumeMp().ToString("N0"));


        if (textRequireLevel != null)
        {
            if (skillData == null)
                textRequireLevel.gameObject.SetActive(false);
            else
            {
                textRequireLevel.text = string.Format(requireLevelFormat, Data.GetRequireCharacterLevel().ToString("N0"));
                textRequireLevel.gameObject.SetActive(true);
            }
        }

        if (uiRequireSkillLevels != null)
        {
            if (skillData == null)
                uiRequireSkillLevels.gameObject.SetActive(false);
            else
            {
                uiRequireSkillLevels.Data = skillData.TempRequireSkillLevels;
                uiRequireSkillLevels.gameObject.SetActive(true);
            }
        }

        var isAttackPure = skillData != null && skillData.IsAttack() && skillData.skillAttackType == SkillAttackType.PureSkillDamage;
        var isAttackWeaponInflict = skillData != null && skillData.IsAttack() && skillData.skillAttackType == SkillAttackType.WeaponDamageInflict;

        if (uiBaseDamageAttribute != null)
        {
            if (!isAttackPure)
                uiBaseDamageAttribute.Hide();
            else
            {
                uiBaseDamageAttribute.Data = Data.GetBaseDamageAttribute();
                uiBaseDamageAttribute.Show();
            }
        }

        if (uiAdditionalDamageAttributes != null)
        {
            if (!isAttackPure)
                uiAdditionalDamageAttributes.Hide();
            else
            {
                uiAdditionalDamageAttributes.Data = Data.GetAdditionalDamageAttributes();
                uiAdditionalDamageAttributes.Show();
            }
        }

        if (textInflictRate != null)
        {
            if (!isAttackWeaponInflict)
                textInflictRate.gameObject.SetActive(false);
            else
            {
                textInflictRate.text = string.Format(inflictRateFormat, (Data.GetInflictRate() * 100f).ToString("N0"));
                textInflictRate.gameObject.SetActive(true);
            }
        }

        if (uiInflictDamageAttributes != null)
        {
            if (!isAttackWeaponInflict)
                uiInflictDamageAttributes.Hide();
            else
            {
                uiInflictDamageAttributes.Data = Data.GetInflictDamageAttributes();
                uiInflictDamageAttributes.Hide();
            }
        }

        if (uiSkillBuff != null)
        {
            if (!skillData.IsBuff())
                uiSkillBuff.Hide();
            else
            {
                uiSkillBuff.Data = new KeyValuePair<SkillBuff, int>(skillData.buff, Data.level);
                uiSkillBuff.Show();
            }
        }

        if (uiSkillDebuff != null)
        {
            if (!skillData.IsDebuff())
                uiSkillDebuff.Hide();
            else
            {
                uiSkillDebuff.Data = new KeyValuePair<SkillBuff, int>(skillData.debuff, Data.level);
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
