namespace MultiplayerARPG
{
    public enum BuffRemoveReasons
    {
        Unset,
        Timeout,
        CharacterDead,
        RemoveByOtherBuffs,
        RemoveByAttackRemoveChance,
        RemoveByAttackedRemoveChance,
        RemoveByUseSkillRemoveChance,
        RemoveByUseItemRemoveChance,
        RemoveByPickupChance,
    }
}