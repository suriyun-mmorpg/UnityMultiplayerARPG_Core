namespace MultiplayerARPG
{
    public struct DamageHitObjectInfo
    {
        public uint ObjectId { get; set; }

        public override int GetHashCode()
        {
            return ObjectId.GetHashCode();
        }
    }
}
