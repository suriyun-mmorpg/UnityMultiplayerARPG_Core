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

    [Header("Stats")]
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
    public string criHitRateStatsFormat = "Cri Hit: {0}%";
    [Tooltip("Cri Dmg Rate Stats Format => {0} = {Amount}")]
    public string criDmgRateStatsFormat = "Cri Dmg: {0}%";
    [Tooltip("Weight Limit Stats Format => {0} = {Weight Limit}")]
    public string weightLimitStatsFormat = "Weight Limit: {0}";

    [Header("Damage")]
    [Tooltip("Damage Format => {0} = {Damage title}, {1} = {Min damage}, {2} = {Max damage}")]
    public string damageFormat = "{0}: {1}~{2}";
    [Tooltip("Average Damage Format => {0} = {Min damage}, {1} = {Max damage}")]
    public string averageDamageFormat = "{0}~{1}";

    [Header("Class")]
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
    public Text textWeightLimit;
    public Text textAverageDamage;
    public Text textAllDamages;
    public UIAttributeAmountPair[] uiCharacterAttributes;
    [Header("Class information")]
    public Text textClassTitle;
    public Text textClassDescription;
    public Image imageClassIcon;
    [Header("Options")]
    public bool showStatsWithBuffs;
    public bool showAttributeWithBuffs;

    private Dictionary<string, UIAttributeAmount> tempUICharacterAttributes = null;
    public Dictionary<string, UIAttributeAmount> TempUICharacterAttributes
    {
        get
        {
            if (tempUICharacterAttributes == null)
            {
                tempUICharacterAttributes = new Dictionary<string, UIAttributeAmount>();
                foreach (var uiCharacterAttribute in uiCharacterAttributes)
                {
                    if (uiCharacterAttribute.attribute != null &&
                        uiCharacterAttribute.ui != null &&
                        !tempUICharacterAttributes.ContainsKey(uiCharacterAttribute.attribute.Id))
                        tempUICharacterAttributes.Add(uiCharacterAttribute.attribute.Id, uiCharacterAttribute.ui);
                }
            }
            return tempUICharacterAttributes;
        }
    }

    private void Update()
    {
        UpdateData();
    }

    protected override void UpdateData()
    {
        if (textName != null)
            textName.text = string.Format(nameFormat, Data == null ? "Unknow" : Data.CharacterName);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, Data == null ? "N/A" : Data.Level.ToString("N0"));

        var expTree = GameInstance.Singleton.expTree;
        var currentExp = 0;
        var nextLevelExp = 0;
        if (Data != null && Data.GetNextLevelExp() > 0)
        {
            currentExp = Data.Exp;
            nextLevelExp = Data.GetNextLevelExp();
        }
        else if (Data != null && Data.Level - 2 > 0 && Data.Level - 2 < expTree.Length)
        {
            var maxExp = expTree[Data.Level - 2];
            currentExp = maxExp;
            nextLevelExp = maxExp;
        }

        if (textExp != null)
            textExp.text = string.Format(expFormat, currentExp.ToString("N0"), nextLevelExp.ToString("N0"));

        if (imageExpGage != null)
            imageExpGage.fillAmount = nextLevelExp <= 0 ? 1 : currentExp / nextLevelExp;

        var currentHp = 0;
        var maxHp = 0;
        if (Data != null)
        {
            currentHp = Data.CurrentHp;
            maxHp = Data.GetMaxHp();
        }

        if (textHp != null)
            textHp.text = string.Format(hpFormat, currentHp.ToString("N0"), maxHp.ToString("N0"));

        if (imageHpGage != null)
            imageHpGage.fillAmount = maxHp <= 0 ? 1 : currentHp / maxHp;

        var currentMp = 0;
        var maxMp = 0;
        if (Data != null)
        {
            currentMp = Data.CurrentMp;
            maxMp = Data.GetMaxMp();
        }

        if (textMp != null)
            textMp.text = string.Format(mpFormat, currentMp.ToString("N0"), maxMp.ToString("N0"));

        if (imageMpGage != null)
            imageMpGage.fillAmount = maxMp <= 0 ? 1 : currentMp / maxMp;

        if (textStatPoint != null)
            textStatPoint.text = string.Format(statPointFormat, Data == null ? "N/A" : Data.StatPoint.ToString("N0"));

        if (textSkillPoint != null)
            textSkillPoint.text = string.Format(skillPointFormat, Data == null ? "N/A" : Data.SkillPoint.ToString("N0"));

        if (textGold != null)
            textGold.text = string.Format(goldFormat, Data == null ? "N/A" : Data.Gold.ToString("N0"));

        var stats = showStatsWithBuffs ? Data.GetStatsWithBuffs() : Data.GetStats();

        if (textAtkRateStats != null)
            textAtkRateStats.text = string.Format(atkRateStatsFormat, stats.atkRate.ToString("N0"));

        if (textDefStats != null)
            textDefStats.text = string.Format(defStatsFormat, stats.def.ToString("N0"));

        if (textCriHitRateStats != null)
            textCriHitRateStats.text = string.Format(criHitRateStatsFormat, (stats.criHitRate * 100f).ToString("N2"));

        if (textCriDmgRateStats != null)
            textCriDmgRateStats.text = string.Format(criDmgRateStatsFormat, (stats.criDmgRate * 100f).ToString("N2"));

        if (textWeightLimit != null)
            textWeightLimit.text = string.Format(weightLimitStatsFormat, stats.weightLimit.ToString("N2"));
        
        if (textAverageDamage != null || textAllDamages != null)
        {
            var damageAmountCount = 0;
            var damageAmountMin = 0f;
            var damageAmountMax = 0f;
            var damagesString = "";
            var characterWeapons = Data.GetWeapons();
            foreach (var characterWeapon in characterWeapons)
            {
                if (!string.IsNullOrEmpty(damagesString))
                    damagesString += "\n";
                var damageElementAmountPairs = characterWeapon.GetAdditionalDamageAttributes();
                foreach (var damageElementAmountPair in damageElementAmountPairs)
                {
                    ++damageAmountCount;
                    var element = damageElementAmountPair.Key;
                    var amount = damageElementAmountPair.Value;
                    damageAmountMin += amount.minDamage;
                    damageAmountMax += amount.maxDamage;
                    damagesString += string.Format(damageFormat,
                        element.title,
                        amount.minDamage,
                        amount.maxDamage) + "\n";
                }
            }

            // Find average damage
            if (damageAmountCount > 0)
            {
                damageAmountMin /= damageAmountCount;
                damageAmountMax /= damageAmountCount;
            }

            if (textAverageDamage != null)
                textAverageDamage.text = string.Format(averageDamageFormat, damageAmountMin, damageAmountMax);

            if (textAllDamages != null)
                textAllDamages.text = damagesString;
        }

        var classData = Data == null ? null : Data.GetClass();
        if (textClassTitle != null)
            textClassTitle.text = string.Format(classTitleFormat, classData == null ? "N/A" : classData.title);

        if (textClassDescription != null)
            textClassDescription.text = string.Format(classDescriptionFormat, classData == null ? "N/A" : classData.description);

        if (imageClassIcon != null)
        {
            imageClassIcon.sprite = classData == null ? null : classData.icon;
            imageClassIcon.gameObject.SetActive(classData != null);
        }

        if (TempUICharacterAttributes.Count > 0 && Data != null)
        {
            var totalAttributes = showAttributeWithBuffs ? Data.GetAttributesWithBuffs() : Data.GetAttributes();
            var characterAttributes = Data.Attributes;
            for (var i = 0; i < characterAttributes.Count; ++i)
            {
                var characterAttribute = characterAttributes[i];
                var attributeId = characterAttribute.attributeId;
                var attribute = characterAttribute.GetAttribute();
                if (TempUICharacterAttributes.ContainsKey(attributeId))
                {
                    var tempUICharacterAttribute = TempUICharacterAttributes[attributeId];
                    tempUICharacterAttribute.Data = new KeyValuePair<Attribute, int>(attribute, totalAttributes[attribute]);
                    tempUICharacterAttribute.indexOfData = i;
                }
            }
        }
    }
}

[System.Serializable]
public class UICharacterEvent : UnityEvent<UICharacter> { }