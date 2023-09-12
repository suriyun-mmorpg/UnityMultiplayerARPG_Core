using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterDataCache
    {
        public bool IsRecaching { get; private set; }
        private CharacterStats _stats;
        public CharacterStats Stats => _stats;
        public Dictionary<Attribute, float> Attributes { get; private set; }
        public Dictionary<BaseSkill, int> Skills { get; private set; }
        public Dictionary<DamageElement, float> Resistances { get; private set; }
        public Dictionary<DamageElement, float> Armors { get; private set; }
        public Dictionary<DamageElement, MinMaxFloat> RightHandDamages { get; private set; }
        public KeyValuePair<DamageElement, MinMaxFloat>? RightHandWeaponDamage { get; private set; }
        public Dictionary<DamageElement, MinMaxFloat> LeftHandDamages { get; private set; }
        public KeyValuePair<DamageElement, MinMaxFloat>? LeftHandWeaponDamage { get; private set; }
        public Dictionary<DamageElement, MinMaxFloat> IncreaseDamages { get; private set; }
        public Dictionary<DamageElement, MinMaxFloat> IncreaseDamagesRate { get; private set; }
        public Dictionary<EquipmentSet, int> EquipmentSets { get; private set; }
        public int MaxHp => (int)_stats.hp;
        public int MaxMp => (int)_stats.mp;
        public int MaxStamina => (int)_stats.stamina;
        public int MaxFood => (int)_stats.food;
        public int MaxWater => (int)_stats.water;
        public float AtkSpeed => _stats.atkSpeed;
        public float MoveSpeed => _stats.moveSpeed;
        public float JumpHeight => _stats.jumpHeight;
        public float HeadDamageAbsorbs => _stats.headDamageAbsorbs;
        public float BodyDamageAbsorbs => _stats.bodyDamageAbsorbs;
        public float FallDamageAbsorbs => _stats.fallDamageAbsorbs;
        public float GravityRate => _stats.gravityRate;
        public float BaseMoveSpeed { get; private set; }
        public float TotalItemWeight { get; private set; }
        public int TotalItemSlot { get; private set; }
        public float LimitItemWeight { get; private set; }
        public int LimitItemSlot { get; private set; }
        public bool DisallowMove { get; private set; }
        public bool DisallowSprint { get; private set; }
        public bool DisallowWalk { get; private set; }
        public bool DisallowJump { get; private set; }
        public bool DisallowCrouch { get; private set; }
        public bool DisallowCrawl { get; private set; }
        public bool DisallowAttack { get; private set; }
        public bool DisallowUseSkill { get; private set; }
        public bool DisallowUseItem { get; private set; }
        public bool FreezeAnimation { get; private set; }
        public bool IsHide { get; private set; }
        public bool MuteFootstepSound { get; private set; }
        public bool IsOverweight { get; private set; }
        public bool HavingChanceToRemoveBuffWhenAttack { get; private set; }
        public bool HavingChanceToRemoveBuffWhenAttacked { get; private set; }
        public bool HavingChanceToRemoveBuffWhenUseSkill { get; private set; }
        public bool HavingChanceToRemoveBuffWhenUseItem { get; private set; }
        public bool HavingChanceToRemoveBuffWhenPickupItem { get; private set; }
        public int BattlePoints { get; private set; }
        public CharacterItem RightHandItem { get; private set; }
        public CharacterItem LeftHandItem { get; private set; }
        public bool IsRightHandItemAvailable { get; private set; }
        public bool IsLeftHandItemAvailable { get; private set; }

        public CharacterDataCache()
        {
            Attributes = new Dictionary<Attribute, float>();
            Resistances = new Dictionary<DamageElement, float>();
            Armors = new Dictionary<DamageElement, float>();
            RightHandDamages = new Dictionary<DamageElement, MinMaxFloat>();
            LeftHandDamages = new Dictionary<DamageElement, MinMaxFloat>();
            IncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
            IncreaseDamagesRate = new Dictionary<DamageElement, MinMaxFloat>();
            Skills = new Dictionary<BaseSkill, int>();
            EquipmentSets = new Dictionary<EquipmentSet, int>();
        }

        ~CharacterDataCache()
        {
            Attributes.Clear();
            Attributes = null;
            Resistances.Clear();
            Resistances = null;
            Armors.Clear();
            Armors = null;
            RightHandDamages.Clear();
            RightHandDamages = null;
            RightHandWeaponDamage = null;
            LeftHandDamages.Clear();
            LeftHandDamages = null;
            LeftHandWeaponDamage = null;
            IncreaseDamages.Clear();
            IncreaseDamages = null;
            IncreaseDamagesRate.Clear();
            IncreaseDamagesRate = null;
            Skills.Clear();
            Skills = null;
            EquipmentSets.Clear();
            EquipmentSets = null;
        }

        public CharacterDataCache MarkToMakeCaches()
        {
            IsRecaching = true;
            return this;
        }

        private void SetStats(CharacterStats stats)
        {
            _stats = stats;
        }

        private void SetAttributes(Dictionary<Attribute, float> attributes)
        {
            Attributes = null;
            Attributes = attributes;
        }

        private void SetResistances(Dictionary<DamageElement, float> resistances)
        {
            Resistances = null;
            Resistances = resistances;
        }

        private void SetArmors(Dictionary<DamageElement, float> armors)
        {
            Armors = null;
            Armors = armors;
        }

        private void SetRightHandDamages(Dictionary<DamageElement, MinMaxFloat> rightHandDamages)
        {
            RightHandDamages = null;
            RightHandDamages = rightHandDamages;
        }

        private void SetRightHandWeaponDamage(KeyValuePair<DamageElement, MinMaxFloat> rightHandDamage)
        {
            RightHandWeaponDamage = null;
            RightHandWeaponDamage = rightHandDamage;
        }

        private void SetLeftHandDamages(Dictionary<DamageElement, MinMaxFloat> leftHandDamages)
        {
            LeftHandDamages = null;
            LeftHandDamages = leftHandDamages;
        }

        private void SetLeftHandWeaponDamage(KeyValuePair<DamageElement, MinMaxFloat> leftHandDamage)
        {
            LeftHandWeaponDamage = null;
            LeftHandWeaponDamage = leftHandDamage;
        }

        private void SetIncreaseDamages(Dictionary<DamageElement, MinMaxFloat> increaseDamages)
        {
            IncreaseDamages = null;
            IncreaseDamages = increaseDamages;
        }

        private void SetIncreaseDamagesRate(Dictionary<DamageElement, MinMaxFloat> increaseDamagesRate)
        {
            IncreaseDamagesRate = null;
            IncreaseDamagesRate = increaseDamagesRate;
        }

        private void SetSkills(Dictionary<BaseSkill, int> skills)
        {
            Skills = null;
            Skills = skills;
        }

        private void SetEquipmentSets(Dictionary<EquipmentSet, int> equipmentSets)
        {
            EquipmentSets = null;
            EquipmentSets = equipmentSets;
        }

        public CharacterDataCache GetCaches(ICharacterData characterData)
        {
            // Don't make cache if not needed
            if (!IsRecaching)
                return this;

            IsRecaching = false;
            Attributes.Clear();
            Resistances.Clear();
            Armors.Clear();
            RightHandDamages.Clear();
            LeftHandDamages.Clear();
            IncreaseDamages.Clear();
            IncreaseDamagesRate.Clear();
            Skills.Clear();
            EquipmentSets.Clear();

            int oldBattlePoints = BattlePoints;

            characterData.GetAllStats(true, true, true,
                SetStats,
                SetAttributes,
                SetResistances,
                SetArmors,
                SetRightHandDamages,
                SetRightHandWeaponDamage,
                SetLeftHandDamages,
                SetLeftHandWeaponDamage,
                SetSkills,
                SetEquipmentSets,
                onGetIncreasingDamages: SetIncreaseDamages,
                onGetIncreasingDamagesRate: SetIncreaseDamagesRate);

            if (characterData.GetDatabase() != null)
                BaseMoveSpeed = characterData.GetDatabase().Stats.baseStats.moveSpeed;

            TotalItemWeight = GameInstance.Singleton.GameplayRule.GetTotalWeight(characterData, _stats);
            TotalItemSlot = GameInstance.Singleton.GameplayRule.GetTotalSlot(characterData, _stats);
            LimitItemWeight = GameInstance.Singleton.GameplayRule.GetLimitWeight(characterData, _stats);
            LimitItemSlot = GameInstance.Singleton.GameplayRule.GetLimitSlot(characterData, _stats);

            IsOverweight = (GameInstance.Singleton.IsLimitInventorySlot && TotalItemSlot > LimitItemSlot) || (GameInstance.Singleton.IsLimitInventoryWeight && TotalItemWeight > LimitItemWeight);
            DisallowMove = false;
            DisallowSprint = false;
            DisallowWalk = false;
            DisallowJump = false;
            DisallowCrouch = false;
            DisallowCrawl = false;
            DisallowAttack = false;
            DisallowUseSkill = false;
            DisallowUseItem = false;
            FreezeAnimation = false;
            IsHide = false;
            MuteFootstepSound = false;
            HavingChanceToRemoveBuffWhenAttack = false;
            HavingChanceToRemoveBuffWhenAttacked = false;
            HavingChanceToRemoveBuffWhenUseSkill = false;
            HavingChanceToRemoveBuffWhenUseItem = false;
            HavingChanceToRemoveBuffWhenPickupItem = false;

            bool allAilmentsWereApplied = false;
            if (characterData.PassengingVehicleEntity != null)
            {
                UpdateAppliedAilments(characterData.PassengingVehicleEntity.GetBuff());
                allAilmentsWereApplied = AllAilmentsWereApplied();
            }

            if (!allAilmentsWereApplied)
            {
                foreach (CharacterBuff characterBuff in characterData.Buffs)
                {
                    UpdateAppliedAilments(characterBuff.GetBuff());
                    allAilmentsWereApplied = AllAilmentsWereApplied();
                    if (allAilmentsWereApplied)
                        break;
                }
            }

            if (!allAilmentsWereApplied)
            {
                foreach (CharacterSummon characterBuff in characterData.Summons)
                {
                    UpdateAppliedAilments(characterBuff.GetBuff());
                    allAilmentsWereApplied = AllAilmentsWereApplied();
                    if (allAilmentsWereApplied)
                        break;
                }
            }

            float tempTotalBattlePoint = 0f;

            foreach (Attribute tempAttribute in Attributes.Keys)
            {
                if (tempAttribute == null)
                    continue;
                float amount = Attributes[tempAttribute];
                tempTotalBattlePoint += tempAttribute.BattlePointScore * amount;
            }

            foreach (BaseSkill tempSkill in Skills.Keys)
            {
                if (tempSkill == null)
                    continue;
                int skillLevel = Skills[tempSkill];
                tempTotalBattlePoint += tempSkill.battlePointScore * skillLevel;
                // Apply ailments by passive buff only
                if (!allAilmentsWereApplied && !tempSkill.IsActive && tempSkill.IsBuff)
                {
                    UpdateAppliedAilments(new CalculatedBuff(tempSkill.Buff, skillLevel));
                    allAilmentsWereApplied = AllAilmentsWereApplied();
                }
            }

            foreach (DamageElement tempDamageElement in Resistances.Keys)
            {
                if (tempDamageElement == null)
                    continue;
                float amount = Resistances[tempDamageElement];
                tempTotalBattlePoint += tempDamageElement.ResistanceBattlePointScore * amount;
            }

            foreach (DamageElement tempDamageElement in Armors.Keys)
            {
                if (tempDamageElement == null)
                    continue;
                float amount = Armors[tempDamageElement];
                tempTotalBattlePoint += tempDamageElement.ArmorBattlePointScore * amount;
            }

            foreach (DamageElement tempDamageElement in RightHandDamages.Keys)
            {
                if (tempDamageElement == null)
                    continue;
                MinMaxFloat amount = RightHandDamages[tempDamageElement];
                tempTotalBattlePoint += tempDamageElement.DamageBattlePointScore * (amount.min + amount.max) * 0.5f;
            }

            foreach (DamageElement tempDamageElement in LeftHandDamages.Keys)
            {
                if (tempDamageElement == null)
                    continue;
                MinMaxFloat amount = LeftHandDamages[tempDamageElement];
                tempTotalBattlePoint += tempDamageElement.DamageBattlePointScore * (amount.min + amount.max) * 0.5f;
            }

            if (characterData.EquipWeapons.NotEmptyRightHandSlot())
            {
                tempTotalBattlePoint += characterData.EquipWeapons.rightHand.GetWeaponDamageBattlePoints();
            }

            if (characterData.EquipWeapons.NotEmptyLeftHandSlot())
            {
                tempTotalBattlePoint += characterData.EquipWeapons.leftHand.GetWeaponDamageBattlePoints();
            }

            tempTotalBattlePoint += GameInstance.Singleton.GameplayRule.GetBattlePointFromCharacterStats(Stats);
            BattlePoints = Mathf.CeilToInt(tempTotalBattlePoint);

            if (characterData == GameInstance.PlayingCharacter)
            {
                int battlePointChange = BattlePoints - oldBattlePoints;
                if (battlePointChange != 0)
                    ClientGenericActions.NotifyBattlePointsChanged(battlePointChange);
            }

            IsRightHandItemAvailable = false;
            IsLeftHandItemAvailable = false;

            IWeaponItem rightWeaponItem = characterData.EquipWeapons.GetRightHandWeaponItem();
            if (rightWeaponItem != null)
            {
                IsRightHandItemAvailable = true;
                RightHandItem = characterData.EquipWeapons.rightHand;
            }
            IWeaponItem leftWeaponItem = characterData.EquipWeapons.GetLeftHandWeaponItem();
            if (leftWeaponItem != null)
            {
                IsLeftHandItemAvailable = true;
                LeftHandItem = characterData.EquipWeapons.leftHand;
            }
            if (!IsRightHandItemAvailable && !IsRightHandItemAvailable)
            {
                IsRightHandItemAvailable = true;
                RightHandItem = CharacterItem.CreateDefaultWeapon();
            }

            return this;
        }

        public CharacterItem GetAvailableWeapon(ref bool isLeftHand)
        {
            if (isLeftHand && !IsLeftHandItemAvailable)
                isLeftHand = false;
            return isLeftHand ? LeftHandItem : RightHandItem;
        }

        public void ClearChanceToRemoveBuffWhenAttack()
        {
            HavingChanceToRemoveBuffWhenAttack = false;
        }

        public void ClearChanceToRemoveBuffWhenAttacked()
        {
            HavingChanceToRemoveBuffWhenAttacked = false;
        }

        public void ClearChanceToRemoveBuffWhenUseSkill()
        {
            HavingChanceToRemoveBuffWhenUseSkill = false;
        }

        public void ClearChanceToRemoveBuffWhenUseItem()
        {
            HavingChanceToRemoveBuffWhenUseItem = false;
        }

        public void ClearChanceToRemoveBuffWhenPickupItem()
        {
            HavingChanceToRemoveBuffWhenPickupItem = false;
        }

        #region Helper functions to get stats amount
        public float GetAttribute(string nameId)
        {
            return GetAttribute(nameId.GenerateHashId());
        }

        public float GetAttribute(int dataId)
        {
            Attribute data;
            float result;
            if (GameInstance.Attributes.TryGetValue(dataId, out data) &&
                Attributes.TryGetValue(data, out result))
                return result;
            return 0f;
        }

        public int GetSkill(string nameId)
        {
            return GetSkill(nameId.GenerateHashId());
        }

        public int GetSkill(int dataId)
        {
            BaseSkill data;
            int result;
            if (GameInstance.Skills.TryGetValue(dataId, out data) &&
                Skills.TryGetValue(data, out result))
                return result;
            return 0;
        }

        public float GetResistance(string nameId)
        {
            return GetResistance(nameId.GenerateHashId());
        }

        public float GetResistance(int dataId)
        {
            DamageElement data;
            float result;
            if (GameInstance.DamageElements.TryGetValue(dataId, out data) &&
                Resistances.TryGetValue(data, out result))
                return result;
            return 0f;
        }

        public float GetArmor(string nameId)
        {
            return GetArmor(nameId.GenerateHashId());
        }

        public float GetArmor(int dataId)
        {
            DamageElement data;
            float result;
            if (GameInstance.DamageElements.TryGetValue(dataId, out data) &&
                Armors.TryGetValue(data, out result))
                return result;
            return 0f;
        }

        public MinMaxFloat GetRightHandDamages(string nameId)
        {
            return GetRightHandDamages(nameId.GenerateHashId());
        }

        public MinMaxFloat GetRightHandDamages(int dataId)
        {
            DamageElement data;
            MinMaxFloat result;
            if (GameInstance.DamageElements.TryGetValue(dataId, out data) &&
                RightHandDamages.TryGetValue(data, out result))
                return result;
            return default;
        }

        public MinMaxFloat GetLeftHandDamages(string nameId)
        {
            return GetLeftHandDamages(nameId.GenerateHashId());
        }

        public MinMaxFloat GetLeftHandDamages(int dataId)
        {
            DamageElement data;
            MinMaxFloat result;
            if (GameInstance.DamageElements.TryGetValue(dataId, out data) &&
                LeftHandDamages.TryGetValue(data, out result))
                return result;
            return default;
        }

        public int GetEquipmentSet(string nameId)
        {
            return GetEquipmentSet(nameId.GenerateHashId());
        }

        public int GetEquipmentSet(int dataId)
        {
            EquipmentSet data;
            int result;
            if (GameInstance.EquipmentSets.TryGetValue(dataId, out data) &&
                EquipmentSets.TryGetValue(data, out result))
                return result;
            return 0;
        }

        public void UpdateAppliedAilments(CalculatedBuff buff)
        {
            Buff tempBuff = buff.GetBuff();
            switch (tempBuff.ailment)
            {
                case AilmentPresets.Stun:
                    DisallowMove = true;
                    DisallowSprint = true;
                    DisallowWalk = true;
                    DisallowJump = true;
                    DisallowCrouch = true;
                    DisallowCrawl = true;
                    DisallowAttack = true;
                    DisallowUseSkill = true;
                    DisallowUseItem = true;
                    break;
                case AilmentPresets.Mute:
                    DisallowUseSkill = true;
                    break;
                case AilmentPresets.Freeze:
                    DisallowMove = true;
                    DisallowSprint = true;
                    DisallowWalk = true;
                    DisallowJump = true;
                    DisallowCrouch = true;
                    DisallowCrawl = true;
                    DisallowAttack = true;
                    DisallowUseSkill = true;
                    DisallowUseItem = true;
                    FreezeAnimation = true;
                    break;
                default:
                    if (tempBuff.disallowMove)
                        DisallowMove = true;
                    if (tempBuff.disallowSprint)
                        DisallowSprint = true;
                    if (tempBuff.disallowWalk)
                        DisallowWalk = true;
                    if (tempBuff.disallowJump)
                        DisallowJump = true;
                    if (tempBuff.disallowCrouch)
                        DisallowCrouch = true;
                    if (tempBuff.disallowCrawl)
                        DisallowCrawl = true;
                    if (tempBuff.disallowAttack)
                        DisallowAttack = true;
                    if (tempBuff.disallowUseSkill)
                        DisallowUseSkill = true;
                    if (tempBuff.disallowUseItem)
                        DisallowUseItem = true;
                    if (tempBuff.freezeAnimation)
                        FreezeAnimation = true;
                    break;
            }
            if (tempBuff.isHide)
                IsHide = true;
            if (tempBuff.muteFootstepSound)
                MuteFootstepSound = true;
            if (buff.GetRemoveBuffWhenAttackChance() > 0f)
                HavingChanceToRemoveBuffWhenAttack = true;
            if (buff.GetRemoveBuffWhenAttackedChance() > 0f)
                HavingChanceToRemoveBuffWhenAttacked = true;
            if (buff.GetRemoveBuffWhenUseSkillChance() > 0f)
                HavingChanceToRemoveBuffWhenUseSkill = true;
            if (buff.GetRemoveBuffWhenUseItemChance() > 0f)
                HavingChanceToRemoveBuffWhenUseItem = true;
            if (buff.GetRemoveBuffWhenPickupItemChance() > 0f)
                HavingChanceToRemoveBuffWhenPickupItem = true;
        }

        public bool AllAilmentsWereApplied()
        {
            return DisallowMove &&
                DisallowSprint &&
                DisallowWalk &&
                DisallowJump &&
                DisallowCrouch &&
                DisallowCrawl &&
                DisallowAttack &&
                DisallowUseSkill &&
                DisallowUseItem &&
                FreezeAnimation &&
                IsHide &&
                MuteFootstepSound &&
                HavingChanceToRemoveBuffWhenAttack &&
                HavingChanceToRemoveBuffWhenAttacked &&
                HavingChanceToRemoveBuffWhenUseSkill &&
                HavingChanceToRemoveBuffWhenUseItem &&
                HavingChanceToRemoveBuffWhenPickupItem;
        }
        #endregion
    }
}
