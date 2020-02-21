namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ItemDropByWeight
    {
        public BaseItem item;
        public float amountPerDamage;
        public int randomWeight;
    }
}
