namespace MultiplayerARPG
{
    [System.Serializable]
    public class Npcs
    {
        public BaseMapInfo mapInfo;
        public Npc[] npcs = new Npc[0];

#if UNITY_EDITOR
        public bool ValidateAddressableHashAssetIDs()
        {
            bool hasChanges = false;
            foreach (Npc npc in npcs)
            {
                hasChanges |= npc.ValidateAddressableHashAssetID();
            }
            return hasChanges;
        }
#endif
    }
}
