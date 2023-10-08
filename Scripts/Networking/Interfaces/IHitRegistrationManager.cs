using System.Collections.Generic;

namespace MultiplayerARPG
{
    public partial interface IHitRegistrationManager
    {
        /// <summary>
        /// This will be called at server to store hit reg validation data
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="randomSeed"></param>
        /// <param name="triggerDurations"></param>
        /// <param name="fireSpread"></param>
        /// <param name="damageInfo"></param>
        /// <param name="damageAmounts"></param>
        /// <param name="weapon"></param>
        /// <param name="skill"></param>
        /// <param name="skillLevel"></param>
        void PrepareHitRegValidation(BaseGameEntity attacker, int randomSeed, float[] triggerDurations, byte fireSpread, DamageInfo damageInfo, Dictionary<DamageElement, MinMaxFloat> damageAmounts, CharacterItem weapon, BaseSkill skill, int skillLevel);
        /// <summary>
        /// This will be called at server to confirm hit reg validation data
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="randomSeed"></param>
        /// <param name="increaseDamageAmounts"></param>
        /// <param name="resultDamageAmounts"></param>
        /// <returns></returns>
        void ConfirmHitRegValidation(BaseGameEntity attacker, int randomSeed, byte triggerIndex, Dictionary<DamageElement, MinMaxFloat> increaseDamageAmounts);
        /// <summary>
        /// This will be called at client to store hit reg destination
        /// </summary>
        /// <param name="hitRegisterData"></param>
        void PrepareHitRegData(HitRegisterData hitRegisterData);
        /// <summary>
        /// This will be called at server when recieve hit reg message from client
        /// </summary>
        /// <param name="attacker"></param>
        /// <param name="message"></param>
        void Register(BaseGameEntity attacker, HitRegisterMessage message);
        /// <summary>
        /// This will be called at client to count hit reg data list
        /// </summary>
        /// <returns></returns>
        int CountHitRegDataList();
        /// <summary>
        /// This will be called at client to retrieve hit reg data list and use it to send hit reg message
        /// </summary>
        /// <returns></returns>
        List<HitRegisterData> GetHitRegDataList();
        /// <summary>
        /// This will be called at client after hit reg message sent to server to clear hit reg data
        /// </summary>
        void ClearHitRegData();
        /// <summary>
        /// Clear all data
        /// </summary>
        void ClearData();
    }
}
