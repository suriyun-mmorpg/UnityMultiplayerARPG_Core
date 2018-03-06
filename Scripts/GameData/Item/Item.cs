using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Create GameData/Item")]
public class Item : BaseGameData
{
    public int sellPrice;
    [Range(1, 1000)]
    public int maxStack = 1;
    public float weight;
}

[System.Serializable]
public struct ItemAmountPair
{
    public Item item;
    public int amount;
}
