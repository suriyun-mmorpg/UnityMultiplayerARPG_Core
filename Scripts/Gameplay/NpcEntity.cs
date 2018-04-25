using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcEntity : RpgNetworkEntity
{
    public string title;
    public NpcDialog startDialog;

    private void Awake()
    {
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.npcTag;
    }
}
