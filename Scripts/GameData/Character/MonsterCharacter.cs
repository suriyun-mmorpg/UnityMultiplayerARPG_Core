using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public enum MonsterCharacteristic
    {
        Normal,
        Aggressive,
        Assist,
    }

    [System.Serializable]
    public struct MonsterCharacterAmount
    {
        public MonsterCharacter monster;
        public short amount;
    }

    [CreateAssetMenu(fileName = "Monster Character", menuName = "Create GameData/Monster Character", order = -4998)]
    public partial class MonsterCharacter : BaseCharacter
    {
        [Header("Monster Data")]
        [Tooltip("`Normal` will attack when being attacked, `Aggressive` will attack when enemy nearby, `Assist` will attack when other with same `Ally Id` being attacked.")]
        public MonsterCharacteristic characteristic;
        [Tooltip("This will work with assist characteristic only, to detect ally")]
        public ushort allyId;
        [Tooltip("This move speed will be applies when it's wandering. if it's going to chase enemy, stats'moveSpeed will be applies")]
        public float wanderMoveSpeed;
        [Tooltip("Range to see an enemy")]
        public float visualRange = 5f;
        [HideInInspector]
        public float deadHideDelay = 2f;
        [HideInInspector]
        public float deadRespawnDelay = 5f;

        [Header("Weapon/Attack Abilities")]
        public DamageInfo damageInfo;
        public DamageIncremental damageAmount;

        [Header("Killing Rewards")]
        public int randomExpMin;
        public int randomExpMax;
        public int randomGoldMin;
        public int randomGoldMax;
        public byte maxDropItems = 5;
        public ItemDrop[] randomItems;
        public ItemDropTable itemDropTable;

        public int RandomExp()
        {
            int min = randomExpMin;
            int max = randomExpMax;
            if (min > max)
                min = max;
            return Random.Range(min, max);
        }

        public int RandomGold()
        {
            int min = randomGoldMin;
            int max = randomGoldMax;
            if (min > max)
                min = max;
            return Random.Range(min, max);
        }

        public void RandomItems(System.Action<Item, short> onRandomItem)
        {
            int countDrops = 0;
            ItemDrop randomItem;
            int loopCounter;

            for (loopCounter = 0; loopCounter < randomItems.Length && countDrops < maxDropItems; ++loopCounter)
            {
                ++countDrops;
                randomItem = randomItems[loopCounter];
                if (randomItem.item == null ||
                    randomItem.amount == 0 ||
                    !GameInstance.Items.ContainsKey(randomItem.item.DataId) ||
                    Random.value > randomItem.dropRate)
                    continue;

                onRandomItem.Invoke(randomItem.item, randomItem.amount);
            }

            // Random drop item from table
            if (itemDropTable != null && countDrops < maxDropItems)
            {
                for (loopCounter = 0; loopCounter < itemDropTable.randomItems.Length && countDrops < maxDropItems; ++loopCounter)
                {
                    ++countDrops;
                    randomItem = itemDropTable.randomItems[loopCounter];
                    if (randomItem.item == null ||
                        randomItem.amount == 0 ||
                        !GameInstance.Items.ContainsKey(randomItem.item.DataId) ||
                        Random.value > randomItem.dropRate)
                        continue;

                    onRandomItem.Invoke(randomItem.item, randomItem.amount);
                }
            }
        }
    }
}
