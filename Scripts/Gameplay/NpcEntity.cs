using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcEntity : RpgNetworkEntity
{
    private void Awake()
    {
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.npcTag;
    }
}
