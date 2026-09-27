using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace MultiplayerARPG.Tests
{
    public class DamageElementAmountsTests
    {
        private readonly List<DamageElement> _elements = new List<DamageElement>();

        [SetUp]
        public void SetUp()
        {
            RuntimeGameDataSlots.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            RuntimeGameDataSlots.Clear();
            foreach (DamageElement element in _elements)
                UnityEngine.Object.DestroyImmediate(element);
            _elements.Clear();
        }

        private DamageElement CreateElement()
        {
            DamageElement element = ScriptableObject.CreateInstance<DamageElement>();
            _elements.Add(element);
            return element;
        }

        private void RegisterElements(int count)
        {
            RuntimeGameDataSlots.RegisterDefaultDamageElement(CreateElement());
            for (int slot = 1; slot < count; ++slot)
                RuntimeGameDataSlots.Register(CreateElement());
        }

        [Test]
        public void AllConfiguredSlotsCombineAndMultiplyFloatAndMinMaxAmounts()
        {
            RegisterElements(DamageElementFloatAmounts.Capacity);
            Assert.That(DamageElementFloatAmounts.Capacity, Is.EqualTo(DamageElementMinMaxFloatAmounts.Capacity));
            Assert.That(RuntimeGameDataSlots.GetDamageElement(0), Is.SameAs(_elements[0]));

            DamageElementFloatAmounts floats = default;
            DamageElementFloatAmounts floatFactors = default;
            DamageElementMinMaxFloatAmounts ranges = default;
            DamageElementMinMaxFloatAmounts rangeFactors = default;
            for (int slot = 0; slot < DamageElementFloatAmounts.Capacity; ++slot)
            {
                Assert.That(_elements[slot].RuntimeSlot, Is.EqualTo(slot));
                floats.Add(slot, slot + 1);
                floatFactors[slot] = 2f;
                ranges[slot] = new MinMaxFloat { min = slot + 1, max = (slot + 1) * 2 };
                rangeFactors[slot] = new MinMaxFloat { min = 2f, max = 3f };
            }

            DamageElementFloatAmounts floatSum = floats + floatFactors;
            DamageElementFloatAmounts floatProduct = floats * floatFactors;
            DamageElementFloatAmounts floatScaled = floats * 0.5f;
            DamageElementMinMaxFloatAmounts rangeSum = ranges + rangeFactors;
            DamageElementMinMaxFloatAmounts rangeProduct = ranges * rangeFactors;
            DamageElementMinMaxFloatAmounts rangeScaled = ranges * 0.5f;
            for (int slot = 0; slot < DamageElementFloatAmounts.Capacity; ++slot)
            {
                float amount = slot + 1f;
                Assert.That(floatSum[slot], Is.EqualTo(amount + 2f), $"Float sum at slot {slot}");
                Assert.That(floatProduct[slot], Is.EqualTo(amount * 2f), $"Float product at slot {slot}");
                Assert.That(floatScaled[slot], Is.EqualTo(amount * 0.5f), $"Float scale at slot {slot}");
                Assert.That(rangeSum[slot].min, Is.EqualTo(amount + 2f), $"Range min sum at slot {slot}");
                Assert.That(rangeSum[slot].max, Is.EqualTo(amount * 2f + 3f), $"Range max sum at slot {slot}");
                Assert.That(rangeProduct[slot].min, Is.EqualTo(amount * 2f), $"Range min product at slot {slot}");
                Assert.That(rangeProduct[slot].max, Is.EqualTo(amount * 6f), $"Range max product at slot {slot}");
                Assert.That(rangeScaled[slot].min, Is.EqualTo(amount * 0.5f), $"Range min scale at slot {slot}");
                Assert.That(rangeScaled[slot].max, Is.EqualTo(amount), $"Range max scale at slot {slot}");
            }

            float expectedMin = DamageElementFloatAmounts.Capacity * (DamageElementFloatAmounts.Capacity + 1) / 2f;
            Assert.That(ranges.Sum().min, Is.EqualTo(expectedMin));
            Assert.That(ranges.Sum().max, Is.EqualTo(expectedMin * 2f));

            DamageElement overflow = CreateElement();
            Assert.Throws<InvalidOperationException>(() => RuntimeGameDataSlots.Register(overflow));
            Assert.Throws<IndexOutOfRangeException>(() => floats.Add(DamageElementFloatAmounts.Capacity, 1f));
            Assert.Throws<IndexOutOfRangeException>(() => ranges.Add(DamageElementMinMaxFloatAmounts.Capacity, default));
        }

        [Test]
        public void SparseProductsKeepSharedSlotsAndExplicitZero()
        {
            RegisterElements(3);
            DamageElementFloatAmounts floats = default;
            floats[0] = 0f;
            floats[1] = 6f;
            DamageElementFloatAmounts factors = default;
            factors[0] = 2f;
            factors[2] = 3f;

            DamageElementFloatAmounts product = floats * factors;
            Assert.That(product.OccupiedMask, Is.EqualTo(1u));
            Assert.That(product.Contains(0), Is.True);
            Assert.That(product[0], Is.Zero);
            Assert.That(product.Contains(1), Is.False);
            Assert.That((floats + factors).OccupiedMask, Is.EqualTo(7u));

            DamageElementMinMaxFloatAmounts ranges = default;
            ranges[0] = default;
            ranges[1] = new MinMaxFloat { min = 4f, max = 8f };
            DamageElementMinMaxFloatAmounts rangeFactors = default;
            rangeFactors[0] = new MinMaxFloat { min = 2f, max = 3f };
            rangeFactors[2] = new MinMaxFloat { min = 1f, max = 1f };

            DamageElementMinMaxFloatAmounts rangeProduct = ranges * rangeFactors;
            Assert.That(rangeProduct.OccupiedMask, Is.EqualTo(1u));
            Assert.That(rangeProduct.Contains(0), Is.True);
            Assert.That(rangeProduct[0].min, Is.Zero);
            Assert.That(rangeProduct[0].max, Is.Zero);
            Assert.That(rangeProduct.Contains(1), Is.False);
            Assert.That((ranges + rangeFactors).OccupiedMask, Is.EqualTo(7u));
        }

        [Test]
        public void ApplyRatesChangesOnlySharedFloatSlots()
        {
            RegisterElements(3);
            DamageElementFloatAmounts values = default;
            values[0] = 10f;
            values[1] = 4f;
            DamageElementFloatAmounts rates = default;
            rates[0] = 0.25f;
            rates[2] = 2f;

            values.ApplyRates(rates);
            Assert.That(values[0], Is.EqualTo(12.5f));
            Assert.That(values[1], Is.EqualTo(4f));
            Assert.That(values.Contains(2), Is.False);
        }
    }
}
