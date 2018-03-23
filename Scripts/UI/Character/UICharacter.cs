using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacter : UISelectionEntry<ICharacterData>
{
    public string prototypeId { get; protected set; }

    [Header("Display Format")]
    [Tooltip("Name Format => {0} = {Character name}")]
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
    [Tooltip("Weight Limit Stats Format => {0} = {Weight Limit}")]
    public string weightLimitStatsFormat = "Weight Limit: {0}";
    [Tooltip("Weapon Damage => {0} = {Min damage}, {1} = {Max damage}")]
    public string weaponDamageFormat = "{0}~{1}";
    [Tooltip("Armor Format => {0} = {Armor}")]
    public string armorFormat = "Armor: {0}";

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
    public Text textWeightLimit;
    public Text textArmor;
    public Text textWeaponDamages;
    public UIDamageElementAmounts uiRightHandDamages;
    public UIDamageElementAmounts uiLeftHandDamages;
    public UICharacterStats uiCharacterStats;
    public UICharacterAttributePair[] uiCharacterAttributes;
    [Header("Class information")]
    public Text textClassTitle;
    public Text textClassDescription;
    public Image imageClassIcon;
    [Header("Options")]
    public bool showStatsWithBuffs;
    public bool showAttributeWithBuffs;

    private Dictionary<Attribute, UICharacterAttribute> cacheUICharacterAttributes = null;
    public Dictionary<Attribute, UICharacterAttribute> CacheUICharacterAttributes
    {
        get
        {
            if (cacheUICharacterAttributes == null)
            {
                cacheUICharacterAttributes = new Dictionary<Attribute, UICharacterAttribute>();
                foreach (var uiCharacterAttribute in uiCharacterAttributes)
                {
                    if (uiCharacterAttribute.attribute != null &&
                        uiCharacterAttribute.ui != null &&
                        !cacheUICharacterAttributes.ContainsKey(uiCharacterAttribute.attribute))
                        cacheUICharacterAttributes.Add(uiCharacterAttribute.attribute, uiCharacterAttribute.ui);
                }
            }
            return cacheUICharacterAttributes;
        }
    }

    public void Setup(ICharacterData data, string prototypeId)
    {
        this.prototypeId = prototypeId;
        Data = data;
    }

    private void Update()
    {
        if (textName != null)
            textName.text = string.Format(nameFormat, Data == null ? "Unknow" : Data.CharacterName);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, Data == null ? "N/A" : Data.Level.ToString("N0"));

        var statsWithBuff = Data.GetStatsWithBuffs();
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
            maxHp = (int)statsWithBuff.hp;
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
            maxMp = (int)statsWithBuff.mp;
        }

        if (textMp != null)
            textMp.text = string.Format(mpFormat, currentMp.ToString("N0"), maxMp.ToString("N0"));

        if (imageMpGage != null)
            imageMpGage.fillAmount = maxMp <= 0 ? 1 : currentMp / maxMp;

        var playerCharacter = Data as IPlayerCharacterData;
        if (textStatPoint != null)
            textStatPoint.text = string.Format(statPointFormat, playerCharacter == null ? "N/A" : playerCharacter.StatPoint.ToString("N0"));

        if (textSkillPoint != null)
            textSkillPoint.text = string.Format(skillPointFormat, playerCharacter == null ? "N/A" : playerCharacter.SkillPoint.ToString("N0"));

        if (textGold != null)
            textGold.text = string.Format(goldFormat, playerCharacter == null ? "N/A" : playerCharacter.Gold.ToString("N0"));

    }

    protected override void UpdateData()
    {
        var gameInstance = GameInstance.Singleton;
        var statsWithBuff = Data.GetStatsWithBuffs();
        var displayingStats = showStatsWithBuffs ? statsWithBuff : Data.GetStats();
        var displayingAttributes = showAttributeWithBuffs ? Data.GetAttributesWithBuffs() : Data.GetAttributes();
        
        if (textWeightLimit != null)
            textWeightLimit.text = string.Format(weightLimitStatsFormat, Data.GetTotalItemWeight().ToString("N2"), statsWithBuff.weightLimit.ToString("N2"));
        
        var rightHandItem = Data.EquipWeapons.rightHand;
        var leftHandItem = Data.EquipWeapons.leftHand;
        var hasRightHandWeapon = rightHandItem.IsValid();
        var hasLeftHandWeapon = leftHandItem.IsValid();
        var rightHandDamages = hasRightHandWeapon ? rightHandItem.GetWeaponItem().GetAllDamages(Data, rightHandItem.level) : null;
        var leftHandDamages = hasLeftHandWeapon ? leftHandItem.GetWeaponItem().GetAllDamages(Data, leftHandItem.level) : null;

        if (textWeaponDamages != null)
        {
            var textDamages = "";
            if (hasRightHandWeapon)
            {
                var sumDamages = GameDataHelpers.GetSumDamages(rightHandDamages);
                if (!string.IsNullOrEmpty(textDamages))
                    textDamages += "\n";
                textDamages += string.Format(weaponDamageFormat, sumDamages.minDamage, sumDamages.maxDamage);
            }
            if (hasLeftHandWeapon)
            {
                var sumDamages = GameDataHelpers.GetSumDamages(leftHandDamages);
                if (!string.IsNullOrEmpty(textDamages))
                    textDamages += "\n";
                textDamages += string.Format(weaponDamageFormat, sumDamages.minDamage, sumDamages.maxDamage);
            }
            if (!hasRightHandWeapon && !hasRightHandWeapon)
            {
                var defaultWeaponItem = gameInstance.DefaultWeaponItem;
                var defaultWeaponItemType = defaultWeaponItem.EquipType;
                var effectiveness = defaultWeaponItem.GetEffectivenessDamage(Data);
                var damageAmount = defaultWeaponItem.GetDamageAttribute(1, effectiveness, 1f);
                textDamages += string.Format(weaponDamageFormat, damageAmount.Value.minDamage, damageAmount.Value.maxDamage);
            }
            textWeaponDamages.text = textDamages;
        }

        if (uiRightHandDamages != null)
        {
            if (!hasRightHandWeapon)
                uiRightHandDamages.Hide();
            else
            {
                uiRightHandDamages.Show();
                uiRightHandDamages.Data = rightHandDamages;
            }
        }

        if (uiLeftHandDamages != null)
        {
            if (!hasLeftHandWeapon)
                uiLeftHandDamages.Hide();
            else
            {
                uiLeftHandDamages.Show();
                uiLeftHandDamages.Data = leftHandDamages;
            }
        }

        if (uiCharacterStats != null)
            uiCharacterStats.Data = displayingStats;

        if (CacheUICharacterAttributes.Count > 0 && Data != null)
        {
            var characterAttributes = Data.Attributes;
            for (var i = 0; i < characterAttributes.Count; ++i)
            {
                var characterAttribute = characterAttributes[i];
                var attribute = characterAttribute.GetAttribute();
                UICharacterAttribute cacheUICharacterAttribute;
                int amount;
                if (CacheUICharacterAttributes.TryGetValue(attribute, out cacheUICharacterAttribute) &&
                    displayingAttributes.TryGetValue(attribute, out amount))
                    cacheUICharacterAttribute.Setup(new KeyValuePair<CharacterAttribute, int>(characterAttribute, amount), i);
            }
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
    }
}

[System.Serializable]
public class UICharacterEvent : UnityEvent<UICharacter> { }