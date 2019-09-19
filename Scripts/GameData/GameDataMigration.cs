using System.Collections.Generic;

namespace MultiplayerARPG
{
    public static class GameDataMigration
    {
        public static bool MigrateArmor(
            CharacterStats stats,
            ArmorAmount[] armors,
            out CharacterStats resultStats,
            out ArmorAmount[] resultArmors)
        {
            resultStats = stats;
            resultArmors = armors;

            if (resultStats.armor != 0)
            {
                List<ArmorAmount> tempArmors = new List<ArmorAmount>(resultArmors);
                tempArmors.Add(new ArmorAmount()
                {
                    amount = resultStats.armor
                });
                resultArmors = tempArmors.ToArray();

                CharacterStats tempStats = resultStats;
                tempStats.armor = 0;
                resultStats = tempStats;
                return true;
            }
            return false;
        }

        public static bool MigrateArmor(
            CharacterStatsIncremental increaseStats, 
            ArmorIncremental[] increaseArmors,
            out CharacterStatsIncremental resultIncreaseStats,
            out ArmorIncremental[] resultIncreaseArmors)
        {
            resultIncreaseStats = increaseStats;
            resultIncreaseArmors = increaseArmors;

            if (resultIncreaseStats.baseStats.armor != 0 ||
                resultIncreaseStats.statsIncreaseEachLevel.armor != 0)
            {
                List<ArmorIncremental> tempIncreaseArmors = new List<ArmorIncremental>(resultIncreaseArmors);
                tempIncreaseArmors.Add(new ArmorIncremental()
                {
                    amount = new IncrementalFloat()
                    {
                        baseAmount = resultIncreaseStats.baseStats.armor,
                        amountIncreaseEachLevel = resultIncreaseStats.statsIncreaseEachLevel.armor
                    }
                });
                resultIncreaseArmors = tempIncreaseArmors.ToArray();

                CharacterStatsIncremental tempIncreaseStats = resultIncreaseStats;
                tempIncreaseStats.baseStats.armor = 0;
                tempIncreaseStats.statsIncreaseEachLevel.armor = 0;
                resultIncreaseStats = tempIncreaseStats;
                return true;
            }
            return false;
        }

        public static bool MigrateBuffArmor(Buff buff, out Buff result)
        {
            result = buff;
            return MigrateArmor(result.increaseStats, result.increaseArmors, out result.increaseStats, out result.increaseArmors);
        }
    }
}
