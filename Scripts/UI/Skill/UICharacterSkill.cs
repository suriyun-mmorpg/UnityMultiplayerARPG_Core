using Cysharp.Text;
using Insthync.AddressableAssetTools;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterSkill : UIDataForCharacter<UICharacterSkillData>
    {
        public CharacterSkill CharacterSkill { get { return Data.characterSkill; } }
        public int Level { get { return Data.targetLevel; } }
        public BaseSkill Skill { get { return CharacterSkill.GetSkill(); } }

        [Header("String Formats")]
        [Tooltip("Format => {0} = {Title}")]
        public UILocaleKeySetting formatKeyTitle = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Description}")]
        public UILocaleKeySetting formatKeyDescription = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Level}")]
        public UILocaleKeySetting formatKeyLevel = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_LEVEL);
        [Tooltip("Format => {0} = {List Of Weapon Type}")]
        public UILocaleKeySetting formatKeyAvailableWeapons = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_AVAILABLE_WEAPONS);
        [Tooltip("Format => {0} = {List Of Armor Type}")]
        public UILocaleKeySetting formatKeyAvailableArmors = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_AVAILABLE_WEAPONS);
        [Tooltip("Format => {0} = {List Of Vehicle Type}")]
        public UILocaleKeySetting formatKeyAvailableVehicles = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_AVAILABLE_WEAPONS);
        [Tooltip("Format => {0} = {Consume Hp Amount}")]
        public UILocaleKeySetting formatKeyConsumeHp = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CONSUME_HP);
        [Tooltip("Format => {0} = {Consume Mp Amount}")]
        public UILocaleKeySetting formatKeyConsumeMp = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CONSUME_MP);
        [Tooltip("Format => {0} = {Consume Stamina Amount}")]
        public UILocaleKeySetting formatKeyConsumeStamina = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_CONSUME_STAMINA);
        [Tooltip("Format => {0} = {Cooldown Duration}")]
        public UILocaleKeySetting formatKeyCoolDownDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_COOLDOWN_DURATION);
        [Tooltip("Format => {0} = {Cooldown Remains Duration}")]
        public UILocaleKeySetting formatKeyCoolDownRemainsDuration = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SIMPLE);
        [Tooltip("Format => {0} = {Monster Title}, {1} = {Monster Level}, {2} = {Amount}, {3} = {Duration}")]
        public UILocaleKeySetting formatKeySummon = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_SUMMON);
        [Tooltip("Format => {0} = {Mount Title}")]
        public UILocaleKeySetting formatKeyMount = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_MOUNT);
        [Tooltip("Format => {0} = {Weapon Damage Multiplicator}")]
        public UILocaleKeySetting formatKeyWeaponDamageMultiplicator = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_ATTACK_WEAPON_DAMAGE_MULTIPLICATOR);
        [Tooltip("Format => {0} = {Skill Type Title}")]
        public UILocaleKeySetting formatKeySkillType = new UILocaleKeySetting(UIFormatKeys.UI_FORMAT_SKILL_TYPE);

        [Header("UI Elements")]
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public TextWrapper uiTextLevel;
        public Image imageIcon;
        public TextWrapper uiTextSkillType;
        public TextWrapper uiTextAvailableWeapons;
        public TextWrapper uiTextAvailableArmors;
        public TextWrapper uiTextAvailableVehicles;
        public TextWrapper uiTextConsumeHp;
        public TextWrapper uiTextConsumeMp;
        public TextWrapper uiTextConsumeStamina;
        public TextWrapper uiTextCoolDownDuration;
        public TextWrapper uiTextCoolDownRemainsDuration;
        public Image imageCoolDownGage;
        public GameObject[] countDownObjects;
        public GameObject[] noCountDownObjects;
        public UISkillRequirement uiRequirement;
        public TextWrapper uiTextSummon;
        public TextWrapper uiTextMount;
        public UIItemCraft uiCraftItem;

        [Header("Skill Attack")]
        public UIDamageElementAmount uiDamageAmount;
        public UIDamageElementInflictions uiDamageInflictions;
        public TextWrapper uiWeaponDamageMultiplicator;
        public UIDamageElementAmounts uiAdditionalDamageAmounts;
        public UIStatusEffectApplyings uiAttackStatusEffects;

        [Header("Buff/Debuff")]
        public UIBuff uiSkillBuff;
        public UIBuff uiSkillDebuff;

        [Header("Events")]
        public UnityEvent onSetLevelZeroData = new UnityEvent();
        public UnityEvent onSetNonLevelZeroData = new UnityEvent();
        public UnityEvent onAbleToLevelUp = new UnityEvent();
        public UnityEvent onUnableToLevelUp = new UnityEvent();
        public UnityEvent onAbleToUse = new UnityEvent();
        public UnityEvent onUnableToUse = new UnityEvent();

        [Header("Options")]
        public UICharacterSkill uiNextLevelSkill;

        protected float _coolDownRemainsDuration;
        protected bool _dirtyIsCountDown;
        protected bool _dirtyAbleToLevelUp;
        protected bool _dirtyAbleToUse;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            uiTextTitle = null;
            uiTextDescription = null;
            uiTextLevel = null;
            imageIcon = null;
            uiTextSkillType = null;
            uiTextAvailableWeapons = null;
            uiTextAvailableArmors = null;
            uiTextAvailableVehicles = null;
            uiTextConsumeHp = null;
            uiTextConsumeMp = null;
            uiTextConsumeStamina = null;
            uiTextCoolDownDuration = null;
            uiTextCoolDownRemainsDuration = null;
            imageCoolDownGage = null;
            countDownObjects.Nulling();
            noCountDownObjects.Nulling();
            uiRequirement = null;
            uiTextSummon = null;
            uiTextMount = null;
            uiCraftItem = null;
            uiDamageAmount = null;
            uiDamageInflictions = null;
            uiWeaponDamageMultiplicator = null;
            uiAdditionalDamageAmounts = null;
            uiAttackStatusEffects = null;
            uiSkillBuff = null;
            uiSkillDebuff = null;
            onSetLevelZeroData?.RemoveAllListeners();
            onSetLevelZeroData = null;
            onSetNonLevelZeroData?.RemoveAllListeners();
            onSetNonLevelZeroData = null;
            onAbleToLevelUp?.RemoveAllListeners();
            onAbleToLevelUp = null;
            onUnableToLevelUp?.RemoveAllListeners();
            onUnableToLevelUp = null;
            onAbleToUse?.RemoveAllListeners();
            onAbleToUse = null;
            onUnableToUse?.RemoveAllListeners();
            onUnableToUse = null;
            uiNextLevelSkill = null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _coolDownRemainsDuration = 0f;
        }

        protected override void Update()
        {
            base.Update();

            if (_coolDownRemainsDuration > 0f)
            {
                _coolDownRemainsDuration -= Time.deltaTime;
                if (_coolDownRemainsDuration <= 0f)
                    _coolDownRemainsDuration = 0f;
            }
            else
            {
                _coolDownRemainsDuration = 0f;
            }

            // Update UIs
            float coolDownDuration = Skill.GetCoolDownDuration(Level);

            if (uiTextCoolDownDuration != null)
            {
                uiTextCoolDownDuration.SetGameObjectActive(Skill.IsActive && coolDownDuration > 0f);
                uiTextCoolDownDuration.text = ZString.Format(
                    LanguageManager.GetText(formatKeyCoolDownDuration),
                    coolDownDuration.ToString("N0"));
            }

            if (uiTextCoolDownRemainsDuration != null)
            {
                uiTextCoolDownRemainsDuration.SetGameObjectActive(Skill.IsActive && _coolDownRemainsDuration > 0);
                uiTextCoolDownRemainsDuration.text = ZString.Format(
                    LanguageManager.GetText(formatKeyCoolDownRemainsDuration),
                    _coolDownRemainsDuration.ToString("N0"));
            }

            if (imageCoolDownGage != null)
            {
                imageCoolDownGage.fillAmount = coolDownDuration <= 0 ? 0 : _coolDownRemainsDuration / coolDownDuration;
                imageCoolDownGage.gameObject.SetActive(imageCoolDownGage.fillAmount > 0f);
            }

            bool isCountDown = _coolDownRemainsDuration > 0f;
            if (_dirtyIsCountDown != isCountDown)
            {
                _dirtyIsCountDown = isCountDown;
                if (countDownObjects != null)
                {
                    foreach (GameObject obj in countDownObjects)
                    {
                        obj.SetActive(isCountDown);
                    }
                }
                if (noCountDownObjects != null)
                {
                    foreach (GameObject obj in noCountDownObjects)
                    {
                        obj.SetActive(!isCountDown);
                    }
                }
            }
        }

        protected void UpdateCoolDownRemainsDuration(float diffToChangeRemainsDuration = 0f)
        {
            if (_coolDownRemainsDuration <= 0f && Character != null && Skill != null)
            {
                int indexOfSkillUsage = Character.IndexOfSkillUsage(SkillUsageType.Skill, Skill.DataId);
                if (indexOfSkillUsage >= 0 && Mathf.Abs(Character.SkillUsages[indexOfSkillUsage].coolDownRemainsDuration - _coolDownRemainsDuration) > diffToChangeRemainsDuration)
                    _coolDownRemainsDuration = Character.SkillUsages[indexOfSkillUsage].coolDownRemainsDuration;
                else
                    _coolDownRemainsDuration = 0f;
            }
        }

        protected override void UpdateUI()
        {
            UpdateCoolDownRemainsDuration(0f);

            IPlayerCharacterData targetPlayer = Character as IPlayerCharacterData;
            if (targetPlayer == null)
                targetPlayer = GameInstance.PlayingCharacter;

            bool ableToLevelUp = targetPlayer != null && Skill != null && Skill.CanLevelUp(targetPlayer, CharacterSkill.level, out _);
            if (_dirtyAbleToLevelUp != ableToLevelUp)
            {
                _dirtyAbleToLevelUp = ableToLevelUp;
                if (ableToLevelUp)
                    onAbleToLevelUp.Invoke();
                else
                    onUnableToLevelUp.Invoke();
            }

            bool ableToUse = targetPlayer != null && Skill != null && Skill.IsActive && CharacterSkill.level > 0;
            if (_dirtyAbleToUse != ableToUse)
            {
                _dirtyAbleToUse = ableToUse;
                if (ableToUse)
                    onAbleToUse.Invoke();
                else
                    onUnableToUse.Invoke();
            }
        }

        protected override async void UpdateData()
        {
            UpdateCoolDownRemainsDuration(1f);

            if (Level <= 0)
            {
                onSetLevelZeroData.Invoke();
            }
            else
            {
                onSetNonLevelZeroData.Invoke();
            }

            if (uiTextTitle != null)
            {
                uiTextTitle.text = ZString.Format(
                    LanguageManager.GetText(formatKeyTitle),
                    Skill == null ? LanguageManager.GetUnknowTitle() : Skill.Title);
            }

            if (uiTextDescription != null)
            {
                uiTextDescription.text = ZString.Format(
                    LanguageManager.GetText(formatKeyDescription),
                    Skill == null ? LanguageManager.GetUnknowDescription() : Skill.Description);
            }

            if (uiTextLevel != null)
            {
                uiTextLevel.text = ZString.Format(
                    LanguageManager.GetText(formatKeyLevel),
                    Level.ToString("N0"));
            }

#if UNITY_EDITOR || !UNITY_SERVER
            if (imageIcon != null)
            {
                Sprite iconSprite = Skill == null ? null : Skill.Icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
            }
#endif

            if (uiTextSkillType != null)
            {
                uiTextSkillType.text = ZString.Format(
                    LanguageManager.GetText(formatKeySkillType),
                    Skill == null ? LanguageManager.GetUnknowTitle() : Skill.TypeTitle);
            }

            if (uiTextAvailableWeapons != null)
            {
                if (string.IsNullOrEmpty(Skill.AvailableWeaponsText))
                {
                    uiTextAvailableWeapons.SetGameObjectActive(false);
                }
                else
                {
                    uiTextAvailableWeapons.SetGameObjectActive(true);
                    uiTextAvailableWeapons.text = ZString.Format(
                        LanguageManager.GetText(formatKeyAvailableWeapons),
                        Skill.AvailableWeaponsText);
                }
            }

            if (uiTextAvailableArmors != null)
            {
                if (string.IsNullOrEmpty(Skill.AvailableArmorsText))
                {
                    uiTextAvailableArmors.SetGameObjectActive(false);
                }
                else
                {
                    uiTextAvailableArmors.SetGameObjectActive(true);
                    uiTextAvailableArmors.text = ZString.Format(
                        LanguageManager.GetText(formatKeyAvailableArmors),
                        Skill.AvailableArmorsText);
                }
            }

            if (uiTextAvailableVehicles != null)
            {
                if (string.IsNullOrEmpty(Skill.AvailableVehiclesText))
                {
                    uiTextAvailableVehicles.SetGameObjectActive(false);
                }
                else
                {
                    uiTextAvailableVehicles.SetGameObjectActive(true);
                    uiTextAvailableVehicles.text = ZString.Format(
                        LanguageManager.GetText(formatKeyAvailableVehicles),
                        Skill.AvailableVehiclesText);
                }
            }

            int tempConsumeAmount;

            if (uiTextConsumeHp != null)
            {
                tempConsumeAmount = Skill == null || Level <= 0 ? 0 : Skill.GetTotalConsumeHp(Level, Character);
                if (tempConsumeAmount != 0)
                {
                    uiTextConsumeHp.text = ZString.Format(
                        LanguageManager.GetText(formatKeyConsumeHp),
                        tempConsumeAmount.ToString("N0"));
                    uiTextConsumeHp.SetGameObjectActive(true);
                }
                else
                {
                    uiTextConsumeHp.SetGameObjectActive(false);
                }
            }

            if (uiTextConsumeMp != null)
            {
                tempConsumeAmount = Skill == null || Level <= 0 ? 0 : Skill.GetTotalConsumeMp(Level, Character);
                if (tempConsumeAmount != 0)
                {
                    uiTextConsumeMp.text = ZString.Format(
                        LanguageManager.GetText(formatKeyConsumeMp),
                        tempConsumeAmount.ToString("N0"));
                    uiTextConsumeMp.SetGameObjectActive(true);
                }
                else
                {
                    uiTextConsumeMp.SetGameObjectActive(false);
                }
            }

            if (uiTextConsumeStamina != null)
            {
                tempConsumeAmount = Skill == null || Level <= 0 ? 0 : Skill.GetTotalConsumeStamina(Level, Character);
                if (tempConsumeAmount != 0)
                {
                    uiTextConsumeStamina.text = ZString.Format(
                        LanguageManager.GetText(formatKeyConsumeStamina),
                        tempConsumeAmount.ToString("N0"));
                    uiTextConsumeStamina.SetGameObjectActive(true);
                }
                else
                {
                    uiTextConsumeStamina.SetGameObjectActive(false);
                }
            }

            if (uiRequirement != null)
            {
                if (Skill == null || Level <= 0 ||
                    (!Skill.IsDisallowToLevelUp(Level) &&
                    Skill.GetRequireCharacterLevel(Level) <= 0 &&
                    Skill.GetRequireCharacterSkillPoint(Level) <= 0 &&
                    Skill.GetRequireCharacterGold(Level) <= 0 &&
                    Skill.GetRequireAttributeAmounts(Level).Count == 0) &&
                    Skill.GetRequireSkillLevels(Level).Count == 0 &&
                    Skill.GetRequireCurrencyAmounts(Level).Count == 0 &&
                    Skill.GetRequireItemAmounts(Level).Count == 0)
                {
                    uiRequirement.Hide();
                }
                else
                {
                    uiRequirement.Show();
                    uiRequirement.Data = Data;
                }
            }

            if (uiTextSummon != null)
            {
                uiTextSummon.SetGameObjectActive(false);
                if (Skill != null && Skill.TryGetSummon(out SkillSummon skillSummon))
                {
                    BaseMonsterCharacterEntity tempMonsterEntity = 
                        await skillSummon.AddressableMonsterCharacterEntity
                            .GetOrLoadAssetAsyncOrUsePrefab(skillSummon.MonsterCharacterEntity);
                    if (tempMonsterEntity != null)
                    {
                        uiTextSummon.SetGameObjectActive(true);
                        uiTextSummon.text = ZString.Format(
                            LanguageManager.GetText(formatKeySummon),
                            tempMonsterEntity.Title,
                            skillSummon.Level.GetAmount(Level),
                            skillSummon.AmountEachTime.GetAmount(Level),
                            skillSummon.MaxStack.GetAmount(Level),
                            skillSummon.Duration.GetAmount(Level));
                    }
                }
            }

            if (uiTextMount != null)
            {
                uiTextMount.SetGameObjectActive(false);
                if (Skill != null && Skill.TryGetMount(out SkillMount skillMount))
                {
                    VehicleEntity tempMountEntity = 
                        await skillMount.AddressableMountEntity
                            .GetOrLoadAssetAsyncOrUsePrefab(skillMount.MountEntity);
                    if (tempMountEntity != null)
                    {
                        uiTextMount.SetGameObjectActive(true);
                        uiTextMount.text = ZString.Format(
                            LanguageManager.GetText(formatKeyMount),
                            tempMountEntity.Title);
                    }
                }
            }

            if (uiCraftItem != null)
            {
                if (Skill == null || !Skill.TryGetItemCraft(out ItemCraft itemCraft) || itemCraft.CraftingItem == null)
                {
                    uiCraftItem.Hide();
                }
                else
                {
                    uiCraftItem.Setup(CrafterType.Character, null, itemCraft);
                    uiCraftItem.Show();
                }
            }

            if (uiDamageAmount != null)
            {
                if (Skill == null || !Skill.TryGetBaseAttackDamageAmount(Character, Level, false, out KeyValuePair<DamageElement, MinMaxFloat> baseAttackDamageAmount))
                {
                    uiDamageAmount.Hide();
                }
                else
                {
                    uiDamageAmount.Show();
                    uiDamageAmount.Data = new UIDamageElementAmountData(baseAttackDamageAmount.Key, baseAttackDamageAmount.Value);
                }
            }

            if (uiDamageInflictions != null)
            {
                if (Skill == null || !Skill.TryGetAttackWeaponDamageInflictions(Character, Level, out Dictionary<DamageElement, float> damageInflictionRates))
                {
                    uiDamageInflictions.Hide();
                }
                else
                {
                    uiDamageInflictions.Show();
                    uiDamageInflictions.Data = damageInflictionRates;
                }
            }

            if (uiWeaponDamageMultiplicator != null)
            {
                if (Skill == null || !Skill.TryGetAttackWeaponDamageMultiplicator(Character, Level, out float weaponDamageMultiplicator))
                {
                    uiWeaponDamageMultiplicator.SetGameObjectActive(false);
                }
                else
                {
                    uiWeaponDamageMultiplicator.SetGameObjectActive(true);
                    uiWeaponDamageMultiplicator.text = ZString.Format(
                        LanguageManager.GetText(formatKeyWeaponDamageMultiplicator),
                        weaponDamageMultiplicator.ToString("N2"));
                }
            }

            if (uiAdditionalDamageAmounts != null)
            {
                if (Skill == null || !Skill.TryGetAttackAdditionalDamageAmounts(Character, Level, out Dictionary<DamageElement, MinMaxFloat> additionalDamageAmounts))
                {
                    uiAdditionalDamageAmounts.Hide();
                }
                else
                {
                    uiAdditionalDamageAmounts.isBonus = false;
                    uiAdditionalDamageAmounts.Show();
                    uiAdditionalDamageAmounts.Data = additionalDamageAmounts;
                }
            }

            if (uiAttackStatusEffects != null)
            {
                if (Skill == null || !Skill.TryGetAttackStatusEffectApplyings(out StatusEffectApplying[] attackStatusEffects) || attackStatusEffects == null || attackStatusEffects.Length == 0)
                {
                    uiAttackStatusEffects.Hide();
                }
                else
                {
                    uiAttackStatusEffects.UpdateData(attackStatusEffects, Level, UIStatusEffectApplyingTarget.Enemy);
                    uiAttackStatusEffects.Show();
                }
            }

            if (uiSkillBuff != null)
            {
                if (Skill == null || !Skill.TryGetBuff(out Buff buff))
                {
                    uiSkillBuff.Hide();
                }
                else
                {
                    uiSkillBuff.Show();
                    uiSkillBuff.Data = new UIBuffData(buff, Level);
                }
            }

            if (uiSkillDebuff != null)
            {
                if (Skill == null || !Skill.TryGetDebuff(out Buff debuff))
                {
                    uiSkillDebuff.Hide();
                }
                else
                {
                    uiSkillDebuff.Show();
                    uiSkillDebuff.Data = new UIBuffData(debuff, Level);
                }
            }

            if (uiNextLevelSkill != null)
            {
                if (Level + 1 > Skill.maxLevel)
                {
                    uiNextLevelSkill.Hide();
                }
                else
                {
                    uiNextLevelSkill.Setup(new UICharacterSkillData(CharacterSkill, Level + 1), Character, IndexOfData);
                    uiNextLevelSkill.Show();
                }
            }
        }

        public void OnClickAdd()
        {
            GameInstance.ClientCharacterHandlers.RequestIncreaseSkillLevel(new RequestIncreaseSkillLevelMessage()
            {
                dataId = Skill.DataId
            }, ClientCharacterActions.ResponseIncreaseSkillLevel);
        }

        public void OnClickUse()
        {
            if (!IsOwningCharacter())
                return;

            if (selectionManager != null)
                selectionManager.DeselectSelectedUI();

            // Controlling by hotkey controller
            UICharacterHotkeys.SetupHotkeyForAimming(HotkeyType.Skill, Skill.Id);
        }
    }
}
