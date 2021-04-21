namespace MultiplayerARPG
{
    public struct DamageHitObjectInfo
    {
        public uint ObjectId { get; set; }
        public int HitBoxIndex { get; set; }

        public override bool Equals(object obj)
        {
            return ObjectId == ((DamageHitObjectInfo)obj).ObjectId;
        }

        public override int GetHashCode()
        {
            return ObjectId.GetHashCode();
        }
    }
}
