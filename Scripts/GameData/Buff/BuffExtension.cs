using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuffExtension
{
    #region Buff Extension
    public static float GetDuration(this Buff buff, int level)
    {
        return buff.duration.GetAmount(level);
    }

    public static int GetRecoveryHp(this Buff buff, int level)
    {
        return buff.recoveryHp.GetAmount(level);
    }

    public static int GetRecoveryMp(this Buff buff, int level)
    {
        return buff.recoveryMp.GetAmount(level);
    }

    public static CharacterStats GetIncreaseStats(this Buff buff, int level)
    {
        return buff.increaseStats.GetCharacterStats(level);
    }

    public static Dictionary<Attribute, int> GetIncreaseAttributes(this Buff buff, int level)
    {
        return GameDataHelpers.MakeAttributeAmountsDictionary(buff.increaseAttributes, new Dictionary<Attribute, int>(), level);
    }

    public static Dictionary<DamageElement, float> GetIncreaseResistances(this Buff buff, int level)
    {
        return GameDataHelpers.MakeResistanceAmountsDictionary(buff.increaseResistances, new Dictionary<DamageElement, float>(), level);
    }

    public static Dictionary<DamageElement, MinMaxFloat> GetIncreaseDamages(this Buff buff, int level)
    {
        return GameDataHelpers.MakeDamageAmountsDictionary(buff.increaseDamages, new Dictionary<DamageElement, MinMaxFloat>(), level);
    }
    #endregion
}
