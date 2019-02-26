using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacter : UISelectionEntry<ICharacterData>
    {
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
        [Tooltip("Stamina Format => {0} = {Current stamina}, {1} = {Max stamina}")]
        public string staminaFormat = "Stamina: {0}/{1}";
        [Tooltip("Food Format => {0} = {Current food}, {1} = {Max food}")]
        public string foodFormat = "Food: {0}/{1}";
        [Tooltip("Water Format => {0} = {Current water}, {1} = {Max water}")]
        public string waterFormat = "Water: {0}/{1}";
        [Tooltip("Stat Point Format => {0} = {Stat point}")]
        public string statPointFormat = "Stat Points: {0}";
        [Tooltip("Skill Point Format => {0} = {Skill point}")]
        public string skillPointFormat = "Skill Points: {0}";
        [Tooltip("Gold Format => {0} = {Gold}")]
        public string goldFormat = "Gold: {0}";
        [Tooltip("Weight Limit Stats Format => {0} = {Current Total Weights}, {1} = {Weight Limit}")]
        public string weightLimitStatsFormat = "Weight: {0}/{1}";
        [Tooltip("Weapon Damage => {0} = {Min damage}, {1} = {Max damage}")]
        public string weaponDamageFormat = "{0}~{1}";

        [Header("Class")]
        [Tooltip("Class Title Format => {0} = {Class title}")]
        public string classTitleFormat = "Class: {0}";
        [Tooltip("Class Description Format => {0} = {Class description}")]
        public string classDescriptionFormat = "{0}";

        [Header("UI Elements")]
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;
        public TextWrapper uiTextExp;
        public Image imageExpGage;
        public TextWrapper uiTextHp;
        public Image imageHpGage;
        public TextWrapper uiTextMp;
        public Image imageMpGage;
        public TextWrapper uiTextStamina;
        public Image imageStaminaGage;
        public TextWrapper uiTextFood;
        public Image imageFoodGage;
        public TextWrapper uiTextWater;
        public Image imageWaterGage;
        public TextWrapper uiTextStatPoint;
        public TextWrapper uiTextSkillPoint;
        public TextWrapper uiTextGold;
        public TextWrapper uiTextWeightLimit;
        public TextWrapper uiTextWeaponDamages;
        public UIDamageElementAmounts uiRightHandDamages;
        public UIDamageElementAmounts uiLeftHandDamages;
        public UICharacterStats uiCharacterStats;
        public UICharacterBuffs uiCharacterBuffs;
        public UIResistanceAmounts uiCharacterResistances;
        public UICharacterAttributePair[] uiCharacterAttributes;
        [Header("Class information")]
        public TextWrapper uiTextClassTitle;
        public TextWrapper uiTextClassDescription;
        public Image imageClassIcon;
        [Header("Options")]
        public bool showStatsWithBuffs;
        public bool showAttributeWithBuffs;

        // Improve garbage collector
        private CharacterStats cacheStatsWithBuffs;
        private Dictionary<DamageElement, float> cacheResistancesWithBuffs;
        private Dictionary<Attribute, short> cacheAttributesWithBuffs;
        private CharacterStats displayingStats;
        private Dictionary<DamageElement, float> displayingResistances;
        private Dictionary<Attribute, short> displayingAttributes;

        private Dictionary<Attribute, UICharacterAttribute> cacheUICharacterAttributes;
        public Dictionary<Attribute, UICharacterAttribute> CacheUICharacterAttributes
        {
            get
            {
                if (cacheUICharacterAttributes == null)
                {
                    cacheUICharacterAttributes = new Dictionary<Attribute, UICharacterAttribute>();
                    foreach (UICharacterAttributePair uiCharacterAttribute in uiCharacterAttributes)
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

        protected override void Update()
        {
            base.Update();

            Profiler.BeginSample("UICharacter - Update UI (Immediately)");
            // Hp
            int currentHp = 0;
            int maxHp = 0;
            if (Data != null)
            {
                currentHp = Data.CurrentHp;
                maxHp = Data.CacheMaxHp;
            }

            if (uiTextHp != null)
                uiTextHp.text = string.Format(hpFormat, currentHp.ToString("N0"), maxHp.ToString("N0"));

            if (imageHpGage != null)
                imageHpGage.fillAmount = maxHp <= 0 ? 0 : (float)currentHp / (float)maxHp;

            // Mp
            int currentMp = 0;
            int maxMp = 0;
            if (Data != null)
            {
                currentMp = Data.CurrentMp;
                maxMp = Data.CacheMaxMp;
            }

            if (uiTextMp != null)
                uiTextMp.text = string.Format(mpFormat, currentMp.ToString("N0"), maxMp.ToString("N0"));

            if (imageMpGage != null)
                imageMpGage.fillAmount = maxMp <= 0 ? 0 : (float)currentMp / (float)maxMp;

            // Stamina
            int currentStamina = 0;
            int maxStamina = 0;
            if (Data != null)
            {
                currentStamina = Data.CurrentStamina;
                maxStamina = Data.CacheMaxStamina;
            }

            if (uiTextStamina != null)
                uiTextStamina.text = string.Format(staminaFormat, currentStamina.ToString("N0"), maxStamina.ToString("N0"));

            if (imageStaminaGage != null)
                imageStaminaGage.fillAmount = maxStamina <= 0 ? 0 : (float)currentStamina / (float)maxStamina;

            // Food
            int currentFood = 0;
            int maxFood = 0;
            if (Data != null)
            {
                currentFood = Data.CurrentFood;
                maxFood = Data.CacheMaxFood;
            }

            if (uiTextFood != null)
                uiTextFood.text = string.Format(foodFormat, currentFood.ToString("N0"), maxFood.ToString("N0"));

            if (imageFoodGage != null)
                imageFoodGage.fillAmount = maxFood <= 0 ? 0 : (float)currentFood / (float)maxFood;

            // Water
            int currentWater = 0;
            int maxWater = 0;
            if (Data != null)
            {
                currentWater = Data.CurrentWater;
                maxWater = Data.CacheMaxWater;
            }

            if (uiTextWater != null)
                uiTextWater.text = string.Format(waterFormat, currentWater.ToString("N0"), maxWater.ToString("N0"));

            if (imageWaterGage != null)
                imageWaterGage.fillAmount = maxWater <= 0 ? 0 : (float)currentWater / (float)maxWater;

            Profiler.EndSample();
        }

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UICharacter - Update UI");

            if (uiTextName != null)
                uiTextName.text = string.Format(nameFormat, Data == null ? LanguageManager.GetUnknowTitle() : Data.CharacterName);

            if (uiTextLevel != null)
                uiTextLevel.text = string.Format(levelFormat, Data == null ? "0" : Data.Level.ToString("N0"));
            
            int[] expTree = GameInstance.Singleton.ExpTree;
            int currentExp = 0;
            int nextLevelExp = 0;
            if (Data != null && Data.GetNextLevelExp() > 0)
            {
                currentExp = Data.Exp;
                nextLevelExp = Data.GetNextLevelExp();
            }
            else if (Data != null && Data.Level - 2 > 0 && Data.Level - 2 < expTree.Length)
            {
                int maxExp = expTree[Data.Level - 2];
                currentExp = maxExp;
                nextLevelExp = maxExp;
            }

            if (uiTextExp != null)
                uiTextExp.text = string.Format(expFormat, currentExp.ToString("N0"), nextLevelExp.ToString("N0"));

            if (imageExpGage != null)
                imageExpGage.fillAmount = nextLevelExp <= 0 ? 1 : (float)currentExp / (float)nextLevelExp;
            
            // Player character data
            IPlayerCharacterData playerCharacter = Data as IPlayerCharacterData;
            if (uiTextStatPoint != null)
                uiTextStatPoint.text = string.Format(statPointFormat, playerCharacter == null ? "0" : playerCharacter.StatPoint.ToString("N0"));

            if (uiTextSkillPoint != null)
                uiTextSkillPoint.text = string.Format(skillPointFormat, playerCharacter == null ? "0" : playerCharacter.SkillPoint.ToString("N0"));

            if (uiTextGold != null)
                uiTextGold.text = string.Format(goldFormat, playerCharacter == null ? "0" : playerCharacter.Gold.ToString("N0"));

            BaseCharacter character = Data == null ? null : Data.GetDatabase();
            if (uiTextClassTitle != null)
                uiTextClassTitle.text = string.Format(classTitleFormat, character == null ? LanguageManager.GetUnknowTitle() : character.Title);

            if (uiTextClassDescription != null)
                uiTextClassDescription.text = string.Format(classDescriptionFormat, character == null ? LanguageManager.GetUnknowDescription() : character.Description);

            if (imageClassIcon != null)
            {
                Sprite iconSprite = character == null ? null : character.icon;
                imageClassIcon.gameObject.SetActive(iconSprite != null);
                imageClassIcon.sprite = iconSprite;
            }

            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
            cacheStatsWithBuffs = Data.GetStats();
            cacheResistancesWithBuffs = Data.GetResistances();
            cacheAttributesWithBuffs = Data.GetAttributes();
            displayingStats = showStatsWithBuffs ? cacheStatsWithBuffs : Data.GetStats(true, false);
            displayingResistances = showStatsWithBuffs ? cacheResistancesWithBuffs : Data.GetResistances(true, false);
            displayingAttributes = showAttributeWithBuffs ? cacheAttributesWithBuffs : Data.GetAttributes(true, false);

            if (uiTextWeightLimit != null)
                uiTextWeightLimit.text = string.Format(weightLimitStatsFormat, Data.GetTotalItemWeight().ToString("N2"), cacheStatsWithBuffs.weightLimit.ToString("N2"));

            CharacterItem rightHandItem = Data.EquipWeapons.rightHand;
            CharacterItem leftHandItem = Data.EquipWeapons.leftHand;
            Item rightHandWeapon = rightHandItem.GetWeaponItem();
            Item leftHandWeapon = leftHandItem.GetWeaponItem();
            Dictionary<DamageElement, MinMaxFloat> rightHandDamages = rightHandWeapon != null ? GameDataHelpers.CombineDamages(Data.GetIncreaseDamages(), rightHandWeapon.GetDamageAmount(rightHandItem.level, rightHandItem.GetEquipmentBonusRate(), Data)) : null;
            Dictionary<DamageElement, MinMaxFloat> leftHandDamages = leftHandWeapon != null ? GameDataHelpers.CombineDamages(Data.GetIncreaseDamages(), leftHandWeapon.GetDamageAmount(leftHandItem.level, leftHandItem.GetEquipmentBonusRate(), Data)) : null;

            if (uiTextWeaponDamages != null)
            {
                string textDamages = "";
                if (rightHandWeapon != null)
                {
                    MinMaxFloat sumDamages = GameDataHelpers.GetSumDamages(rightHandDamages);
                    if (!string.IsNullOrEmpty(textDamages))
                        textDamages += "\n";
                    textDamages += string.Format(weaponDamageFormat, sumDamages.min.ToString("N0"), sumDamages.max.ToString("N0"));
                }
                if (leftHandWeapon != null)
                {
                    MinMaxFloat sumDamages = GameDataHelpers.GetSumDamages(leftHandDamages);
                    if (!string.IsNullOrEmpty(textDamages))
                        textDamages += "\n";
                    textDamages += string.Format(weaponDamageFormat, sumDamages.min.ToString("N0"), sumDamages.max.ToString("N0"));
                }
                if (rightHandWeapon == null && leftHandWeapon == null)
                {
                    Item defaultWeaponItem = GameInstance.Singleton.DefaultWeaponItem;
                    WeaponItemEquipType defaultWeaponItemType = defaultWeaponItem.EquipType;
                    KeyValuePair<DamageElement, MinMaxFloat> damageAmount = defaultWeaponItem.GetDamageAmount(1, 1f, Data);
                    textDamages = string.Format(weaponDamageFormat, damageAmount.Value.min.ToString("N0"), damageAmount.Value.max.ToString("N0"));
                }
                uiTextWeaponDamages.text = textDamages;
            }

            if (uiRightHandDamages != null)
            {
                if (rightHandWeapon == null)
                    uiRightHandDamages.Hide();
                else
                {
                    uiRightHandDamages.Show();
                    uiRightHandDamages.Data = rightHandDamages;
                }
            }

            if (uiLeftHandDamages != null)
            {
                if (leftHandWeapon == null)
                    uiLeftHandDamages.Hide();
                else
                {
                    uiLeftHandDamages.Show();
                    uiLeftHandDamages.Data = leftHandDamages;
                }
            }

            if (uiCharacterStats != null)
                uiCharacterStats.Data = displayingStats;

            if (uiCharacterResistances != null)
                uiCharacterResistances.Data = displayingResistances;

            if (CacheUICharacterAttributes.Count > 0 && Data != null)
            {
                CharacterAttribute tempCharacterAttribute;
                Attribute tempAttribute;
                short tempAmount;
                IList<CharacterAttribute> characterAttributes = Data.Attributes;
                for (int indexOfData = 0; indexOfData < characterAttributes.Count; ++indexOfData)
                {
                    tempCharacterAttribute = characterAttributes[indexOfData];
                    tempAttribute = tempCharacterAttribute.GetAttribute();
                    UICharacterAttribute cacheUICharacterAttribute;
                    tempAmount = 0;
                    if (CacheUICharacterAttributes.TryGetValue(tempAttribute, out cacheUICharacterAttribute))
                    {
                        if (displayingAttributes.ContainsKey(tempAttribute))
                            tempAmount = displayingAttributes[tempAttribute];
                        cacheUICharacterAttribute.Setup(new CharacterAttributeTuple(tempCharacterAttribute, tempAmount), Data, indexOfData);
                        cacheUICharacterAttribute.Show();
                    }
                }
            }

            if (uiCharacterBuffs != null)
                uiCharacterBuffs.UpdateData(Data);
        }
    }
}
