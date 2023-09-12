namespace MultiplayerARPG
{
    public class ExpDropEntity : BaseRewardDropEntity, IPickupActivatableEntity
    {
        protected override bool ProceedPickingUpAtServer_Implementation(BaseCharacterEntity characterEntity, out UITextKeys message)
        {
            characterEntity.Exp.Increase(amount);
            message = UITextKeys.NONE;
            return true;
        }
    }
}
