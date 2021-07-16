namespace MultiplayerARPG
{
    public struct CharacterRecoveryData
    {
        public float recoveryingHp;
        public float recoveryingMp;
        public float recoveryingStamina;
        public float recoveryingFood;
        public float recoveryingWater;
        public float decreasingHp;
        public float decreasingMp;
        public float decreasingStamina;
        public float decreasingFood;
        public float decreasingWater;

        public CharacterRecoveryData Apply(BaseCharacterEntity characterEntity, EntityInfo causer)
        {
            int tempAmount;
            // Hp
            if (characterEntity.CurrentHp < characterEntity.MaxHp)
            {
                if (recoveryingHp >= 1)
                {
                    tempAmount = (int)recoveryingHp;
                    characterEntity.OnBuffHpRecovery(causer, tempAmount);
                    recoveryingHp -= tempAmount;
                }
            }
            else
                recoveryingHp = 0;

            // Decrease Hp
            if (characterEntity.CurrentHp > 0)
            {
                if (decreasingHp >= 1)
                {
                    tempAmount = (int)decreasingHp;
                    characterEntity.OnBuffHpDecrease(causer, tempAmount);
                    decreasingHp -= tempAmount;
                }
            }
            else
                decreasingHp = 0;

            // Mp
            if (characterEntity.CurrentMp < characterEntity.MaxMp)
            {
                if (recoveryingMp >= 1)
                {
                    tempAmount = (int)recoveryingMp;
                    characterEntity.OnBuffMpRecovery(causer, tempAmount);
                    recoveryingMp -= tempAmount;
                }
            }
            else
                recoveryingMp = 0;

            // Decrease Mp
            if (characterEntity.CurrentMp > 0)
            {
                if (decreasingMp >= 1)
                {
                    tempAmount = (int)decreasingMp;
                    characterEntity.OnBuffMpDecrease(causer, tempAmount);
                    decreasingMp -= tempAmount;
                }
            }
            else
                decreasingMp = 0;

            // Stamina
            if (characterEntity.CurrentStamina < characterEntity.MaxStamina)
            {
                if (recoveryingStamina >= 1)
                {
                    tempAmount = (int)recoveryingStamina;
                    characterEntity.OnBuffStaminaRecovery(causer, tempAmount);
                    recoveryingStamina -= tempAmount;
                }
            }
            else
                recoveryingStamina = 0;

            // Decrease Stamina
            if (characterEntity.CurrentStamina > 0)
            {
                if (decreasingStamina >= 1)
                {
                    tempAmount = (int)decreasingStamina;
                    characterEntity.OnBuffStaminaDecrease(causer, tempAmount);
                    decreasingStamina -= tempAmount;
                }
            }
            else
                decreasingStamina = 0;

            // Food
            if (characterEntity.CurrentFood < characterEntity.MaxFood)
            {
                if (recoveryingFood >= 1)
                {
                    tempAmount = (int)recoveryingFood;
                    characterEntity.OnBuffFoodRecovery(causer, tempAmount);
                    recoveryingFood -= tempAmount;
                }
            }
            else
                recoveryingFood = 0;

            // Decrease Food
            if (characterEntity.CurrentFood > 0)
            {
                if (decreasingFood >= 1)
                {
                    tempAmount = (int)decreasingFood;
                    characterEntity.OnBuffFoodDecrease(causer, tempAmount);
                    decreasingFood -= tempAmount;
                }
            }
            else
                decreasingFood = 0;

            // Water
            if (characterEntity.CurrentWater < characterEntity.MaxWater)
            {
                if (recoveryingWater >= 1)
                {
                    tempAmount = (int)recoveryingWater;
                    characterEntity.OnBuffWaterRecovery(causer, tempAmount);
                    recoveryingWater -= tempAmount;
                }
            }
            else
                recoveryingWater = 0;

            // Decrease Water
            if (characterEntity.CurrentWater > 0)
            {
                if (decreasingWater >= 1)
                {
                    tempAmount = (int)decreasingWater;
                    characterEntity.OnBuffWaterDecrease(causer, tempAmount);
                    decreasingWater -= tempAmount;
                }
            }
            else
                decreasingWater = 0;

            return this;
        }
    }
}
