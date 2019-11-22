using System.Collections;
using System.Collections.Generic;

namespace MultiplayerARPG
{
    public sealed class CharacterDataCache
    {
        public bool IsRecaching { get; private set; }
        private CharacterStats stats;
        public CharacterStats Stats { get { return stats; } }
        private Dictionary<Attribute, float> attributes;
        public Dictionary<Attribute, float> Attributes { get { return attributes; } }
        private Dictionary<BaseSkill, short> skills;
        public Dictionary<BaseSkill, short> Skills { get { return skills; } }
        private Dictionary<DamageElement, float> resistances;
        public Dictionary<DamageElement, float> Resistances { get { return resistances; } }
        private Dictionary<DamageElement, float> armors;
        public Dictionary<DamageElement, float> Armors { get { return armors; } }
        private Dictionary<DamageElement, MinMaxFloat> increaseDamages;
        public Dictionary<DamageElement, MinMaxFloat> IncreaseDamages { get { return increaseDamages; } }
        private Dictionary<EquipmentSet, int> equipmentSets;
        public Dictionary<EquipmentSet, int> EquipmentSets { get { return equipmentSets; } }
        private int maxHp;
        public int MaxHp { get { return maxHp; } }
        private int maxMp;
        public int MaxMp { get { return maxMp; } }
        private int maxStamina;
        public int MaxStamina { get { return maxStamina; } }
        private int maxFood;
        public int MaxFood { get { return maxFood; } }
        private int maxWater;
        public int MaxWater { get { return maxWater; } }
        private float totalItemWeight;
        public float TotalItemWeight { get { return totalItemWeight; } }
        private float atkSpeed;
        public float AtkSpeed { get { return atkSpeed; } }
        private float moveSpeed;
        public float MoveSpeed { get { return moveSpeed; } }
        public float BaseMoveSpeed { get; private set; }
        public bool DisallowMove { get; private set; }
        public bool DisallowAttack { get; private set; }
        public bool DisallowUseSkill { get; private set; }
        public bool DisallowUseItem { get; private set; }
        public bool IsHide { get; private set; }
        public bool MuteFootstepSound { get; private set; }

        public CharacterDataCache()
        {
            attributes = new Dictionary<Attribute, float>();
            resistances = new Dictionary<DamageElement, float>();
            armors = new Dictionary<DamageElement, float>();
            increaseDamages = new Dictionary<DamageElement, MinMaxFloat>();
            skills = new Dictionary<BaseSkill, short>();
            equipmentSets = new Dictionary<EquipmentSet, int>();
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
                attributes,
                resistances,
                armors,
                increaseDamages,
                skills,
                equipmentSets,
                out maxHp,
                out maxMp,
                out maxStamina,
                out maxFood,
                out maxWater,
                out totalItemWeight,
                out atkSpeed,
                out moveSpeed);

            if (characterData.GetDatabase() != null)
                BaseMoveSpeed = characterData.GetDatabase().Stats.baseStats.moveSpeed;

            DisallowMove = false;
            DisallowAttack = false;
            DisallowUseSkill = false;
            DisallowUseItem = false;
            IsHide = false;
            MuteFootstepSound = false;
            Buff tempBuff;
            foreach (CharacterBuff characterBuff in characterData.Buffs)
            {
                tempBuff = characterBuff.GetBuff();
                if (tempBuff.disallowMove)
                    DisallowMove = true;
                if (tempBuff.disallowAttack)
                    DisallowAttack = true;
                if (tempBuff.disallowUseSkill)
                    DisallowUseSkill = true;
                if (tempBuff.disallowUseItem)
                    DisallowUseItem = true;
                if (tempBuff.isHide)
                    IsHide = true;
                if (tempBuff.muteFootstepSound)
                    MuteFootstepSound = true;
                if (DisallowMove &&
                    DisallowAttack &&
                    DisallowUseSkill &&
                    DisallowUseItem &&
                    IsHide &&
                    MuteFootstepSound)
                    break;
            }

            return this;
        }
    }
}
