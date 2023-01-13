namespace MultiplayerARPG
{
    public partial interface ISkillItem : IUsableItem
    {
        /// <summary>
        /// Skill which will be activated when use this item
        /// </summary>
        BaseSkill UsingSkill { get; }
        /// <summary>
        /// Activating skill's level
        /// </summary>
        int UsingSkillLevel { get; }
    }
}
