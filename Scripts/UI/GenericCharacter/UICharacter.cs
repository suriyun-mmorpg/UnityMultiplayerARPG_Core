using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacter : UISelectionEntry<ICharacterData>
{
    [Header("Display Format")]
    [Tooltip("Name Format => {0} = {name}")]
    public string nameFormat = "{0}";
    [Tooltip("Level Format => {0} = {Level}")]
    public string levelFormat = "Lv: {0}";
    [Tooltip("Exp Format => {0} = {Current exp}, {1} = {Max exp}")]
    public string expFormat = "Exp: {0}/{1}";
    [Tooltip("Hp Format => {0} = {Current hp}, {1} = {Max hp}")]
    public string hpFormat = "Hp: {0}/{1}";
    [Tooltip("Mp Format => {0} = {Current mp}, {1} = {Max mp}")]
    public string mpFormat = "Mp: {0}/{1}";
    [Tooltip("Stat Point Format => {0} = {Stat point}")]
    public string statPointFormat = "Stat Points: {0}";
    [Tooltip("Skill Point Format => {0} = {Skill point}")]
    public string skillPointFormat = "Skill Points: {0}";
    [Tooltip("Gold Format => {0} = {Gold}")]
    public string goldFormat = "Gold: {0}";
    [Tooltip("Atk Rate Stats Format => {0} = {Amount}")]
    public string atkRateStatsFormat = "Atk Rate: {0}";
    [Tooltip("Def Stats Format => {0} = {Amount}")]
    public string defStatsFormat = "Def: {0}";
    [Tooltip("Cri Hit Rate Stats Format => {0} = {Amount}")]
    public string criHitRateStatsFormat = "Cri Hit: {0}";
    [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}")]
    public string criDmgRateStatsFormat = "Cri Dmg: {0}";
    [Tooltip("Damage Format => {0} = {Damage title}, {1} = {Min damage}, {2} = {Max damage}")]
    public string damageFormat = "{0}: {1}~{2}";
    public string defaultDamageTitle = "Damage";
    public string averageDamageTitle = "Average Damage";
    [Tooltip("Class Title Format => {0} = {Class title}")]
    public string classTitleFormat = "Class: {0}";
    [Tooltip("Class Description Format => {0} = {Class description}")]
    public string classDescriptionFormat = "{0}";
    [Header("UI Elements")]
    public Text textName;
    public Text textLevel;
    public Text textExp;
    public Text textHp;
    public Text textMp;
    public Text textStatPoint;
    public Text textSkillPoint;
    public Text textGold;
    public Text textAtkRateStats;
    public Text textDefStats;
    public Text textCriHitRateStats;
    public Text textCriDmgRateStats;
    public Text textAverageDamage;
    public Text textAllDamages;
    [Header("Class information")]
    public Text textClassTitle;
    public Text textClassDescription;
    public Image imageClassIcon;

    protected virtual void Update()
    {
        if (textName != null)
            textName.text = string.Format(nameFormat, data == null ? "Unknow" : data.CharacterName);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, data == null ? "N/A" : data.Level.ToString("N0"));

        if (textExp != null)
        {
            var expString = "";
            if (data == null)
                expString = string.Format(expFormat, 0, 0);
            else if (data.GetNextLevelExp() > 0)
                expString = string.Format(expFormat, data.Exp.ToString("N0"), data.GetNextLevelExp().ToString("N0"));
            else
                expString = string.Format(expFormat, "Max");
            textExp.text = expString;
        }

        if (textHp != null)
        {
            var hpString = "";
            if (data == null)
                hpString = string.Format(hpFormat, 0, 0);
            else
                hpString = string.Format(hpFormat, data.CurrentHp.ToString("N0"), data.GetMaxHp().ToString("N0"));
            textHp.text = hpString;
        }

        if (textMp != null)
        {
            var mpString = "";
            if (data == null)
                mpString = string.Format(mpFormat, 0, 0);
            else
                mpString = string.Format(mpFormat, data.CurrentMp.ToString("N0"), data.GetMaxMp().ToString("N0"));
            textMp.text = mpString;
        }

        if (textStatPoint != null)
            textStatPoint.text = string.Format(statPointFormat, data == null ? "N/A" : data.StatPoint.ToString("N0"));

        if (textSkillPoint != null)
            textSkillPoint.text = string.Format(skillPointFormat, data == null ? "N/A" : data.SkillPoint.ToString("N0"));

        if (textGold != null)
            textGold.text = string.Format(goldFormat, data == null ? "N/A" : data.Gold.ToString("N0"));

        var stats = data.GetStatsWithoutBuffs();

        if (textAtkRateStats != null)
        {
            textAtkRateStats.gameObject.SetActive(stats.atkRate != 0);
            textAtkRateStats.text = stats.atkRate.ToString("N0");
        }

        if (textDefStats != null)
        {
            textDefStats.gameObject.SetActive(stats.def != 0);
            textDefStats.text = stats.def.ToString("N0");
        }

        if (textCriHitRateStats != null)
        {
            textCriHitRateStats.gameObject.SetActive(stats.criHitRate != 0);
            textCriHitRateStats.text = stats.criHitRate.ToString("N0");
        }

        if (textCriDmgRateStats != null)
        {
            textCriDmgRateStats.gameObject.SetActive(stats.criDmgRate != 0);
            textCriDmgRateStats.text = stats.criDmgRate.ToString("N0");
        }
        
        if (textAllDamages != null || textAllDamages != null)
        {
            var damageAmountCount = 0;
            var damageAmountMin = 0f;
            var damageAmountMax = 0f;
            var damagesString = "";
            var weaponItems = data.GetWeapons();
            foreach (var weaponItem in weaponItems)
            {
                if (!string.IsNullOrEmpty(damagesString))
                    damagesString += "\n";
                var damageAmounts = weaponItem.TempDamageAmounts.Values;
                foreach (var damageAmount in damageAmounts)
                {
                    ++damageAmountCount;
                    damageAmountMin += damageAmount.minDamage;
                    damageAmountMax += damageAmount.maxDamage;
                    damagesString += string.Format(damageFormat,
                        damageAmount.damage == null ? defaultDamageTitle : damageAmount.damage.title,
                        damageAmount.minDamage,
                        damageAmount.maxDamage) + "\n";
                }
            }

            // Find average damage
            if (damageAmountCount > 0)
            {
                damageAmountMin /= damageAmountCount;
                damageAmountMax /= damageAmountCount;
            }

            if (textAverageDamage != null)
            {
                textAverageDamage.gameObject.SetActive(damageAmountCount > 0);
                textAverageDamage.text = string.Format(damageFormat, averageDamageTitle, damageAmountMin, damageAmountMax);
            }

            if (textAllDamages != null)
            {
                textAllDamages.gameObject.SetActive(damageAmountCount > 0);
                textAllDamages.text = damagesString;
            }
        }

        if (textClassTitle != null)
            textClassTitle.text = string.Format(classTitleFormat, data == null ? "N/A" : data.GetClass().title);

        if (textClassDescription != null)
            textClassDescription.text = string.Format(classDescriptionFormat, data == null ? "N/A" : data.GetClass().description);

        if (imageClassIcon != null)
            imageClassIcon.sprite = data == null ? null : data.GetClass().icon;
    }
}

[System.Serializable]
public class UICharacterEvent : UnityEvent<UICharacter> { }