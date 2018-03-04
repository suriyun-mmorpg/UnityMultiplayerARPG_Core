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
    public Image imageExpGage;
    public Text textHp;
    public Image imageHpGage;
    public Text textMp;
    public Image imageMpGage;
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
    [Header("Options")]
    public bool showStatsWithBuff;

    protected virtual void Update()
    {
        if (textName != null)
            textName.text = string.Format(nameFormat, data == null ? "Unknow" : data.CharacterName);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, data == null ? "N/A" : data.Level.ToString("N0"));

        var expTree = GameInstance.Singleton.expTree;
        var currentExp = 0;
        var nextLevelExp = 0;
        if (data != null && data.GetNextLevelExp() > 0)
        {
            currentExp = data.Exp;
            nextLevelExp = data.GetNextLevelExp();
        }
        else if (data != null && data.Level - 2 > 0 && data.Level - 2 < expTree.Length)
        {
            var maxExp = expTree[data.Level - 2];
            currentExp = maxExp;
            nextLevelExp = maxExp;
        }

        if (textExp != null)
            textExp.text = string.Format(expFormat, currentExp.ToString("N0"), nextLevelExp.ToString("N0"));

        if (imageExpGage != null)
            imageExpGage.fillAmount = nextLevelExp <= 0 ? 1 : currentExp / nextLevelExp;

        var currentHp = 0;
        var maxHp = 0;
        if (data != null)
        {
            currentHp = data.CurrentHp;
            maxHp = data.GetMaxHp();
        }

        if (textHp != null)
            textHp.text = string.Format(hpFormat, currentHp.ToString("N0"), maxHp.ToString("N0"));

        if (imageHpGage != null)
            imageHpGage.fillAmount = maxHp <= 0 ? 1 : currentHp / maxHp;

        var currentMp = 0;
        var maxMp = 0;
        if (data != null)
        {
            currentMp = data.CurrentMp;
            maxMp = data.GetMaxMp();
        }

        if (textMp != null)
            textMp.text = string.Format(mpFormat, currentMp.ToString("N0"), maxMp.ToString("N0"));

        if (imageMpGage != null)
            imageMpGage.fillAmount = maxMp <= 0 ? 1 : currentMp / maxMp;

        if (textStatPoint != null)
            textStatPoint.text = string.Format(statPointFormat, data == null ? "N/A" : data.StatPoint.ToString("N0"));

        if (textSkillPoint != null)
            textSkillPoint.text = string.Format(skillPointFormat, data == null ? "N/A" : data.SkillPoint.ToString("N0"));

        if (textGold != null)
            textGold.text = string.Format(goldFormat, data == null ? "N/A" : data.Gold.ToString("N0"));

        var stats = showStatsWithBuff ? data.GetStatsWithBuffs() : data.GetStatsWithoutBuffs();

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
                        damageAmount.damageElement == null ? defaultDamageTitle : damageAmount.damageElement.title,
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