namespace MultiplayerARPG
{
    public struct UIBuffData
    {
        public Buff buff;
        public short targetLevel;
        public UIBuffData(Buff buff, short targetLevel)
        {
            this.buff = buff;
            this.targetLevel = targetLevel;
        }
    }
}
