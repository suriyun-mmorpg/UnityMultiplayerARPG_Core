using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MonsterCharacteristic
{
    Normal,
    Aggressive,
    Assist,
}

[CreateAssetMenu(fileName = "MonsterCharacterDatabase", menuName = "Create GameData/MonsterCharacterDatabase")]
public class MonsterCharacterDatabase : BaseCharacterDatabase
{
    [Header("Monster Data")]
    public MonsterCharacteristic characteristic;
    [Tooltip("If this is TRUE, character will not move")]
    public bool Immovable;
    [Tooltip("This will work with assist characteristic only, to detect ally")]
    public ushort allyId;
    public float visualRange = 5f;
    [Tooltip("Level will be used to show only, not calculating with stats/skills")]
    public int level;
    public CharacterStats stats;

    [Header("Killing Rewards")]
    public int randomExpMin;
    public int randomExpMax;
    public int randomGoldMin;
    public int randomGoldMax;
    public ItemDrop[] randomItems;

    public int RandomExp()
    {
        var min = randomExpMin;
        var max = randomExpMax;
        if (min > max)
            min = max;
        return Random.Range(min, max);
    }

    public int RandomGold()
    {
        var min = randomGoldMin;
        var max = randomGoldMax;
        if (min > max)
            min = max;
        return Random.Range(min, max);
    }

    public List<ItemAmountPair> RandomItems()
    {
        var rewards = new List<ItemAmountPair>();
        foreach (var randomItem in randomItems)
        {
            if (randomItem.item == null ||
                randomItem.amount == 0 ||
                !GameInstance.Items.ContainsKey(randomItem.item.Id) ||
                Random.value > randomItem.dropRate)
                continue;
                rewards.Add(new ItemAmountPair()
                {
                    item = randomItem.item,
                    amount = randomItem.amount,
                });
        }
        return rewards;
    }

    public override CharacterStats GetCharacterStats(int level)
    {
        return stats;
    }
}
