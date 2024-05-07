using LiteNetLibManager;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class AssetReferenceBaseUISceneGameplay : AssetReferenceComponent<BaseUISceneGameplay>
    {
        public AssetReferenceBaseUISceneGameplay(string guid) : base(guid)
        {
        }
    }
}