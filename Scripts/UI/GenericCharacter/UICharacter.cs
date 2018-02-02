using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacter : UISelectionEntry<ICharacterData>
{
    [Header("Display Format")]
    public string nameFormat = "{0}";
    public string levelFormat = "Lv: {0}";
    public string expFormat = "Exp: {0}";
    public string hpFormat = "Hp: {0}";
    public string mpFormat = "Mp: {0}";
    public string statPointFormat = "Stat Points: {0}";
    public string skillPointFormat = "Skill Points: {0}";
    public string goldFormat = "Gold: {0}";
    public string classTitleFormat = "Class: {0}";
    [Header("UI Elements")]
    public Text textName;
    public Text textLevel;
    public Text textExp;
    public Text textHp;
    public Text textMp;
    public Text textStatPoint;
    public Text textSkillPoint;
    public Text textGold;
    [Header("Class information")]
    public Text textClassTitle;
    public Text textClassDescription;
    public Image imageClassIcon;

    protected virtual void Update()
    {
        if (textName != null)
            textName.text = string.Format(nameFormat, data == null ? "" : data.CharacterName);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, data == null ? "N/A" : data.Level.ToString("N0"));

        if (textExp != null)
        {
            var expString = "";
            if (data == null)
                expString = "N/A";
            else if (data.GetNextLevelExp() > 0)
                expString = data.Exp.ToString("N0") + "/" + data.GetNextLevelExp().ToString("N0");
            else
                expString = "Max";
            textExp.text = string.Format(expFormat, expString);
        }

        if (textHp != null)
            textHp.text = string.Format(hpFormat, data == null ? "N/A" : data.CurrentHp.ToString("N0") + "/" + data.GetMaxHp().ToString("N0"));

        if (textMp != null)
            textMp.text = string.Format(mpFormat, data == null ? "N/A" : data.CurrentMp.ToString("N0") + "/" + data.GetMaxMp().ToString("N0"));

        if (textStatPoint != null)
            textStatPoint.text = string.Format(statPointFormat, data == null ? "N/A" : data.StatPoint.ToString("N0"));

        if (textSkillPoint != null)
            textSkillPoint.text = string.Format(skillPointFormat, data == null ? "N/A" : data.SkillPoint.ToString("N0"));

        if (textGold != null)
            textGold.text = string.Format(goldFormat, data == null ? "N/A" : data.Gold.ToString("N0"));

        if (textClassTitle != null)
            textClassTitle.text = string.Format(classTitleFormat, data == null ? "N/A" : data.GetClass().title);

        if (textClassDescription != null)
            textClassDescription.text = data == null ? "N/A" : data.GetClass().description;

        if (imageClassIcon != null)
            imageClassIcon.sprite = data == null ? null : data.GetClass().icon;
    }
}

[System.Serializable]
public class UICharacterEvent : UnityEvent<UICharacter> { }