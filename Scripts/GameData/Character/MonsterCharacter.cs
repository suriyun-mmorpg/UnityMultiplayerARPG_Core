using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

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

    [CreateAssetMenu(fileName = "Monster Character", menuName = "Create GameData/Monster Character")]
    public class MonsterCharacter : BaseCharacter
    {
        [Header("Monster Data")]
        public MonsterCharacteristic characteristic;
        [Tooltip("This will work with assist characteristic only, to detect ally")]
        public ushort allyId;
        [Tooltip("This move speed will be applies when it's wandering. if it's going to chase enemy, stats'moveSpeed will be applies")]
        public float wanderMoveSpeed;
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
        public ItemDrop[] randomItems;

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

        public List<ItemAmount> RandomItems()
        {
            List<ItemAmount> rewards = new List<ItemAmount>();
            foreach (ItemDrop randomItem in randomItems)
            {
                if (randomItem.item == null ||
                    randomItem.amount == 0 ||
                    !GameInstance.Items.ContainsKey(randomItem.item.DataId) ||
                    Random.value > randomItem.dropRate)
                    continue;
                rewards.Add(new ItemAmount()
                {
                    item = randomItem.item,
                    amount = randomItem.amount,
                });
            }
            return rewards;
        }
    }
}
