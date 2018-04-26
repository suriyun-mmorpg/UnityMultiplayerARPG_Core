using System.Collections;
using System.Collections.Generic;

public class NpcEntity : RpgNetworkEntity
{
    public NpcDialog startDialog;

    private void Awake()
    {
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.npcTag;
    }
}
