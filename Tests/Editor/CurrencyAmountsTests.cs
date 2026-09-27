using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MultiplayerARPG.Tests
{
    public class CurrencyAmountsTests
    {
        private readonly List<UnityEngine.Object> _assets = new List<UnityEngine.Object>();

        [SetUp]
        public void SetUp()
        {
            RuntimeGameDataSlots.Clear();
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
            asset.name = typeof(T).Name + "-" + _assets.Count;
            _assets.Add(asset);
            return asset;
        }

        [Test]
        public void EveryConfiguredSlotSupportsArithmeticAndCapacityChecks()
        {
            CurrencyAmounts left = default;
            CurrencyAmounts right = default;
            for (int slot = 0; slot < CurrencyAmounts.Capacity; ++slot)
            {
                Currency currency = CreateAsset<Currency>();
                RuntimeGameDataSlots.Register(currency);
                Assert.That(currency.RuntimeSlot, Is.EqualTo(slot));
                left[slot] = slot + 2;
                right[slot] = 1;
            }

            CurrencyAmounts sum = left + right;
            CurrencyAmounts difference = left - right;
            CurrencyAmounts doubled = left * 2;
            CurrencyAmounts halved = left * 0.5f;
            for (int slot = 0; slot < CurrencyAmounts.Capacity; ++slot)
            {
                Assert.That(sum[slot], Is.EqualTo(slot + 3));
                Assert.That(difference[slot], Is.EqualTo(slot + 1));
                Assert.That(doubled[slot], Is.EqualTo((slot + 2) * 2));
                Assert.That(halved[slot], Is.EqualTo(Mathf.CeilToInt((slot + 2) * 0.5f)));
                Assert.That(left[slot], Is.EqualTo(slot + 2), $"Source changed at slot {slot}");
            }

            Currency overflow = CreateAsset<Currency>();
            Assert.Throws<InvalidOperationException>(() => RuntimeGameDataSlots.Register(overflow));
            Assert.Throws<IndexOutOfRangeException>(() => left.Add(CurrencyAmounts.Capacity, 1));
        }

        [Test]
        public void DuplicateEntriesRoundIndividuallyAndDictionaryViewKeepsExplicitZero()
        {
            Currency coins = CreateAsset<Currency>();
            Currency gems = CreateAsset<Currency>();
            RuntimeGameDataSlots.Register(coins);
            RuntimeGameDataSlots.Register(gems);
            CurrencyAmount[] entries =
            {
                new CurrencyAmount { currency = coins, amount = 3 },
                new CurrencyAmount { currency = coins, amount = 3 },
                new CurrencyAmount { currency = gems, amount = -1 },
                new CurrencyAmount { currency = null, amount = 100 },
            };

            CurrencyAmounts amounts = default;
            GameDataHelpers.CombineCurrencies(entries, ref amounts, 0.5f);
            Assert.That(amounts[coins.RuntimeSlot], Is.EqualTo(4));
            Assert.That(amounts[gems.RuntimeSlot], Is.Zero);
            Assert.That(amounts.Contains(gems.RuntimeSlot), Is.True);

            Dictionary<Currency, int> view = new Dictionary<Currency, int>();
            amounts.CopyTo(view);
            Assert.That(view[coins], Is.EqualTo(4));
            Assert.That(view[gems], Is.Zero);
            Assert.That(view.ToCurrencyAmounts()[coins.RuntimeSlot], Is.EqualTo(4));

            CurrencyAmounts fromList = default;
            GameDataHelpers.CombineCurrencies(new List<CurrencyAmount>(entries), ref fromList, 0.5f);
            Assert.That(fromList.OccupiedMask, Is.EqualTo(amounts.OccupiedMask));
            Assert.That(fromList[coins.RuntimeSlot], Is.EqualTo(4));
            amounts.Remove(gems.RuntimeSlot);
            Assert.That(amounts.Contains(gems.RuntimeSlot), Is.False);
        }

        [Test]
        public void MockSkillQuestAndGuildRequirementsRebuildAfterSlotRegistrationChanges()
        {
            Currency coins = CreateAsset<Currency>();
            Currency gems = CreateAsset<Currency>();
            RuntimeGameDataSlots.Register(coins);
            RuntimeGameDataSlots.Register(gems);
            CurrencyAmount[] prices =
            {
                new CurrencyAmount { currency = coins, amount = 3 },
                new CurrencyAmount { currency = coins, amount = 2 },
                new CurrencyAmount { currency = gems, amount = 1 },
            };

            Skill skill = CreateAsset<Skill>();
            skill.requirementEachLevels.Add(new SkillRequirementEntry());
            skill.requirementEachLevels.Add(new SkillRequirementEntry { currencyAmounts = prices });
            Quest quest = CreateAsset<Quest>();
            quest.rewardCurrencies = prices;
            SocialSystemSetting guild = CreateAsset<SocialSystemSetting>();
            FieldInfo guildCurrencies = typeof(SocialSystemSetting).GetField("createGuildRequireCurrencies", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(guildCurrencies, Is.Not.Null);
            guildCurrencies.SetValue(guild, prices);

            Assert.That(skill.GetRequireCurrencyAmounts(1)[coins.RuntimeSlot], Is.EqualTo(5));
            Assert.That(skill.GetRequireCurrencyAmounts(99)[gems.RuntimeSlot], Is.EqualTo(1));
            Assert.That(quest.IndexedRewardCurrencies[coins.RuntimeSlot], Is.EqualTo(5));
            Assert.That(guild.IndexedCreateGuildRequireCurrencies[gems.RuntimeSlot], Is.EqualTo(1));
            Assert.That(guild.CreateGuildRequireCurrencies[coins], Is.EqualTo(5));

            RuntimeGameDataSlots.Clear();
            RuntimeGameDataSlots.Register(gems);
            RuntimeGameDataSlots.Register(coins);
            Assert.That(quest.IndexedRewardCurrencies[coins.RuntimeSlot], Is.EqualTo(5));
            Assert.That(guild.IndexedCreateGuildRequireCurrencies[gems.RuntimeSlot], Is.EqualTo(1));
            Assert.That(guild.CreateGuildRequireCurrencies[coins], Is.EqualTo(5));
        }
    }
}
