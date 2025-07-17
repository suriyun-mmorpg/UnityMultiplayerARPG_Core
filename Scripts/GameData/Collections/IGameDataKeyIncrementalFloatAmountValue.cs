namespace MultiplayerARPG
{
    public interface IGameDataKeyIncrementalFloatAmountValue<TType>
        where TType : BaseGameData
    {
        TType Key { get; }
        IncrementalFloat Value { get; }
    }
}
