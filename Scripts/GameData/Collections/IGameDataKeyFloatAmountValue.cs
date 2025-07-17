namespace MultiplayerARPG
{
    public interface IGameDataKeyFloatAmountValue<TType>
        where TType : BaseGameData
    {
        TType Key { get; }
        float Value { get; }
    }
}
