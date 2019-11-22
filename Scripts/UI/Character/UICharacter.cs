using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public partial class UICharacter : UISelectionEntry<ICharacterData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Name}")]
        public UILocaleKeySetting formatKeyName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {Stat Points}")]
        public UILocaleKeySetting formatKeyStatPoint = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAT_POINTS);
        [Tooltip("Format => {0} = {Skill Points}")]
        public UILocaleKeySetting formatKeySkillPoint = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_POINTS);
        [Tooltip("Format => {0} = {Gold Amount}")]
        public UILocaleKeySetting formatKeyGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);
        [Tooltip("Format => {0} = {Current Total Weights}, {1} = {Weight Limit}")]
        public UILocaleKeySetting formatKeyWeightLimitStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_WEIGHT);
        [Tooltip("Format => {0} = {Min Damage}, {1} = {Max Damage}")]
        public UILocaleKeySetting formatKeyWeaponDamage = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_AMOUNT);

        [Header("UI Elements")]
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;
        // EXP
        public UIGageValue uiGageExp;
        // HP
        public UIGageValue uiGageHp;
        // MP
        public UIGageValue uiGageMp;
        // Stamina
        public UIGageValue uiGageStamina;
        // Food
        public UIGageValue uiGageFood;
        // Water
        public UIGageValue uiGageWater;

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
        public UIArmorAmounts uiCharacterArmors;
        public UICharacterAttributePair[] uiCharacterAttributes;
        public UICharacterClass uiCharacterClass;

        [Header("Options")]
        public bool showStatsWithBuffs;
        public bool showAttributeWithBuffs;
        public bool showResistanceWithBuffs;
        public bool showArmorWithBuffs;
        public bool showDamageWithBuffs;

        // Improve garbage collector
        private CharacterStats cacheStats;
        private Dictionary<Attribute, float> cacheAttributes;
        private Dictionary<DamageElement, float> cacheResistances;
        private Dictionary<DamageElement, float> cacheArmors;
        private Dictionary<DamageElement, MinMaxFloat> cacheDamages;
        private Dictionary<EquipmentSet, int> cacheEquipmentSets;
        // Cache bonus data
        private CharacterStats bonusStats;
        private Dictionary<Attribute, float> bonusAttributes;
        private Dictionary<DamageElement, float> bonusResistances;
        private Dictionary<DamageElement, float> bonusArmors;
        private Dictionary<DamageElement, MinMaxFloat> bonusDamages;
        private Dictionary<BaseSkill, short> bonusSkills;

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
                maxHp = Data.GetCaches().MaxHp;
            }
            if (uiGageHp != null)
                uiGageHp.Update(currentHp, maxHp);

            // Mp
            int currentMp = 0;
            int maxMp = 0;
            if (Data != null)
            {
                currentMp = Data.CurrentMp;
                maxMp = Data.GetCaches().MaxMp;
            }
            if (uiGageMp != null)
                uiGageMp.Update(currentMp, maxMp);

            // Stamina
            int currentStamina = 0;
            int maxStamina = 0;
            if (Data != null)
            {
                currentStamina = Data.CurrentStamina;
                maxStamina = Data.GetCaches().MaxStamina;
            }
            if (uiGageStamina != null)
                uiGageStamina.Update(currentStamina, maxStamina);

            // Food
            int currentFood = 0;
            int maxFood = 0;
            if (Data != null)
            {
                currentFood = Data.CurrentFood;
                maxFood = Data.GetCaches().MaxFood;
            }
            if (uiGageFood != null)
                uiGageFood.Update(currentFood, maxFood);

            // Water
            int currentWater = 0;
            int maxWater = 0;
            if (Data != null)
            {
                currentWater = Data.CurrentWater;
                maxWater = Data.GetCaches().MaxWater;
            }
            if (uiGageWater != null)
                uiGageWater.Update(currentWater, maxWater);

            Profiler.EndSample();
        }

        protected override void UpdateUI()
        {
            Profiler.BeginSample("UICharacter - Update UI");

            if (uiTextName != null)
            {
                uiTextName.text = string.Format(
                    LanguageManager.GetText(formatKeyName),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);
            }

            if (uiTextLevel != null)
            {
                uiTextLevel.text = string.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Data == null ? "1" : Data.Level.ToString("N0"));
            }

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
            if (uiGageExp != null)
                uiGageExp.Update(currentExp, nextLevelExp);

            // Player character data
            IPlayerCharacterData playerCharacter = Data as IPlayerCharacterData;
            if (uiTextStatPoint != null)
            {
                uiTextStatPoint.text = string.Format(
                    LanguageManager.GetText(formatKeyStatPoint),
                    playerCharacter == null ? "0" : playerCharacter.StatPoint.ToString("N0"));
            }

            if (uiTextSkillPoint != null)
            {
                uiTextSkillPoint.text = string.Format(
                    LanguageManager.GetText(formatKeySkillPoint),
                    playerCharacter == null ? "0" : playerCharacter.SkillPoint.ToString("N0"));
            }

            if (uiTextGold != null)
            {
                uiTextGold.text = string.Format(
                    LanguageManager.GetText(formatKeyGold),
                    playerCharacter == null ? "0" : playerCharacter.Gold.ToString("N0"));
            }

            BaseCharacter character = Data == null ? null : Data.GetDatabase();
            if (uiCharacterClass != null)
                uiCharacterClass.Data = character;

            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
            cacheStats = showStatsWithBuffs ? Data.GetStats() : Data.GetStats(true, false);
            cacheAttributes = showAttributeWithBuffs ? Data.GetAttributes() : Data.GetAttributes(true, false);
            cacheResistances = showResistanceWithBuffs ? Data.GetResistances() : Data.GetResistances(true, false);
            cacheArmors = showArmorWithBuffs ? Data.GetArmors() : Data.GetArmors(true, false);
            cacheDamages = showDamageWithBuffs ? Data.GetIncreaseDamages() : Data.GetIncreaseDamages(true, false);

            if (!showStatsWithBuffs)
            {
                // Prepare base stats, it will be multiplied with increase stats rate
                CharacterStats baseStats = new CharacterStats();
                if (Data.GetDatabase() != null)
                    baseStats += Data.GetDatabase().GetCharacterStats(Data.Level);
                Dictionary<Attribute, float> baseAttributes = Data.GetCharacterAttributes();
                baseStats += GameDataHelpers.GetStatsFromAttributes(baseAttributes);
            }

            if (bonusAttributes == null)
                bonusAttributes = new Dictionary<Attribute, float>();
            if (bonusResistances == null)
                bonusResistances = new Dictionary<DamageElement, float>();
            if (bonusArmors == null)
                bonusArmors = new Dictionary<DamageElement, float>();
            if (bonusDamages == null)
                bonusDamages = new Dictionary<DamageElement, MinMaxFloat>();
            if (bonusSkills == null)
                bonusSkills = new Dictionary<BaseSkill, short>();
            if (cacheEquipmentSets == null)
                cacheEquipmentSets = new Dictionary<EquipmentSet, int>();

            Data.GetEquipmentSetBonus(ref bonusStats, bonusAttributes, bonusResistances, bonusArmors, bonusDamages, bonusSkills, cacheEquipmentSets);
            // Increase stats by equipment set bonus
            cacheStats += bonusStats;
            cacheAttributes = GameDataHelpers.CombineAttributes(cacheAttributes, bonusAttributes);
            cacheResistances = GameDataHelpers.CombineResistances(cacheResistances, bonusResistances);
            cacheArmors = GameDataHelpers.CombineArmors(cacheArmors, bonusArmors);
            cacheDamages = GameDataHelpers.CombineDamages(cacheDamages, bonusDamages);

            if (uiTextWeightLimit != null)
            {
                uiTextWeightLimit.text = string.Format(
                    LanguageManager.GetText(formatKeyWeightLimitStats),
                    Data.GetCaches().TotalItemWeight.ToString("N2"),
                    Data.GetCaches().Stats.weightLimit.ToString("N2"));
            }

            CharacterItem rightHandItem = Data.EquipWeapons.rightHand;
            CharacterItem leftHandItem = Data.EquipWeapons.leftHand;
            Item rightHandWeapon = rightHandItem.GetWeaponItem();
            Item leftHandWeapon = leftHandItem.GetWeaponItem();
            Dictionary<DamageElement, MinMaxFloat> rightHandDamages = rightHandWeapon != null ? GameDataHelpers.CombineDamages(cacheDamages, rightHandItem.GetDamageAmount(Data)) : null;
            Dictionary<DamageElement, MinMaxFloat> leftHandDamages = leftHandWeapon != null ? GameDataHelpers.CombineDamages(cacheDamages, leftHandItem.GetDamageAmount(Data)) : null;

            if (uiTextWeaponDamages != null)
            {
                string textDamages = "";
                if (rightHandWeapon != null)
                {
                    MinMaxFloat sumDamages = GameDataHelpers.GetSumDamages(rightHandDamages);
                    if (!string.IsNullOrEmpty(textDamages))
                        textDamages += "\n";
                    textDamages += string.Format(
                        LanguageManager.GetText(formatKeyWeaponDamage),
                        sumDamages.min.ToString("N0"),
                        sumDamages.max.ToString("N0"));
                }
                if (leftHandWeapon != null)
                {
                    MinMaxFloat sumDamages = GameDataHelpers.GetSumDamages(leftHandDamages);
                    if (!string.IsNullOrEmpty(textDamages))
                        textDamages += "\n";
                    textDamages += string.Format(
                        LanguageManager.GetText(formatKeyWeaponDamage),
                        sumDamages.min.ToString("N0"),
                        sumDamages.max.ToString("N0"));
                }
                if (rightHandWeapon == null && leftHandWeapon == null)
                {
                    Item defaultWeaponItem = GameInstance.Singleton.DefaultWeaponItem;
                    WeaponItemEquipType defaultWeaponItemType = defaultWeaponItem.EquipType;
                    KeyValuePair<DamageElement, MinMaxFloat> damageAmount = defaultWeaponItem.GetDamageAmount(1, 1f, Data);
                    textDamages = string.Format(
                        LanguageManager.GetText(formatKeyWeaponDamage),
                        damageAmount.Value.min.ToString("N0"),
                        damageAmount.Value.max.ToString("N0"));
                }
                uiTextWeaponDamages.text = textDamages;
            }

            if (uiRightHandDamages != null)
            {
                if (rightHandWeapon == null)
                {
                    uiRightHandDamages.Hide();
                }
                else
                {
                    uiRightHandDamages.isBonus = false;
                    uiRightHandDamages.Show();
                    uiRightHandDamages.Data = rightHandDamages;
                }
            }

            if (uiLeftHandDamages != null)
            {
                if (leftHandWeapon == null)
                {
                    uiLeftHandDamages.Hide();
                }
                else
                {
                    uiLeftHandDamages.isBonus = false;
                    uiLeftHandDamages.Show();
                    uiLeftHandDamages.Data = leftHandDamages;
                }
            }

            if (uiCharacterStats != null)
            {
                uiCharacterStats.displayType = UICharacterStats.DisplayType.Simple;
                uiCharacterStats.isBonus = false;
                uiCharacterStats.Data = cacheStats;
            }

            if (uiCharacterResistances != null)
            {
                uiCharacterResistances.isBonus = false;
                uiCharacterResistances.Data = cacheResistances;
            }

            if (uiCharacterArmors != null)
            {
                uiCharacterArmors.isBonus = false;
                uiCharacterArmors.Data = cacheArmors;
            }

            if (CacheUICharacterAttributes.Count > 0 && Data != null)
            {
                int tempIndexOfAttribute = -1;
                CharacterAttribute tempCharacterAttribute;
                float tempAmount;
                foreach (Attribute attribute in CacheUICharacterAttributes.Keys)
                {
                    tempIndexOfAttribute = Data.IndexOfAttribute(attribute.DataId);
                    tempCharacterAttribute = tempIndexOfAttribute >= 0 ? Data.Attributes[tempIndexOfAttribute] : CharacterAttribute.Create(attribute, 0);
                    tempAmount = 0;
                    if (cacheAttributes.ContainsKey(attribute))
                        tempAmount = cacheAttributes[attribute];
                    CacheUICharacterAttributes[attribute].Setup(new UICharacterAttributeData(tempCharacterAttribute, tempAmount), Data, tempIndexOfAttribute);
                    CacheUICharacterAttributes[attribute].Show();
                }
            }

            if (uiCharacterBuffs != null)
                uiCharacterBuffs.UpdateData(Data);
        }
    }
}
