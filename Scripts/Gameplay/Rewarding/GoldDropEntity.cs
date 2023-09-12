namespace MultiplayerARPG
{
    public class GoldDropEntity : BaseRewardDropEntity, IPickupActivatableEntity
    {
        protected override bool ProceedPickingUpAtServer_Implementation(BaseCharacterEntity characterEntity, out UITextKeys message)
        {
            if (characterEntity is BasePlayerCharacterEntity playerCharacterEntity)
                playerCharacterEntity.Gold.Increase(amount);
            else if (characterEntity is BaseMonsterCharacterEntity monsterCharacterEntity && monsterCharacterEntity.Summoner is BasePlayerCharacterEntity summonerCharacterEntity)
                summonerCharacterEntity.Gold.Increase(amount);
            message = UITextKeys.NONE;
            return true;
        }
    }
}
