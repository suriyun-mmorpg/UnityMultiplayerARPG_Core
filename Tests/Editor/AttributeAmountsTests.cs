using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace MultiplayerARPG.Tests
{
    public class AttributeAmountsTests
    {
        private readonly List<Attribute> _attributes = new List<Attribute>();

        [SetUp]
        public void SetUp()
        {
            RuntimeGameDataSlots.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            RuntimeGameDataSlots.Clear();
            foreach (Attribute attribute in _attributes)
                UnityEngine.Object.DestroyImmediate(attribute);
            _attributes.Clear();
        }

        private Attribute RegisterAttribute()
        {
            Attribute attribute = ScriptableObject.CreateInstance<Attribute>();
            _attributes.Add(attribute);
            RuntimeGameDataSlots.Register(attribute);
            return attribute;
        }

        [Test]
        public void EveryConfiguredSlotCombinesAndMultiplies()
        {
            AttributeAmounts left = default;
            AttributeAmounts right = default;
            for (int slot = 0; slot < AttributeAmounts.Capacity; ++slot)
            {
                Assert.That(RegisterAttribute().RuntimeSlot, Is.EqualTo(slot));
                left.Add(slot, slot + 1);
                right[slot] = 2f;
            }

            AttributeAmounts sum = left + right;
            AttributeAmounts product = left * right;
            AttributeAmounts scaled = left * 0.5f;
            for (int slot = 0; slot < AttributeAmounts.Capacity; ++slot)
            {
                Assert.That(sum.Contains(slot), Is.True);
                Assert.That(sum[slot], Is.EqualTo(slot + 3f), $"Sum at slot {slot}");
                Assert.That(product[slot], Is.EqualTo((slot + 1) * 2f), $"Product at slot {slot}");
                Assert.That(scaled[slot], Is.EqualTo((slot + 1) * 0.5f), $"Scale at slot {slot}");
                Assert.That(left[slot], Is.EqualTo(slot + 1f), $"Source changed at slot {slot}");
            }

            Attribute overflow = ScriptableObject.CreateInstance<Attribute>();
            _attributes.Add(overflow);
            Assert.Throws<InvalidOperationException>(() => RuntimeGameDataSlots.Register(overflow));
            Assert.Throws<IndexOutOfRangeException>(() => left.Add(AttributeAmounts.Capacity, 1f));
        }

        [Test]
        public void SparseMultiplicationKeepsOnlySharedSlotsIncludingExplicitZero()
        {
            RegisterAttribute();
            RegisterAttribute();
            RegisterAttribute();

            AttributeAmounts left = default;
            left[0] = 6f;
            left[1] = 4f;
            left[2] = 0f;
            AttributeAmounts right = default;
            right[0] = 0.5f;
            right[2] = 3f;

            AttributeAmounts product = left * right;
            Assert.That(product.OccupiedMask, Is.EqualTo((1u << 0) | (1u << 2)));
            Assert.That(product[0], Is.EqualTo(3f));
            Assert.That(product[1], Is.EqualTo(0f));
            Assert.That(product.Contains(2), Is.True);
            Assert.That(product[2], Is.EqualTo(0f));
            Assert.That(left.Contains(1), Is.True);

            AttributeAmounts sum = left + right;
            Assert.That(sum.OccupiedMask, Is.EqualTo(7u));
            Assert.That(sum[0], Is.EqualTo(6.5f));
            Assert.That(sum[1], Is.EqualTo(4f));
            Assert.That(sum.Contains(2), Is.True);

            left.Remove(1);
            Assert.That(left.Contains(1), Is.False);
            left.Clear();
            Assert.That(left.OccupiedMask, Is.Zero);
        }

        [Test]
        public void RatesAndWeightedAmountsUseOnlySharedSlots()
        {
            RegisterAttribute();
            RegisterAttribute();
            RegisterAttribute();
            AttributeAmounts values = default;
            values[0] = 10f;
            values[1] = 4f;
            AttributeAmounts rates = default;
            rates[0] = 0.25f;
            rates[2] = 2f;

            Assert.That(values.GetWeightedAmount(rates), Is.EqualTo(2.5f));
            values.ApplyRates(rates);
            Assert.That(values[0], Is.EqualTo(12.5f));
            Assert.That(values[1], Is.EqualTo(4f));
            Assert.That(values.Contains(2), Is.False);
        }

        [Test]
        public void CombineIgnoresSlotsOutsideCurrentRegistration()
        {
            RegisterAttribute();
            RegisterAttribute();
            RegisterAttribute();
            AttributeAmounts stale = default;
            stale[2] = 9f;

            RuntimeGameDataSlots.Clear();
            RegisterAttribute();
            AttributeAmounts result = default;
            result.Combine(stale);
            Assert.That(result.OccupiedMask, Is.Zero);
        }
    }
}
