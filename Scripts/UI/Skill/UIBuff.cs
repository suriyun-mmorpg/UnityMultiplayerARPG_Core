using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBuff : UISelectionEntry<KeyValuePair<Buff, int>>
{
    [Tooltip("Duration Format => {0} = {Duration}")]
    public string durationFormat = "Duration: {0}";
    [Tooltip("Recovery Hp Format => {0} = {Recovery amount}")]
    public string recoveryHpFormat = "Recovery Hp: {0}";
    [Tooltip("Recovery Mp Format => {0} = {Recovery amount}")]
    public string recoveryMpFormat = "Recovery Mp: {0}";

    [Header("UI Elements")]
    public Text textDuration;
    public Text textRecoveryHp;
    public Text textRecoveryMp;
    public UICharacterStats uiBuffStats;
    public UIAttributeAmounts uiBuffAttributes;
    public UIResistanceAmounts uiBuffResistances;
    public UIDamageElementAmounts uiBuffDamages;

    protected override void UpdateData()
    {
        var buff = Data.Key;
        var level = Data.Value;

        if (textDuration != null)
        {
            var duration = buff.GetDuration(level);
            textDuration.text = string.Format(durationFormat, duration.ToString("N0"));
            textDuration.gameObject.SetActive(duration != 0);
        }

        if (textRecoveryHp != null)
        {
            var recoveryHp = buff.GetRecoveryHp(level);
            textRecoveryHp.text = string.Format(recoveryHpFormat, recoveryHp.ToString("N0"));
            textRecoveryHp.gameObject.SetActive(recoveryHp != 0);
        }

        if (textRecoveryMp != null)
        {
            var recoveryMp = buff.GetRecoveryMp(level);
            textRecoveryMp.text = string.Format(recoveryMpFormat, recoveryMp.ToString("N0"));
            textRecoveryMp.gameObject.SetActive(recoveryMp != 0);
        }

        if (uiBuffStats != null)
            uiBuffStats.Data = buff.GetIncreaseStats(level);

        if (uiBuffAttributes != null)
            uiBuffAttributes.Data = GameDataHelpers.MakeAttributeAmountsDictionary(buff.increaseAttributes, new Dictionary<Attribute, int>(), level);

        if (uiBuffResistances != null)
            uiBuffResistances.Data = GameDataHelpers.MakeResistanceAmountsDictionary(buff.increaseResistances, new Dictionary<DamageElement, float>(), level);

        if (uiBuffDamages != null)
            uiBuffDamages.Data = GameDataHelpers.MakeDamageAmountsDictionary(buff.increaseDamages, new Dictionary<DamageElement, MinMaxFloat>(), level);
    }
}
