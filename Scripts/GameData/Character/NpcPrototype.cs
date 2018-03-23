using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NpcCharacteristic
{
    Normal,
    Aggressive,
    Conversate,
}

[CreateAssetMenu(fileName = "MonsterPrototype", menuName = "Create GameData/MonsterPrototype")]
public class NpcPrototype : CharacterPrototype
{
    [Header("Monster Data")]
    public NpcCharacteristic characteristic;
    public float visualRange = 5f;
    public float randomWanderDurationMin = 1f;
    public float randomWanderDurationMax = 5f;
    public float randomWanderRadius = 5f;
    [Tooltip("Level will be used to show only, not calculating with stats/skills")]
    public int level;
    public CharacterStats stats;
    public Skill[] skills;

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
}
