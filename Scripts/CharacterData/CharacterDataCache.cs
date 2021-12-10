using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public sealed class CharacterDataCache
    {
        public bool IsRecaching { get; private set; }
        private CharacterStats stats;
        public CharacterStats Stats => stats;
        public Dictionary<Attribute, float> Attributes { get; }
        public Dictionary<BaseSkill, short> Skills { get; }
        public Dictionary<DamageElement, float> Resistances { get; }
        public Dictionary<DamageElement, float> Armors { get; }
        public Dictionary<DamageElement, MinMaxFloat> IncreaseDamages { get; }
        public Dictionary<EquipmentSet, int> EquipmentSets { get; }
        private int maxHp;
        public int MaxHp => maxHp;
        private int maxMp;
        public int MaxMp => maxMp;
        private int maxStamina;
        public int MaxStamina => maxStamina;
        private int maxFood;
        public int MaxFood => maxFood;
        private int maxWater;
        public int MaxWater => maxWater;
        private float atkSpeed;
        public float AtkSpeed => atkSpeed;
        private float moveSpeed;
        public float MoveSpeed => moveSpeed;
        public float BaseMoveSpeed { get; private set; }
        public float TotalItemWeight { get; private set; }
        public short TotalItemSlot { get; private set; }
        public float LimitItemWeight { get; private set; }
        public short LimitItemSlot { get; private set; }
        public bool DisallowMove { get; private set; }
        public bool DisallowAttack { get; private set; }
        public bool DisallowUseSkill { get; private set; }
        public bool DisallowUseItem { get; private set; }
        public bool FreezeAnimation { get; private set; }
        public bool IsHide { get; private set; }
        public bool MuteFootstepSound { get; private set; }
        public bool IsOverweight { get; private set; }

        public CharacterDataCache()
        {
            Attributes = new Dictionary<Attribute, float>();
            Resistances = new Dictionary<DamageElement, float>();
            Armors = new Dictionary<DamageElement, float>();
            IncreaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
            Skills = new Dictionary<BaseSkill, short>();
            EquipmentSets = new Dictionary<EquipmentSet, int>();
        }

        public CharacterDataCache MarkToMakeCaches()
        {
            IsRecaching = true;
            return this;
        }

        public CharacterDataCache MakeCache(ICharacterData characterData)
        {
            // Don't make cache if not needed
            if (!IsRecaching)
                return this;

            IsRecaching = false;

            characterData.GetAllStats(
                ref stats,
                Attributes,
                Resistances,
                Armors,
                IncreaseDamages,
                Skills,
                EquipmentSets,
                out maxHp,
                out maxMp,
                out maxStamina,
                out maxFood,
                out maxWater,
                out atkSpeed,
                out moveSpeed,
                false);

            if (characterData.GetDatabase() != null)
                BaseMoveSpeed = characterData.GetDatabase().Stats.baseStats.moveSpeed;

            TotalItemWeight = GameInstance.Singleton.GameplayRule.GetTotalWeight(characterData, stats);
            TotalItemSlot = GameInstance.Singleton.GameplayRule.GetTotalSlot(characterData, stats);
            LimitItemWeight = GameInstance.Singleton.GameplayRule.GetLimitWeight(characterData, stats);
            LimitItemSlot = GameInstance.Singleton.GameplayRule.GetLimitSlot(characterData, stats);

            IsOverweight = (GameInstance.Singleton.IsLimitInventorySlot && TotalItemSlot > LimitItemSlot) || (GameInstance.Singleton.IsLimitInventoryWeight && TotalItemWeight > LimitItemWeight);
            DisallowMove = false;
            DisallowAttack = false;
            DisallowUseSkill = false;
            DisallowUseItem = false;
            FreezeAnimation = false;
            IsHide = false;
            MuteFootstepSound = false;

            foreach (CharacterBuff characterBuff in characterData.Buffs)
            {
                UpdateAppliedAilments(characterBuff.GetBuff());
                if (AllAilmentsWereApplied())
                    break;
            }

            if (!AllAilmentsWereApplied())
            {
                foreach (BaseSkill tempSkill in Skills.Keys)
                {
                    if (tempSkill == null || tempSkill.IsActive || !tempSkill.IsBuff)
                        continue;
                    UpdateAppliedAilments(tempSkill.Buff);
                    if (AllAilmentsWereApplied())
                        break;
                }
            }

            return this;
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

        public short GetSkill(string nameId)
        {
            return GetSkill(nameId.GenerateHashId());
        }

        public short GetSkill(int dataId)
        {
            BaseSkill data;
            short result;
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

        public MinMaxFloat GetIncreaseDamage(string nameId)
        {
            return GetIncreaseDamage(nameId.GenerateHashId());
        }

        public MinMaxFloat GetIncreaseDamage(int dataId)
        {
            DamageElement data;
            MinMaxFloat result;
            if (GameInstance.DamageElements.TryGetValue(dataId, out data) &&
                IncreaseDamages.TryGetValue(data, out result))
                return result;
            return default(MinMaxFloat);
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

        public void UpdateAppliedAilments(Buff tempBuff)
        {
            switch (tempBuff.ailment)
            {
                case AilmentPresets.Stun:
                    DisallowMove = true;
                    DisallowAttack = true;
                    DisallowUseSkill = true;
                    DisallowUseItem = true;
                    break;
                case AilmentPresets.Mute:
                    DisallowUseSkill = true;
                    break;
                case AilmentPresets.Freeze:
                    DisallowMove = true;
                    DisallowAttack = true;
                    DisallowUseSkill = true;
                    DisallowUseItem = true;
                    FreezeAnimation = true;
                    break;
                default:
                    if (tempBuff.disallowMove)
                        DisallowMove = true;
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
        }

        public bool AllAilmentsWereApplied()
        {
            return DisallowMove &&
                DisallowAttack &&
                DisallowUseSkill &&
                DisallowUseItem &&
                FreezeAnimation &&
                IsHide &&
                MuteFootstepSound;
        }
        #endregion
    }
}
