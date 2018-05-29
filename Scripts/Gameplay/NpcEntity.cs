using System.Collections;
using System.Collections.Generic;

public class NpcEntity : RpgNetworkEntity
{
    public NpcDialog startDialog;

    protected override void Awake()
    {
        base.Awake();
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.npcTag;
    }
}
