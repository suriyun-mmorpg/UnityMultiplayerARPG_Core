namespace MultiplayerARPG
{
    [System.Serializable]
    public struct ArmorAmount
    {
        public DamageElement damageElement;
        public float amount;
    }

    [System.Serializable]
    public struct ArmorIncremental
    {
        public DamageElement damageElement;
        public IncrementalFloat amount;
    }
}
