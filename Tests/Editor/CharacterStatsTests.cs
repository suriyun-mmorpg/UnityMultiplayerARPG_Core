using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace MultiplayerARPG.Tests
{
    public class CharacterStatsTests
    {
        private CharacterStatsDelegate _increaseHook;
        private CharacterStatsDelegate _decreaseHook;
        private CharacterStatsDelegate _multiplyHook;
        private CharacterStatsAndNumberDelegate _multiplyNumberHook;

        [SetUp]
        public void SetUp()
        {
            _increaseHook = GameExtensionInstance.onIncreaseCharacterStats;
            _decreaseHook = GameExtensionInstance.onDecreaseCharacterStats;
            _multiplyHook = GameExtensionInstance.onMultiplyCharacterStats;
            _multiplyNumberHook = GameExtensionInstance.onMultiplyCharacterStatsWithNumber;
            GameExtensionInstance.onIncreaseCharacterStats = null;
            GameExtensionInstance.onDecreaseCharacterStats = null;
            GameExtensionInstance.onMultiplyCharacterStats = null;
            GameExtensionInstance.onMultiplyCharacterStatsWithNumber = null;
        }

        [TearDown]
        public void TearDown()
        {
            GameExtensionInstance.onIncreaseCharacterStats = _increaseHook;
            GameExtensionInstance.onDecreaseCharacterStats = _decreaseHook;
            GameExtensionInstance.onMultiplyCharacterStats = _multiplyHook;
            GameExtensionInstance.onMultiplyCharacterStatsWithNumber = _multiplyNumberHook;
        }

        [Test]
        public void OperatorsUpdateEveryFloatStatAndLeaveOperandsUnchanged()
        {
            FieldInfo[] fields = typeof(CharacterStats)
                .GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Where(field => field.FieldType == typeof(float))
                .OrderBy(field => field.Name)
                .ToArray();
            Assert.That(fields.Length, Is.GreaterThan(40));

            object leftBox = new CharacterStats();
            object rightBox = new CharacterStats();
            for (int i = 0; i < fields.Length; ++i)
            {
                fields[i].SetValue(leftBox, (float)(i + 2));
                fields[i].SetValue(rightBox, (float)(i % 5 - 2));
            }
            CharacterStats left = (CharacterStats)leftBox;
            CharacterStats right = (CharacterStats)rightBox;

            CharacterStats sum = left + right;
            CharacterStats difference = left - right;
            CharacterStats product = left * right;
            CharacterStats scaled = left * -0.5f;
            for (int i = 0; i < fields.Length; ++i)
            {
                float a = i + 2;
                float b = i % 5 - 2;
                Assert.That(fields[i].GetValue(sum), Is.EqualTo(a + b), $"Add {fields[i].Name}");
                Assert.That(fields[i].GetValue(difference), Is.EqualTo(a - b), $"Subtract {fields[i].Name}");
                Assert.That(fields[i].GetValue(product), Is.EqualTo(a * b), $"Multiply {fields[i].Name}");
                Assert.That(fields[i].GetValue(scaled), Is.EqualTo(a * -0.5f), $"Scale {fields[i].Name}");
                Assert.That(fields[i].GetValue(left), Is.EqualTo(a), $"Left operand changed: {fields[i].Name}");
                Assert.That(fields[i].GetValue(right), Is.EqualTo(b), $"Right operand changed: {fields[i].Name}");
            }
        }

        [Test]
        public void OperatorsInvokeExtensionHooks()
        {
            CharacterStats left = new CharacterStats { hp = 5f };
            CharacterStats right = new CharacterStats { hp = 2f };
            GameExtensionInstance.onIncreaseCharacterStats = (ref CharacterStats value, CharacterStats other) => value.hp += 10f;
            GameExtensionInstance.onDecreaseCharacterStats = (ref CharacterStats value, CharacterStats other) => value.hp += 20f;
            GameExtensionInstance.onMultiplyCharacterStats = (ref CharacterStats value, CharacterStats other) => value.hp += 30f;
            GameExtensionInstance.onMultiplyCharacterStatsWithNumber = (ref CharacterStats value, float multiplier) => value.hp += 40f;

            Assert.That((left + right).hp, Is.EqualTo(17f));
            Assert.That((left - right).hp, Is.EqualTo(23f));
            Assert.That((left * right).hp, Is.EqualTo(40f));
            Assert.That((left * 2f).hp, Is.EqualTo(50f));
            Assert.That(left.hp, Is.EqualTo(5f));
        }
    }
}
