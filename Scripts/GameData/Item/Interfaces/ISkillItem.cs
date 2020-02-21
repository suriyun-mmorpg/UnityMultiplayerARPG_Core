namespace MultiplayerARPG
{
    public partial interface ISkillItem : IUsableItem
    {
        BaseSkill UsingSkill { get; }
        short UsingSkillLevel { get; }
    }
}
