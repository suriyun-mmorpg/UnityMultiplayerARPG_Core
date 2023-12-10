namespace MultiplayerARPG
{
    public struct UIStatusEffectResistanceAmountData
    {
        public StatusEffect statusEffect;
        public float amount;
        public UIStatusEffectResistanceAmountData(StatusEffect statusEffect, float amount)
        {
            this.statusEffect = statusEffect;
            this.amount = amount;
        }
    }
}
