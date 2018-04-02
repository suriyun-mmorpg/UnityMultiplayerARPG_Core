using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISkillBuff : UISelectionEntry<KeyValuePair<SkillBuff, int>>
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

    protected override void UpdateData()
    {
        var skillBuff = Data.Key;
        var skillLevel = Data.Value;

        if (textDuration != null)
        {
            var duration = skillBuff.GetDuration(skillLevel);
            textDuration.text = string.Format(durationFormat, duration.ToString("N0"));
            textDuration.gameObject.SetActive(duration != 0);
        }

        if (textRecoveryHp != null)
        {
            var recoveryHp = skillBuff.GetRecoveryHp(skillLevel);
            textRecoveryHp.text = string.Format(recoveryHpFormat, recoveryHp.ToString("N0"));
            textRecoveryHp.gameObject.SetActive(recoveryHp != 0);
        }

        if (textRecoveryMp != null)
        {
            var recoveryMp = skillBuff.GetRecoveryMp(skillLevel);
            textRecoveryMp.text = string.Format(recoveryMpFormat, recoveryMp.ToString("N0"));
            textRecoveryMp.gameObject.SetActive(recoveryMp != 0);
        }

        if (uiBuffStats != null)
            uiBuffStats.Data = skillBuff.GetStats(skillLevel);

        if (uiBuffAttributes != null)
            uiBuffAttributes.Data = GameDataHelpers.MakeAttributeAmountsDictionary(skillBuff.increaseAttributes, new Dictionary<Attribute, int>(), skillLevel);

        if (uiBuffResistances != null)
            uiBuffResistances.Data = GameDataHelpers.MakeResistanceAmountsDictionary(skillBuff.increaseResistances, new Dictionary<Resistance, float>(), skillLevel);
    }
}
