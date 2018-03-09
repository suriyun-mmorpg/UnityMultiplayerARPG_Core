using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterBuff : UISelectionEntry<CharacterBuff>
{
    [Header("Generic Info Format")]
    [Tooltip("Title Format => {0} = {Title}")]
    public string titleFormat = "{0}";

    [Header("Generic Buff Format")]
    [Tooltip("Buff Duration Format => {0} = {Duration}")]
    public string buffDurationFormat = "Duration: {0}";
    [Tooltip("Buff Remains Duration Format => {0} = {Remains duration}")]
    public string buffRemainsDurationFormat = "{0}";
    [Tooltip("Recovery Hp Format => {0} = {Recovery amount}")]
    public string recoveryHpFormat = "Recovery Hp: {0}";
    [Tooltip("Recovery Mp Format => {0} = {Recovery amount}")]
    public string recoveryMpFormat = "Recovery Mp: {0}";

    [Header("UI Elements")]
    public Text textTitle;
    public Image imageIcon;
    public Text textBuffDuration;
    public Text textBuffRemainsDuration;
    public Image imageBuffDurationGage;
    public Text textRecoveryHp;
    public Text textRecoveryMp;
    public UICharacterStats uiCharacterStats;

    protected virtual void Update()
    {
        var skillData = data.GetSkill();

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, skillData == null ? "Unknow" : skillData.title);

        if (imageIcon != null)
        {
            imageIcon.sprite = skillData == null ? null : skillData.icon;
            imageIcon.gameObject.SetActive(skillData != null);
        }

        var buffRemainDuration = data.buffRemainsDuration;
        var buffDuration = data.GetBuffDuration();

        if (textBuffDuration != null)
            textBuffDuration.text = string.Format(buffDurationFormat, buffDuration.ToString("N0"));

        if (textBuffRemainsDuration != null)
            textBuffRemainsDuration.text = string.Format(buffRemainsDurationFormat, buffRemainDuration.ToString("N0"));

        if (imageBuffDurationGage != null)
            imageBuffDurationGage.fillAmount = buffDuration <= 0 ? 1 : buffRemainDuration / buffDuration;

        if (textRecoveryHp != null)
            textRecoveryHp.text = string.Format(recoveryHpFormat, data.GetBuffRecoveryHp().ToString("N0"));

        if (textRecoveryMp != null)
            textRecoveryMp.text = string.Format(recoveryMpFormat, data.GetBuffRecoveryMp().ToString("N0"));

        var stats = data.GetBuffStats();
        if (uiCharacterStats != null)
            uiCharacterStats.data = stats;
    }
}

[System.Serializable]
public class UICharacterBuffEvent : UnityEvent<UICharacterBuff> { }
