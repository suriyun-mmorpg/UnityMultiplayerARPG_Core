using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterBuff : UIDataForCharacter<CharacterBuff>
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
    public UIBuff uiBuff;

    private void Update()
    {
        var characterBuff = Data;

        // Update duration
        if (character != null && indexOfData >= 0 && indexOfData < character.Buffs.Count)
            characterBuff = character.Buffs[indexOfData];

        var buffRemainDuration = characterBuff.buffRemainsDuration;
        var buffDuration = characterBuff.GetDuration();

        if (textDuration != null)
            textDuration.text = string.Format(buffDurationFormat, buffDuration.ToString("N0"));

        if (textRemainsDuration != null)
            textRemainsDuration.text = string.Format(buffRemainsDurationFormat, buffRemainDuration.ToString("N0"));

        if (imageDurationGage != null)
            imageDurationGage.fillAmount = buffDuration <= 0 ? 0 : buffRemainDuration / buffDuration;
    }

    protected override void UpdateData()
    {
        var skill = Data.GetSkill();
        var item = Data.GetItem();

        if (Data.type == BuffTypes.SkillBuff || Data.type == BuffTypes.SkillDebuff)
        {
            if (textTitle != null)
                textTitle.text = string.Format(titleFormat, skill == null ? "Unknow" : skill.title);

            if (imageIcon != null)
            {
                imageIcon.sprite = skill == null ? null : skill.icon;
                imageIcon.gameObject.SetActive(skill != null);
            }
        }

        if (Data.type == BuffTypes.PotionBuff)
        {
            if (textTitle != null)
                textTitle.text = string.Format(titleFormat, item == null ? "Unknow" : item.title);

            if (imageIcon != null)
            {
                imageIcon.sprite = item == null ? null : item.icon;
                imageIcon.gameObject.SetActive(item != null);
            }
        }

        if (uiBuff != null)
        {
            if (skill == null && item == null)
                uiBuff.Hide();
            else
            {
                var buff = Data.GetBuff();
                uiBuff.Show();
                uiBuff.Data = new KeyValuePair<Buff, int>(buff, Data.level);
            }
        }
    }
}

[System.Serializable]
public class UICharacterBuffEvent : UnityEvent<UICharacterBuff> { }
