using System.Collections.Generic;
using Cysharp.Text;
using LiteNetLibManager;
using UnityEngine;
using UnityEngine.Profiling;

namespace MultiplayerARPG
{
    public partial class UICharacter : UISelectionEntry<ICharacterData>
    {
        [Header("String Formats")]
        [Tooltip("Format => {0} = {Character Id}")]
        public UILocaleKeySetting formatKeyId = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Character Name}")]
        public UILocaleKeySetting formatKeyName = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {Stat Points}")]
        public UILocaleKeySetting formatKeyStatPoint = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_STAT_POINTS);
        [Tooltip("Format => {0} = {Skill Points}")]
        public UILocaleKeySetting formatKeySkillPoint = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_POINTS);
        [Tooltip("Format => {0} = {Battle Points}")]
        public UILocaleKeySetting formatKeyBattlePoint = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_BATTLE_POINTS);
        [Tooltip("Format => {0} = {Gold Amount}")]
        public UILocaleKeySetting formatKeyGold = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_GOLD);
        [Tooltip("Format => {0} = {Current Total Weights}, {1} = {Weight Limit}")]
        public UILocaleKeySetting formatKeyWeightLimitStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_WEIGHT);
        [Tooltip("Format => {0} = {Current Total Slots}, {1} = {Slot Limit}")]
        public UILocaleKeySetting formatKeySlotLimitStats = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CURRENT_SLOT);
        [Tooltip("Format => {0} = {Min Damage}, {1} = {Max Damage}")]
        public UILocaleKeySetting formatKeyWeaponDamage = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_DAMAGE_AMOUNT);

        [Header("UI Elements")]
        public TextWrapper uiTextId;
        public TextWrapper uiTextName;
        public TextWrapper uiTextLevel;
        public UIGageValue uiGageExp;
        public UIGageValue uiGageHp;
        public UIGageValue uiGageMp;
        public UIGageValue uiGageStamina;
        public UIGageValue uiGageFood;
        public UIGageValue uiGageWater;
        public TextWrapper uiTextStatPoint;
        public TextWrapper uiTextSkillPoint;
        public TextWrapper uiTextBattlePoint;
        public TextWrapper uiTextGold;
        public TextWrapper uiTextWeightLimit;
        public TextWrapper uiTextSlotLimit;
        public TextWrapper uiTextWeaponDamages;
        public UIDamageElementAmounts uiRightHandDamages;
        public UIDamageElementAmounts uiLeftHandDamages;
        public UICharacterStats uiCharacterStats;
        public UICharacterBuffs uiCharacterBuffs;
        public UIResistanceAmounts uiCharacterResistances;
        public UIDamageElementAmounts uiCharacterElementalDamages;
        public UIArmorAmounts uiCharacterArmors;
        public UICharacterAttributePair[] uiCharacterAttributes;
        public UICharacterCurrencyPair[] uiCharacterCurrencies;
        public UICharacterClass uiCharacterClass;
        public UIPlayerIcon uiPlayerIcon;
        public UIPlayerFrame uiPlayerFrame;
        public UIPlayerTitle uiPlayerTitle;

        [Header("Options")]
        [Tooltip("If this is `TRUE` it won't update data when controlling character's data changes")]
        public bool notForOwningCharacter;
        [Tooltip("If this is `TRUE` it will show stats which sum with buffs")]
        public bool showStatsWithBuffs;
        [Tooltip("If this is `TRUE` it will show attributes which sum with buffs")]
        public bool showAttributeWithBuffs;
        [Tooltip("If this is `TRUE` it will show resistances which sum with buffs")]
        public bool showResistanceWithBuffs;
        [Tooltip("If this is `TRUE` it will show armors which sum with buffs")]
        public bool showArmorWithBuffs;
        [Tooltip("If this is `TRUE` it will show damages which sum with buffs")]
        public bool showDamageWithBuffs;

        // Improve garbage collector
        private Dictionary<BaseSkill, int> cacheSkills;
        private CharacterStats cacheStats;
        private Dictionary<Attribute, float> cacheAttributes;
        private Dictionary<DamageElement, float> cacheResistances;
        private Dictionary<DamageElement, float> cacheArmors;
        private Dictionary<DamageElement, MinMaxFloat> cacheDamages;
        private Dictionary<EquipmentSet, int> cacheEquipmentSets;

        public bool NotForOwningCharacter
        {
            get { return notForOwningCharacter; }
            set
            {
                notForOwningCharacter = value;
                RegisterOwningCharacterEvents();
            }
        }

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

        private Dictionary<Currency, UICharacterCurrency> cacheUICharacterCurrencies;
        public Dictionary<Currency, UICharacterCurrency> CacheUICharacterCurrencies
        {
            get
            {
                if (cacheUICharacterCurrencies == null)
                {
                    cacheUICharacterCurrencies = new Dictionary<Currency, UICharacterCurrency>();
                    foreach (UICharacterCurrencyPair uiCharacterCurrency in uiCharacterCurrencies)
                    {
                        if (uiCharacterCurrency.currency != null &&
                            uiCharacterCurrency.ui != null &&
                            !cacheUICharacterCurrencies.ContainsKey(uiCharacterCurrency.currency))
                            cacheUICharacterCurrencies.Add(uiCharacterCurrency.currency, uiCharacterCurrency.ui);
                    }
                }
                return cacheUICharacterCurrencies;
            }
        }

        protected override void OnEnable()
        {
            UpdateOwningCharacterData();
            RegisterOwningCharacterEvents();
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            UnregisterOwningCharacterEvents();
            base.OnDisable();
        }

        public void RegisterOwningCharacterEvents()
        {
            UnregisterOwningCharacterEvents();
            if (notForOwningCharacter || !GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onDataIdChange += OnDataIdChange;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onAttributesOperation += OnAttributesOperation;
            GameInstance.PlayingCharacterEntity.onSkillsOperation += OnSkillsOperation;
            GameInstance.PlayingCharacterEntity.onSummonsOperation += OnSummonsOperation;
            GameInstance.PlayingCharacterEntity.onBuffsOperation += OnBuffsOperation;
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onCurrenciesOperation += OnCurrenciesOperation;
        }

        public void UnregisterOwningCharacterEvents()
        {
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onDataIdChange -= OnDataIdChange;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onAttributesOperation -= OnAttributesOperation;
            GameInstance.PlayingCharacterEntity.onSkillsOperation -= OnSkillsOperation;
            GameInstance.PlayingCharacterEntity.onSummonsOperation -= OnSummonsOperation;
            GameInstance.PlayingCharacterEntity.onBuffsOperation -= OnBuffsOperation;
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onCurrenciesOperation -= OnCurrenciesOperation;
        }

        private void OnDataIdChange(int dataId)
        {
            UpdateOwningCharacterData();
        }

        private void OnEquipWeaponSetChange(byte equipWeaponSet)
        {
            UpdateOwningCharacterData();
        }

        private void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnAttributesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnSummonsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnBuffsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnCurrenciesOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        public void UpdateOwningCharacterData()
        {
            if (notForOwningCharacter || GameInstance.PlayingCharacter == null) return;
            Data = GameInstance.PlayingCharacter;
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

            if (uiTextId != null)
            {
                uiTextId.text = ZString.Format(
                    LanguageManager.GetText(formatKeyId),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Id);
            }

            if (uiTextName != null)
            {
                uiTextName.text = ZString.Format(
                    LanguageManager.GetText(formatKeyName),
                    Data == null ? LanguageManager.GetUnknowTitle() : Data.Title);
            }

            if (uiTextLevel != null)
            {
                uiTextLevel.text = ZString.Format(
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
                uiTextStatPoint.text = ZString.Format(
                    LanguageManager.GetText(formatKeyStatPoint),
                    playerCharacter == null ? "0" : playerCharacter.StatPoint.ToString("N0"));
            }

            if (uiTextSkillPoint != null)
            {
                uiTextSkillPoint.text = ZString.Format(
                    LanguageManager.GetText(formatKeySkillPoint),
                    playerCharacter == null ? "0" : playerCharacter.SkillPoint.ToString("N0"));
            }

            if (uiTextBattlePoint != null)
            {
                uiTextBattlePoint.text = ZString.Format(
                    LanguageManager.GetText(formatKeyBattlePoint),
                    playerCharacter == null ? "0" : playerCharacter.GetCaches().BattlePoints.ToString("N0"));
            }

            if (uiTextGold != null)
            {
                uiTextGold.text = ZString.Format(
                    LanguageManager.GetText(formatKeyGold),
                    playerCharacter == null ? "0" : playerCharacter.Gold.ToString("N0"));
            }

            // Icon
            if (uiPlayerIcon != null)
            {
                if (playerCharacter != null)
                    uiPlayerIcon.SetDataByDataId(playerCharacter.IconDataId);
                uiPlayerIcon.SetVisible(playerCharacter != null);
            }

            // Frame
            if (uiPlayerFrame != null)
            {
                if (playerCharacter != null)
                    uiPlayerFrame.SetDataByDataId(playerCharacter.FrameDataId);
                uiPlayerFrame.SetVisible(playerCharacter != null);
            }

            // Title
            if (uiPlayerTitle != null)
            {
                if (playerCharacter != null)
                    uiPlayerTitle.SetDataByDataId(playerCharacter.TitleDataId);
                uiPlayerTitle.SetVisible(playerCharacter != null);
            }

            Profiler.EndSample();
        }

        protected override void UpdateData()
        {
            IPlayerCharacterData playerCharacter = Data as IPlayerCharacterData;

            cacheStats = new CharacterStats();
            cacheAttributes = new Dictionary<Attribute, float>();
            cacheResistances = new Dictionary<DamageElement, float>();
            cacheArmors = new Dictionary<DamageElement, float>();
            cacheDamages = new Dictionary<DamageElement, MinMaxFloat>();
            cacheSkills = new Dictionary<BaseSkill, int>();
            cacheEquipmentSets = new Dictionary<EquipmentSet, int>();
            cacheSkills = GameDataHelpers.CombineSkills(cacheSkills, Data.GetSkills(true));
            cacheAttributes = GameDataHelpers.CombineAttributes(cacheAttributes, showAttributeWithBuffs ? Data.GetAttributes(true, true, cacheSkills) : Data.GetAttributes(true, false, null));
            cacheResistances = GameDataHelpers.CombineResistances(cacheResistances, showResistanceWithBuffs ? Data.GetResistances(true, true, cacheAttributes, cacheSkills) : Data.GetResistances(true, false, null, null));
            cacheArmors = GameDataHelpers.CombineArmors(cacheArmors, showArmorWithBuffs ? Data.GetArmors(true, true, cacheAttributes, cacheSkills) : Data.GetArmors(true, false, null, null));
            cacheDamages = GameDataHelpers.CombineDamages(cacheDamages, showDamageWithBuffs ? Data.GetIncreaseDamages(true, true, cacheAttributes, cacheSkills) : Data.GetIncreaseDamages(true, false, null, null));
            cacheStats = cacheStats + (showStatsWithBuffs ? Data.GetStats(true, true, cacheSkills) : Data.GetStats(true, false, null));
            Data.GetEquipmentSetBonus(ref cacheStats, cacheAttributes, cacheResistances, cacheArmors, cacheDamages, cacheSkills, cacheEquipmentSets, true);

            if (uiTextWeightLimit != null)
            {
                uiTextWeightLimit.text = ZString.Format(
                    LanguageManager.GetText(formatKeyWeightLimitStats),
                    Data.GetCaches().TotalItemWeight.ToString("N2"),
                    Data.GetCaches().LimitItemWeight.ToString("N2"));
            }

            if (uiTextSlotLimit != null)
            {
                uiTextSlotLimit.text = ZString.Format(
                    LanguageManager.GetText(formatKeySlotLimitStats),
                    Data.GetCaches().TotalItemSlot.ToString("N0"),
                    Data.GetCaches().LimitItemSlot.ToString("N0"));
            }

            CharacterItem rightHandItem = Data.EquipWeapons.rightHand;
            CharacterItem leftHandItem = Data.EquipWeapons.leftHand;
            IWeaponItem rightHandWeapon = rightHandItem.GetWeaponItem();
            IWeaponItem leftHandWeapon = leftHandItem.GetWeaponItem();
            Dictionary<DamageElement, MinMaxFloat> rightHandDamages = null;
            Dictionary<DamageElement, MinMaxFloat> leftHandDamages = null;
            if (rightHandWeapon != null)
            {
                rightHandDamages = GameDataHelpers.CombineDamages(null, cacheDamages);
                rightHandDamages = GameDataHelpers.CombineDamages(rightHandDamages, rightHandItem.GetDamageAmount(Data));
            }
            if (leftHandWeapon != null)
            {
                leftHandDamages = GameDataHelpers.CombineDamages(null, cacheDamages);
                leftHandDamages = GameDataHelpers.CombineDamages(leftHandDamages, leftHandItem.GetDamageAmount(Data));
            }

            if (uiTextWeaponDamages != null)
            {
                using (Utf16ValueStringBuilder textDamages = ZString.CreateStringBuilder(false))
                {
                    if (rightHandWeapon != null)
                    {
                        MinMaxFloat sumDamages = GameDataHelpers.GetSumDamages(rightHandDamages);
                        if (textDamages.Length > 0)
                            textDamages.Append('\n');
                        textDamages.AppendFormat(
                            LanguageManager.GetText(formatKeyWeaponDamage),
                            sumDamages.min.ToString("N0"),
                            sumDamages.max.ToString("N0"));
                    }
                    if (leftHandWeapon != null)
                    {
                        MinMaxFloat sumDamages = GameDataHelpers.GetSumDamages(leftHandDamages);
                        if (textDamages.Length > 0)
                            textDamages.Append('\n');
                        textDamages.AppendFormat(
                            LanguageManager.GetText(formatKeyWeaponDamage),
                            sumDamages.min.ToString("N0"),
                            sumDamages.max.ToString("N0"));
                    }
                    if (rightHandWeapon == null && leftHandWeapon == null)
                    {
                        IWeaponItem defaultWeaponItem = GameInstance.Singleton.DefaultWeaponItem;
                        KeyValuePair<DamageElement, MinMaxFloat> damageAmount = defaultWeaponItem.GetDamageAmount(1, 1f, Data);
                        textDamages.AppendFormat(
                            LanguageManager.GetText(formatKeyWeaponDamage),
                            damageAmount.Value.min.ToString("N0"),
                            damageAmount.Value.max.ToString("N0"));
                    }
                    uiTextWeaponDamages.text = textDamages.ToString();
                }
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

            if (uiCharacterElementalDamages != null)
            {
                uiCharacterElementalDamages.isBonus = false;
                uiCharacterElementalDamages.Data = cacheDamages;
            }

            if (uiCharacterArmors != null)
            {
                uiCharacterArmors.isBonus = false;
                uiCharacterArmors.Data = cacheArmors;
            }

            if (CacheUICharacterAttributes.Count > 0 && Data != null)
            {
                int tempIndexOfAttribute;
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

            if (CacheUICharacterCurrencies.Count > 0 && playerCharacter != null)
            {
                int tempIndexOfCurrency;
                CharacterCurrency tempCharacterCurrency;
                foreach (Currency currency in CacheUICharacterCurrencies.Keys)
                {
                    tempIndexOfCurrency = playerCharacter.IndexOfCurrency(currency.DataId);
                    tempCharacterCurrency = tempIndexOfCurrency >= 0 ? playerCharacter.Currencies[tempIndexOfCurrency] : CharacterCurrency.Create(currency, 0);
                    CacheUICharacterCurrencies[currency].Setup(new UICharacterCurrencyData(tempCharacterCurrency, tempCharacterCurrency.amount), Data, tempIndexOfCurrency);
                    CacheUICharacterCurrencies[currency].Show();
                }
            }

            if (uiCharacterBuffs != null)
                uiCharacterBuffs.UpdateData(Data);

            BaseCharacter character = Data == null ? null : Data.GetDatabase();
            if (uiCharacterClass != null)
                uiCharacterClass.Data = character;
        }
    }
}
