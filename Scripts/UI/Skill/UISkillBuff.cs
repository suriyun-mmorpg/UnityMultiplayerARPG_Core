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
            textDuration.text = string.Format(durationFormat, skillBuff.GetDuration(skillLevel).ToString("N0"));

        if (textRecoveryHp != null)
            textRecoveryHp.text = string.Format(durationFormat, skillBuff.GetRecoveryHp(skillLevel).ToString("N0"));

        if (textRecoveryMp != null)
            textRecoveryMp.text = string.Format(durationFormat, skillBuff.GetRecoveryMp(skillLevel).ToString("N0"));

        if (uiBuffStats != null)
            uiBuffStats.Data = skillBuff.GetStats(skillLevel);

        if (uiBuffAttributes != null)
            uiBuffAttributes.Data = GameDataHelpers.MakeAttributeAmountsDictionary(skillBuff.increaseAttributes, new Dictionary<Attribute, int>(), skillLevel);

        if (uiBuffResistances != null)
            uiBuffResistances.Data = GameDataHelpers.MakeResistanceAmountsDictionary(skillBuff.increaseResistances, new Dictionary<Resistance, float>(), skillLevel);
    }
}
