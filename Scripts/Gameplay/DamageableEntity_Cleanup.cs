namespace MultiplayerARPG
{
    public partial class DamageableEntity
    {
        public override void Clean(bool isObjectDestroyed)
        {
            base.Clean(isObjectDestroyed);
            if (isObjectDestroyed)
            {
                combatTextTransform = null;
                opponentAimTransform = null;
                onNormalDamageHit?.RemoveAllListeners();
                onNormalDamageHit = null;
                onCriticalDamageHit?.RemoveAllListeners();
                onCriticalDamageHit = null;
                onBlockedDamageHit?.RemoveAllListeners();
                onBlockedDamageHit = null;
                onDamageMissed?.RemoveAllListeners();
                onDamageMissed = null;
                onCurrentHpChange = null;
                onReceiveDamage = null;
                onReceivedDamage = null;
                HitBoxes?.Nullify();
                HitBoxes = null;
            }
            SafeArea = null;
        }
    }
}