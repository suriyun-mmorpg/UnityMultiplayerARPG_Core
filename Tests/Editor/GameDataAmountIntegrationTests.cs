using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MultiplayerARPG.Tests
{
    public class GameDataAmountIntegrationTests
    {
        private readonly List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();
        private Attribute _strength;
        private Attribute _intelligence;
        private DamageElement _fire;
        private DamageElement _ice;

        [SetUp]
        public void SetUp()
        {
            RuntimeGameDataSlots.Clear();
            _strength = CreateAsset<Attribute>();
            _intelligence = CreateAsset<Attribute>();
            _fire = CreateAsset<DamageElement>();
            _ice = CreateAsset<DamageElement>();
            RuntimeGameDataSlots.Register(_strength);
            RuntimeGameDataSlots.Register(_intelligence);
            RuntimeGameDataSlots.RegisterDefaultDamageElement(_fire);
            RuntimeGameDataSlots.Register(_ice);
        }

        [TearDown]
        public void TearDown()
        {
            RuntimeGameDataSlots.Clear();
            foreach (UnityEngine.Object asset in _assets)
                UnityEngine.Object.DestroyImmediate(asset);
            _assets.Clear();
        }

        private T CreateAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            _assets.Add(asset);
            return asset;
        }

        private static void SetSerializedField(Type owner, object target, string name, object value)
        {
            FieldInfo field = owner.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, $"Missing serialized field {owner.Name}.{name}");
            field.SetValue(target, value);
        }

        private static IncrementalFloat FloatAtLevel(float first, float increase = 0f)
        {
            return new IncrementalFloat { baseAmount = first, amountIncreaseEachLevel = increase };
        }

        private static IncrementalMinMaxFloat RangeAtLevel(float min, float max, float minIncrease = 0f, float maxIncrease = 0f)
        {
            return new IncrementalMinMaxFloat
            {
                baseAmount = new MinMaxFloat { min = min, max = max },
                amountIncreaseEachLevel = new MinMaxFloat { min = minIncrease, max = maxIncrease },
            };
        }

        [Test]
        public void MockWeaponAndSkillCombineRequirementsStatsAndAttributeEffectiveness()
        {
            WeaponItem weapon = CreateAsset<WeaponItem>();
            weapon.Requirement.attributeAmounts = new[]
            {
                new AttributeAmount { attribute = _strength, amount = 3f },
                new AttributeAmount { attribute = _intelligence, amount = 2f },
                new AttributeAmount { attribute = _strength, amount = 2f },
            };
            SetSerializedField(typeof(BaseEquipmentItem), weapon, "increaseAttributes", new[]
            {
                new AttributeIncremental { attribute = _strength, amount = FloatAtLevel(1f, 0.5f) },
                new AttributeIncremental { attribute = _intelligence, amount = FloatAtLevel(1.5f) },
            });
            SetSerializedField(typeof(BaseEquipmentItem), weapon, "increaseStats", new CharacterStatsIncremental
            {
                baseStats = new CharacterStats { hp = 5f },
                statsIncreaseEachLevel = new CharacterStats { hp = 2f },
            });
            SetSerializedField(typeof(Attribute), _strength, "statsIncreaseEachLevel", new CharacterStats { hp = 2f });
            SetSerializedField(typeof(Attribute), _intelligence, "statsIncreaseEachLevel", new CharacterStats { mp = 3f });

            Skill skill = CreateAsset<Skill>();
            skill.requirementEachLevels.Add(new SkillRequirementEntry());
            // TODO: Add boundary tests for skill and item requirements above
            // requirementEachLevels.Count after BaseSkill's out-of-range indexing is fixed.
            skill.requirementEachLevels.Add(new SkillRequirementEntry
            {
                attributeAmounts = new[]
                {
                    new AttributeAmount { attribute = _strength, amount = 4f },
                    new AttributeAmount { attribute = _intelligence, amount = 1f },
                },
            });
            skill.effectivenessAttributes = new[]
            {
                new DamageEffectivenessAttribute { attribute = _strength, effectiveness = 0.5f },
                new DamageEffectivenessAttribute { attribute = _intelligence, effectiveness = 1.5f },
                new DamageEffectivenessAttribute { attribute = _strength, effectiveness = 0.25f },
            };

            Assert.That(weapon.RequireAttributeAmounts[_strength.RuntimeSlot], Is.EqualTo(5f));
            Assert.That(weapon.RequireAttributeAmounts[_intelligence.RuntimeSlot], Is.EqualTo(2f));
            Assert.That(skill.GetRequireAttributeAmounts(1)[_strength.RuntimeSlot], Is.EqualTo(4f));
            Assert.That(skill.GetRequireAttributeAmounts(99)[_intelligence.RuntimeSlot], Is.EqualTo(1f));

            AttributeAmounts attributes = default;
            attributes[_strength.RuntimeSlot] = 10f;
            attributes[_intelligence.RuntimeSlot] = 4f;
            GameDataHelpers.CombineAttributes(weapon.IncreaseAttributes, ref attributes, 3, 1f);
            Assert.That(attributes[_strength.RuntimeSlot], Is.EqualTo(12f));
            Assert.That(attributes[_intelligence.RuntimeSlot], Is.EqualTo(5.5f));
            Assert.That(attributes.GetWeightedAmount(skill.CacheEffectivenessAttributes), Is.EqualTo(17.25f));

            CharacterStats stats = attributes.GetStats() + weapon.GetIncreaseStats(3);
            Assert.That(stats.hp, Is.EqualTo(33f));
            Assert.That(stats.mp, Is.EqualTo(16.5f));

            RuntimeGameDataSlots.Clear();
            RuntimeGameDataSlots.Register(_intelligence);
            RuntimeGameDataSlots.Register(_strength);
            Assert.That(weapon.RequireAttributeAmounts[_strength.RuntimeSlot], Is.EqualTo(5f));
            Assert.That(weapon.RequireAttributeAmounts[_intelligence.RuntimeSlot], Is.EqualTo(2f));
            Assert.That(skill.CacheEffectivenessAttributes[_strength.RuntimeSlot], Is.EqualTo(0.75f));
            Assert.That(skill.CacheEffectivenessAttributes[_intelligence.RuntimeSlot], Is.EqualTo(1.5f));
        }

        [Test]
        public void MockWeaponAndSkillCombineDamageResistanceArmorAndInflictions()
        {
            WeaponItem weapon = CreateAsset<WeaponItem>();
            SetSerializedField(typeof(BaseEquipmentItem), weapon, "increaseDamages", new[]
            {
                new DamageIncremental { damageElement = _fire, amount = RangeAtLevel(3f, 5f, 1f, 2f) },
                new DamageIncremental { damageElement = _ice, amount = RangeAtLevel(2f, 4f) },
                new DamageIncremental { damageElement = _fire, amount = RangeAtLevel(1f, 1f) },
            });
            SetSerializedField(typeof(BaseEquipmentItem), weapon, "increaseResistances", new[]
            {
                new ResistanceIncremental { damageElement = _fire, amount = FloatAtLevel(0.1f, 0.05f) },
            });
            SetSerializedField(typeof(BaseEquipmentItem), weapon, "increaseArmors", new[]
            {
                new ArmorIncremental { damageElement = _ice, amount = FloatAtLevel(2f, 1f) },
            });

            Skill skill = CreateAsset<Skill>();
            skill.skillAttackType = Skill.SkillAttackType.Normal;
            skill.additionalDamageAmounts = new[]
            {
                new DamageIncremental { damageElement = _fire, amount = RangeAtLevel(4f, 6f, 1f, 2f) },
                new DamageIncremental { damageElement = _ice, amount = RangeAtLevel(1f, 3f) },
            };
            skill.weaponDamageInflictions = new[]
            {
                new DamageInflictionIncremental { damageElement = _fire, rate = FloatAtLevel(0.25f, 0.125f) },
                new DamageInflictionIncremental { damageElement = _ice, rate = FloatAtLevel(0.5f) },
            };
            skill.weaponDamageMultiplicator = FloatAtLevel(1f, 0.25f);

            DamageElementMinMaxFloatAmounts damages = default;
            GameDataHelpers.CombineDamages(weapon.IncreaseDamages, ref damages, 3, 1f);
            Assert.That(skill.TryGetIndexedAttackAdditionalDamageAmounts(null, 3, out DamageElementMinMaxFloatAmounts extra), Is.True);
            damages += extra;
            Assert.That(damages[_fire.RuntimeSlot].min, Is.EqualTo(12f));
            Assert.That(damages[_fire.RuntimeSlot].max, Is.EqualTo(20f));
            Assert.That(damages[_ice.RuntimeSlot].min, Is.EqualTo(3f));
            Assert.That(damages[_ice.RuntimeSlot].max, Is.EqualTo(7f));

            Assert.That(skill.TryGetIndexedAttackWeaponDamageInflictions(null, 3, out DamageElementFloatAmounts inflictions), Is.True);
            Assert.That(inflictions[_fire.RuntimeSlot], Is.EqualTo(0.5f));
            Assert.That(inflictions[_ice.RuntimeSlot], Is.EqualTo(0.5f));
            Assert.That(skill.TryGetAttackWeaponDamageMultiplicator(null, 3, out float multiplier), Is.True);
            Assert.That(multiplier, Is.EqualTo(1.5f));
            Assert.That((damages * multiplier)[_fire.RuntimeSlot].max, Is.EqualTo(30f));

            DamageElementFloatAmounts resistances = default;
            DamageElementFloatAmounts armors = default;
            GameDataHelpers.CombineResistances(weapon.IncreaseResistances, ref resistances, 3, 1f);
            GameDataHelpers.CombineArmors(weapon.IncreaseArmors, ref armors, 3, 1f);
            Assert.That(resistances[_fire.RuntimeSlot], Is.EqualTo(0.2f).Within(0.00001f));
            Assert.That(armors[_ice.RuntimeSlot], Is.EqualTo(4f));
            Assert.That(resistances.Contains(_ice.RuntimeSlot), Is.False);
            Assert.That(armors.Contains(_fire.RuntimeSlot), Is.False);
        }
    }
}
