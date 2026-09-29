using Insthync.UnityEditorUtils;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class EquipmentBonus
    {
        [System.NonSerialized]
        private int _indexedGeneration = -1;

        private void EnsureIndexed()
        {
            if (_indexedGeneration == RuntimeGameDataSlots.Generation)
                return;
            _cacheAttributes = default;
            _cacheAttributesRate = default;
            _cacheResistances = default;
            _cacheArmors = default;
            _cacheArmorsRate = default;
            _cacheDamages = default;
            _cacheDamagesRate = default;
            GameDataHelpers.CombineAttributes(attributes, ref _cacheAttributes, 1f);
            GameDataHelpers.CombineAttributes(attributesRate, ref _cacheAttributesRate, 1f);
            GameDataHelpers.CombineResistances(resistances, ref _cacheResistances, 1f);
            GameDataHelpers.CombineArmors(armors, ref _cacheArmors, 1f);
            GameDataHelpers.CombineArmors(armorsRate, ref _cacheArmorsRate, 1f);
            GameDataHelpers.CombineDamages(damages, ref _cacheDamages, 1f);
            GameDataHelpers.CombineDamages(damagesRate, ref _cacheDamagesRate, 1f);
            _dictionaryAttributes = null;
            _dictionaryAttributesRate = null;
            _dictionaryResistances = null;
            _dictionaryArmors = null;
            _dictionaryArmorsRate = null;
            _dictionaryDamages = null;
            _dictionaryDamagesRate = null;
            _indexedGeneration = RuntimeGameDataSlots.Generation;
        }

        public void RegisterReferencedData()
        {
            GameInstance.AddAttributes(attributes);
            GameInstance.AddAttributes(attributesRate);
            GameInstance.AddDamageElements(resistances);
            GameInstance.AddDamageElements(armors);
            GameInstance.AddDamageElements(armorsRate);
            GameInstance.AddDamageElements(damages);
            GameInstance.AddDamageElements(damagesRate);
            GameInstance.AddSkills(Skills.Keys);
            GameInstance.AddStatusEffects(StatusEffectResistances.Keys);
        }

        [SerializeField]
        private CharacterStats stats = new CharacterStats();
        public CharacterStats Stats => stats;

        [SerializeField]
        private CharacterStats statsRate = new CharacterStats();
        public CharacterStats StatsRate => statsRate;

        [ArrayElementTitle("attribute")]
        [SerializeField]
        private AttributeAmount[] attributes = new AttributeAmount[0];
        [System.NonSerialized]
        private AttributeAmounts _cacheAttributes;
        [System.NonSerialized]
        private Dictionary<Attribute, float> _dictionaryAttributes;
        [Newtonsoft.Json.JsonIgnore]
        public AttributeAmounts IndexedAttributes
        {
            get
            {
                EnsureIndexed();
                return _cacheAttributes;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<Attribute, float> Attributes
        {
            get
            {
                EnsureIndexed();
                if (_dictionaryAttributes == null)
                {
                    _dictionaryAttributes = new Dictionary<Attribute, float>();
                    _cacheAttributes.CopyTo(_dictionaryAttributes);
                }
                return _dictionaryAttributes;
            }
        }

        [ArrayElementTitle("attribute")]
        [SerializeField]
        private AttributeAmount[] attributesRate = new AttributeAmount[0];
        [System.NonSerialized]
        private AttributeAmounts _cacheAttributesRate;
        [System.NonSerialized]
        private Dictionary<Attribute, float> _dictionaryAttributesRate;
        [Newtonsoft.Json.JsonIgnore]
        public AttributeAmounts IndexedAttributesRate
        {
            get
            {
                EnsureIndexed();
                return _cacheAttributesRate;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<Attribute, float> AttributesRate
        {
            get
            {
                EnsureIndexed();
                if (_dictionaryAttributesRate == null)
                {
                    _dictionaryAttributesRate = new Dictionary<Attribute, float>();
                    _cacheAttributesRate.CopyTo(_dictionaryAttributesRate);
                }
                return _dictionaryAttributesRate;
            }
        }

        [ArrayElementTitle("damageElement")]
        [SerializeField]
        private ResistanceAmount[] resistances = new ResistanceAmount[0];
        [System.NonSerialized]
        private DamageElementFloatAmounts _cacheResistances;
        [System.NonSerialized]
        private Dictionary<DamageElement, float> _dictionaryResistances;
        [Newtonsoft.Json.JsonIgnore]
        public DamageElementFloatAmounts IndexedResistances
        {
            get
            {
                EnsureIndexed();
                return _cacheResistances;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<DamageElement, float> Resistances
        {
            get
            {
                EnsureIndexed();
                if (_dictionaryResistances == null)
                {
                    _dictionaryResistances = new Dictionary<DamageElement, float>();
                    _cacheResistances.CopyTo(_dictionaryResistances);
                }
                return _dictionaryResistances;
            }
        }

        [ArrayElementTitle("damageElement")]
        [SerializeField]
        private ArmorAmount[] armors = new ArmorAmount[0];
        [System.NonSerialized]
        private DamageElementFloatAmounts _cacheArmors;
        [System.NonSerialized]
        private Dictionary<DamageElement, float> _dictionaryArmors;
        [Newtonsoft.Json.JsonIgnore]
        public DamageElementFloatAmounts IndexedArmors
        {
            get
            {
                EnsureIndexed();
                return _cacheArmors;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<DamageElement, float> Armors
        {
            get
            {
                EnsureIndexed();
                if (_dictionaryArmors == null)
                {
                    _dictionaryArmors = new Dictionary<DamageElement, float>();
                    _cacheArmors.CopyTo(_dictionaryArmors);
                }
                return _dictionaryArmors;
            }
        }

        [ArrayElementTitle("damageElement")]
        [SerializeField]
        private ArmorAmount[] armorsRate = new ArmorAmount[0];
        [System.NonSerialized]
        private DamageElementFloatAmounts _cacheArmorsRate;
        [System.NonSerialized]
        private Dictionary<DamageElement, float> _dictionaryArmorsRate;
        [Newtonsoft.Json.JsonIgnore]
        public DamageElementFloatAmounts IndexedArmorsRate
        {
            get
            {
                EnsureIndexed();
                return _cacheArmorsRate;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<DamageElement, float> ArmorsRate
        {
            get
            {
                EnsureIndexed();
                if (_dictionaryArmorsRate == null)
                {
                    _dictionaryArmorsRate = new Dictionary<DamageElement, float>();
                    _cacheArmorsRate.CopyTo(_dictionaryArmorsRate);
                }
                return _dictionaryArmorsRate;
            }
        }

        [ArrayElementTitle("damageElement")]
        [SerializeField]
        private DamageAmount[] damages = new DamageAmount[0];
        [System.NonSerialized]
        private DamageElementMinMaxFloatAmounts _cacheDamages;
        [System.NonSerialized]
        private Dictionary<DamageElement, MinMaxFloat> _dictionaryDamages;
        [Newtonsoft.Json.JsonIgnore]
        public DamageElementMinMaxFloatAmounts IndexedDamages
        {
            get
            {
                EnsureIndexed();
                return _cacheDamages;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<DamageElement, MinMaxFloat> Damages
        {
            get
            {
                EnsureIndexed();
                if (_dictionaryDamages == null)
                {
                    _dictionaryDamages = new Dictionary<DamageElement, MinMaxFloat>();
                    _cacheDamages.CopyToDictionary(_dictionaryDamages);
                }
                return _dictionaryDamages;
            }
        }

        [ArrayElementTitle("damageElement")]
        [SerializeField]
        private DamageAmount[] damagesRate = new DamageAmount[0];
        [System.NonSerialized]
        private DamageElementMinMaxFloatAmounts _cacheDamagesRate;
        [System.NonSerialized]
        private Dictionary<DamageElement, MinMaxFloat> _dictionaryDamagesRate;
        [Newtonsoft.Json.JsonIgnore]
        public DamageElementMinMaxFloatAmounts IndexedDamagesRate
        {
            get
            {
                EnsureIndexed();
                return _cacheDamagesRate;
            }
        }
        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<DamageElement, MinMaxFloat> DamagesRate
        {
            get
            {
                EnsureIndexed();
                if (_dictionaryDamagesRate == null)
                {
                    _dictionaryDamagesRate = new Dictionary<DamageElement, MinMaxFloat>();
                    _cacheDamagesRate.CopyToDictionary(_dictionaryDamagesRate);
                }
                return _dictionaryDamagesRate;
            }
        }

        [ArrayElementTitle("skill")]
        [SerializeField]
        private SkillLevel[] skills = new SkillLevel[0];
        [System.NonSerialized]
        private Dictionary<BaseSkill, int> _cacheSkills = null;
        public Dictionary<BaseSkill, int> Skills
        {
            get
            {
                if (_cacheSkills == null)
                {
                    _cacheSkills = new Dictionary<BaseSkill, int>();
                    GameDataHelpers.CombineSkills(skills, _cacheSkills, 1f);
                }
                return _cacheSkills;
            }
        }

        [ArrayElementTitle("statusEffect")]
        [SerializeField]
        private StatusEffectResistanceAmount[] statusEffectResistances = new StatusEffectResistanceAmount[0];
        [System.NonSerialized]
        private Dictionary<StatusEffect, float> _cacheStatusEffectResistances = null;
        public Dictionary<StatusEffect, float> StatusEffectResistances
        {
            get
            {
                if (_cacheStatusEffectResistances == null)
                {
                    _cacheStatusEffectResistances = new Dictionary<StatusEffect, float>();
                    GameDataHelpers.CombineStatusEffectResistances(statusEffectResistances, _cacheStatusEffectResistances, 1f);
                }
                return _cacheStatusEffectResistances;
            }
        }
    }
}
