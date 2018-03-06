using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterSkillLevel : UISelectionEntry<CharacterSkillLevel>
{
    [Header("Generic Info Format")]
    [Tooltip("Title Format => {0} = {Title}")]
    public string titleFormat = "{0}";
    [Tooltip("Description Format => {0} = {Description}")]
    public string descriptionFormat = "{0}";
    [Tooltip("Level Format => {0} = {Level}")]
    public string levelFormat = "Lv: {0}";
    [Tooltip("Require Skill Level Format => {0} = {Skill title}, {1} = {Skill level}")]
    public string requireSkillLevelFormat = "Require {0}: {1}";
    [Tooltip("Consume Mp Format => {0} = {Consume Mp amount}")]
    public string consumeMpFormat = "Consume Mp: {0}";
    [Tooltip("Cool Down Duration Format => {0} = {Duration}")]
    public string coolDownDurationFormat = "Cooldown: {0}";
    [Tooltip("Cool Down Remains Duration Format => {0} = {Remains duration}")]
    public string coolDownRemainsDurationFormat = "{0}";

    [Header("Generic Buff Format")]
    [Tooltip("Buff Duration Format => {0} = {Duration}")]
    public string buffDurationFormat = "Duration: {0}";
    [Tooltip("Recovery Hp Format => {0} = {Recovery amount}")]
    public string recoveryHpFormat = "Recovery Hp: {0}";
    [Tooltip("Recovery Mp Format => {0} = {Recovery amount}")]
    public string recoveryMpFormat = "Recovery Mp: {0}";

    [Header("Skill Buff Stats Format")]
    [Tooltip("Hp Stats Format => {0} = {Amount}")]
    public string hpStatsFormat = "Hp: {0}";
    [Tooltip("Mp Stats Format => {0} = {Amount}")]
    public string mpStatsFormat = "Mp: {0}";
    [Tooltip("Atk Rate Stats Format => {0} = {Amount}")]
    public string atkRateStatsFormat = "Atk Rate: {0}";
    [Tooltip("Def Stats Format => {0} = {Amount}")]
    public string defStatsFormat = "Def: {0}";
    [Tooltip("Cri Hit Rate Stats Format => {0} = {Amount}")]
    public string criHitRateStatsFormat = "Cri Hit: {0}";
    [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}")]
    public string criDmgRateStatsFormat = "Cri Dmg: {0}";

    [Header("Skill Buff Stats Percentage Format")]
    [Tooltip("Hp Stats Percentage Format => {0} = {Amount}")]
    public string hpStatsPercentageFormat = "Hp: {0}%";
    [Tooltip("Mp Stats Percentage Format => {0} = {Amount}")]
    public string mpStatsPercentageFormat = "Mp: {0}%";
    [Tooltip("Atk Rate Stats Percentage Format => {0} = {Amount}")]
    public string atkRateStatsPercentageFormat = "Atk Rate: {0}%";
    [Tooltip("Def Stats Percentage Format => {0} = {Amount}")]
    public string defStatsPercentageFormat = "Def: {0}%";
    [Tooltip("Cri Hit Rate Stats Percentage Format => {0} = {Amount}")]
    public string criHitRateStatsPercentageFormat = "Cri Hit: {0}%";
    [Tooltip("Cri Dmg Rate Stats Percentage Format => {0} = {Amount}")]
    public string criDmgRateStatsPercentageFormat = "Cri Dmg: {0}%";

    [Header("Skill Attack Damage Format")]
    [Tooltip("Damage Format => {0} = {Damage title}, {1} = {Min damage}, {2} = {Max damage}")]
    public string damageFormat = "{0}: {1}~{2}";
    public string defaultDamageTitle = "Damage";

    [Header("UI Elements")]
    public Text textTitle;
    public Text textDescription;
    public Text textLevel;
    public Image imageIcon;
    public Text textRequireSkillLevel;
    public Text textConsumeMp;
    public Text textCoolDownDuration;
    public Text textCoolDownRemainsDuration;
    public Image imageCoolDownGage;
    public Text textBuffDuration;
    public Text textRecoveryHp;
    public Text textRecoveryMp;
    public Text textStats;
    public Text textStatsPercentage;
    public Text textDamage;

    protected virtual void Update()
    {
        var skillData = data.GetSkill();

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, skillData == null ? "Unknow" : skillData.title);

        if (textDescription != null)
            textDescription.text = string.Format(descriptionFormat, skillData == null ? "N/A" : skillData.description);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, data == null ? "N/A" : data.level.ToString("N0"));

        if (imageIcon != null)
            imageIcon.sprite = skillData == null ? null : skillData.icon;

        if (textRequireSkillLevel != null)
        {
            if (skillData == null || skillData.requireSkillLevels == null || skillData.requireSkillLevels.Length == 0)
                textRequireSkillLevel.gameObject.SetActive(false);
            else
            {
                var requireSkillLevels = skillData.requireSkillLevels;
                var requireSkillLevelsText = "";
                foreach (var requireSkillLevel in requireSkillLevels)
                {
                    if (requireSkillLevel == null || requireSkillLevel.skill == null || requireSkillLevel.level <= 0)
                        continue;
                    requireSkillLevelsText += string.Format(requireSkillLevelFormat, requireSkillLevel.skill.title, requireSkillLevel.level) + "\n";
                }
                textRequireSkillLevel.gameObject.SetActive(!string.IsNullOrEmpty(requireSkillLevelsText));
                textRequireSkillLevel.text = requireSkillLevelsText;
            }
        }

        if (textConsumeMp != null)
            textConsumeMp.text = string.Format(consumeMpFormat, data.GetConsumeMp().ToString("N0"));

        var coolDownRemainDuration = data.coolDownRemainsDuration;
        var coolDownDuration = data.GetCoolDownDuration();

        if (textCoolDownDuration != null)
            textCoolDownDuration.text = string.Format(coolDownDurationFormat, coolDownDuration.ToString("N0"));

        if (textCoolDownRemainsDuration != null)
            textCoolDownRemainsDuration.text = string.Format(coolDownRemainsDurationFormat, coolDownRemainDuration.ToString("N0"));

        if (imageCoolDownGage != null)
            imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 1 : coolDownRemainDuration / coolDownDuration;

        var isBuff = skillData != null && skillData.isBuff;

        if (textBuffDuration != null)
        {
            textBuffDuration.text = string.Format(buffDurationFormat, data.GetBuffDuration().ToString("N0"));
            textBuffDuration.gameObject.SetActive(isBuff);
        }

        if (textRecoveryHp != null)
        {
            textRecoveryHp.text = string.Format(recoveryHpFormat, data.GetRecoveryHp().ToString("N0"));
            textRecoveryHp.gameObject.SetActive(isBuff);
        }

        if (textRecoveryMp != null)
        {
            textRecoveryMp.text = string.Format(recoveryMpFormat, data.GetRecoveryMp().ToString("N0"));
            textRecoveryMp.gameObject.SetActive(isBuff);
        }

        var stats = data.GetStats();

        if (textStats != null)
        {
            var statsString = "";
            if (isBuff)
            {
                if (stats.hp != 0)
                    statsString += string.Format(hpStatsFormat, stats.hp) + "\n";
                if (stats.mp != 0)
                    statsString += string.Format(mpStatsFormat, stats.mp) + "\n";
                if (stats.atkRate != 0)
                    statsString += string.Format(atkRateStatsFormat, stats.atkRate) + "\n";
                if (stats.def != 0)
                    statsString += string.Format(defStatsFormat, stats.def) + "\n";
                if (stats.criHitRate != 0)
                    statsString += string.Format(criHitRateStatsFormat, stats.criHitRate) + "\n";
                if (stats.criDmgRate != 0)
                    statsString += string.Format(criDmgRateStatsFormat, stats.criDmgRate) + "\n";
            }
            textStats.gameObject.SetActive(!string.IsNullOrEmpty(statsString));
            textStats.text = statsString;
        }

        var statsPercentage = data.GetStatsPercentage();

        if (textStatsPercentage != null)
        {
            var statsPercentageString = "";
            if (isBuff)
            {
                if (statsPercentage.hp != 0)
                    statsPercentageString += string.Format(hpStatsPercentageFormat, statsPercentage.hp) + "\n";
                if (statsPercentage.mp != 0)
                    statsPercentageString += string.Format(mpStatsPercentageFormat, statsPercentage.mp) + "\n";
                if (statsPercentage.atkRate != 0)
                    statsPercentageString += string.Format(atkRateStatsPercentageFormat, statsPercentage.atkRate) + "\n";
                if (statsPercentage.def != 0)
                    statsPercentageString += string.Format(defStatsPercentageFormat, statsPercentage.def) + "\n";
                if (statsPercentage.criHitRate != 0)
                    statsPercentageString += string.Format(criHitRateStatsPercentageFormat, statsPercentage.criHitRate) + "\n";
                if (statsPercentage.criDmgRate != 0)
                    statsPercentageString += string.Format(criDmgRateStatsPercentageFormat, statsPercentage.criDmgRate) + "\n";
            }
            textStatsPercentage.gameObject.SetActive(!string.IsNullOrEmpty(statsPercentageString));
            textStatsPercentage.text = statsPercentageString;
        }

        if (textDamage != null)
        {
            if (skillData == null || !skillData.isAttack || skillData.TempDamageAmounts.Count == 0)
                textDamage.gameObject.SetActive(false);
            else
            {
                var damageAmounts = skillData.TempDamageAmounts.Values;
                var damagesText = "";
                foreach (var damageAmount in damageAmounts)
                {
                    damagesText += string.Format(damageFormat,
                        damageAmount.damageElement == null ? defaultDamageTitle : damageAmount.damageElement.title,
                        damageAmount.minDamage,
                        damageAmount.maxDamage) + "\n";
                }
                textDamage.gameObject.SetActive(!string.IsNullOrEmpty(damagesText));
                textDamage.text = damagesText;
            }
        }
    }
}

[System.Serializable]
public class UICharacterSkillLevelEvent : UnityEvent<UICharacterSkillLevel> { }
