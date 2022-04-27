namespace MultiplayerARPG
{
    [System.Flags]
    public enum SimulateActionTriggerState : byte
    {
        None = 0,
        IsLeftHand = 1 << 0,
        IsSkill = 1 << 1,
    }
}
