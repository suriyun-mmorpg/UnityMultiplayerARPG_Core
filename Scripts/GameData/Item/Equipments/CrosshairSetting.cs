namespace MultiplayerARPG
{
    [System.Serializable]
    public struct CrosshairSetting
    {
        public bool hidden;
        public float expandPerFrameWhileMoving;
        public float expandPerFrameWhileAttacking;
        public float shrinkPerFrame;
        public float minSpread;
        public float maxSpread;
        public float recoil;
    }
}
