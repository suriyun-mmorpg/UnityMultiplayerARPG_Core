public sealed class NpcEntity : RpgNetworkEntity
{
    public NpcDialog startDialog;

    protected override void Awake()
    {
        base.Awake();
        gameObject.tag = gameInstance.npcTag;
    }
}
