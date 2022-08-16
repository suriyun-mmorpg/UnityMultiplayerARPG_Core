namespace MultiplayerARPG
{
    public static class DamageableEntityExtensions
    {
        public static bool IsDead(this IDamageableEntity damageableEntity)
        {
            return damageableEntity.CurrentHp <= 0;
        }

        public static bool IsHideOrDead(this IDamageableEntity damageableEntity)
        {
            return damageableEntity.IsDead() || damageableEntity.IsHide();
        }
    }
}
