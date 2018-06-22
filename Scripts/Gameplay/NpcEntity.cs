public sealed class NpcEntity : RpgNetworkEntity
{
    public NpcDialog startDialog;

    protected override void Awake()
    {
        base.Awake();
        var gameInstance = GameInstance.Singleton;
        gameObject.tag = gameInstance.npcTag;
    }
}
