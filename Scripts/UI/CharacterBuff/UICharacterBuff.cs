using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterBuff : UISelectionEntry<CharacterBuff>
{
    [Header("Generic Info Format")]
    [Tooltip("Title Format => {0} = {Title}")]
    public string titleFormat = "{0}";

    [Header("Generic Buff Format")]
    [Tooltip("Buff Duration Format => {0} = {Duration}")]
    public string buffDurationFormat = "Duration: {0}";
    [Tooltip("Buff Remains Duration Format => {0} = {Remains duration}")]
    public string buffRemainsDurationFormat = "{0}";
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

    [Header("UI Elements")]
    public Text textTitle;
    public Image imageIcon;
    public Text textBuffDuration;
    public Text textBuffRemainsDuration;
    public Image imageBuffDurationGage;
    public Text textRecoveryHp;
    public Text textRecoveryMp;
    public Text textStats;
    public Text textStatsPercentage;

    protected virtual void Update()
    {
        var skillData = data.GetSkill();

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, skillData == null ? "Unknow" : skillData.title);

        if (imageIcon != null)
            imageIcon.sprite = skillData == null ? null : skillData.icon;

        var buffRemainDuration = data.buffRemainsDuration;
        var buffDuration = data.GetBuffDuration();

        if (textBuffDuration != null)
            textBuffDuration.text = string.Format(buffDurationFormat, buffDuration.ToString("N0"));

        if (textBuffRemainsDuration != null)
            textBuffRemainsDuration.text = string.Format(buffRemainsDurationFormat, buffRemainDuration.ToString("N0"));

        if (imageBuffDurationGage != null)
            imageBuffDurationGage.fillAmount = buffDuration <= 0 ? 1 : buffRemainDuration / buffDuration;

        if (textRecoveryHp != null)
            textRecoveryHp.text = string.Format(recoveryHpFormat, data.GetRecoveryHp().ToString("N0"));

        if (textRecoveryMp != null)
            textRecoveryMp.text = string.Format(recoveryMpFormat, data.GetRecoveryMp().ToString("N0"));

        var stats = data.GetStats();

        if (textStats != null)
        {
            var statsString = "";
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
            textStats.gameObject.SetActive(!string.IsNullOrEmpty(statsString));
            textStats.text = statsString;
        }

        var statsPercentage = data.GetStatsPercentage();

        if (textStatsPercentage != null)
        {
            var statsPercentageString = "";
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
            textStatsPercentage.gameObject.SetActive(!string.IsNullOrEmpty(statsPercentageString));
            textStatsPercentage.text = statsPercentageString;
        }
    }
}

[System.Serializable]
public class UICharacterBuffEvent : UnityEvent<UICharacterBuff> { }
