namespace MultiplayerARPG
{
    public interface IAttackRecoiler
    {
        public float DefaultDuration { get; }
        public void PlayRecoiling();
    }
}
