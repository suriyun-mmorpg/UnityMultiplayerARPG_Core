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

    [Header("UI Elements")]
    public Text textTitle;
    public Image imageIcon;
    public Text textDuration;
    public Text textRemainsDuration;
    public Image imageDurationGage;
    public UISkillBuff uiSkillBuff;

    private void Update()
    {
        var buffRemainDuration = Data.buffRemainsDuration;
        var buffDuration = Data.GetDuration();

        if (textDuration != null)
            textDuration.text = string.Format(buffDurationFormat, buffDuration.ToString("N0"));

        if (textRemainsDuration != null)
            textRemainsDuration.text = string.Format(buffRemainsDurationFormat, buffRemainDuration.ToString("N0"));

        if (imageDurationGage != null)
            imageDurationGage.fillAmount = buffDuration <= 0 ? 1 : buffRemainDuration / buffDuration;
    }

    protected override void UpdateData()
    {
        var skillData = Data.GetSkill();

        if (textTitle != null)
            textTitle.text = string.Format(titleFormat, skillData == null ? "Unknow" : skillData.title);

        if (imageIcon != null)
        {
            imageIcon.sprite = skillData == null ? null : skillData.icon;
            imageIcon.gameObject.SetActive(skillData != null);
        }

        if (uiSkillBuff != null)
        {
            if (skillData == null)
                uiSkillBuff.Hide();
            else
            {
                var skillBuff = !Data.isDebuff ? skillData.buff : skillData.debuff;
                uiSkillBuff.Data = new KeyValuePair<SkillBuff, int>(skillBuff, Data.level);
                uiSkillBuff.Show();
            }
        }
    }
}

[System.Serializable]
public class UICharacterBuffEvent : UnityEvent<UICharacterBuff> { }
