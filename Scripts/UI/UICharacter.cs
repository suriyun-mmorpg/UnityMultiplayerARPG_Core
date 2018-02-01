using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacter : UIBase
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
    [Header("Events")]
    public UICharacterEvent eventOnSelect;
    [Header("Save data")]
    public ICharacterData characterData;

    protected virtual void Update()
    {
        if (textName != null)
            textName.text = string.Format(nameFormat, characterData == null ? "" : characterData.CharacterName);

        if (textLevel != null)
            textLevel.text = string.Format(levelFormat, characterData == null ? "N/A" : characterData.Level.ToString("N0"));

        if (textExp != null)
        {
            var expString = "";
            if (characterData == null)
                expString = "N/A";
            else if (characterData.GetNextLevelExp() > 0)
                expString = characterData.Exp.ToString("N0") + "/" + characterData.GetNextLevelExp().ToString("N0");
            else
                expString = "Max";
            textExp.text = string.Format(expFormat, expString);
        }

        if (textHp != null)
            textHp.text = string.Format(hpFormat, characterData == null ? "N/A" : characterData.CurrentHp.ToString("N0") + "/" + characterData.GetMaxHp().ToString("N0"));

        if (textMp != null)
            textMp.text = string.Format(mpFormat, characterData == null ? "N/A" : characterData.CurrentMp.ToString("N0") + "/" + characterData.GetMaxMp().ToString("N0"));

        if (textStatPoint != null)
            textStatPoint.text = string.Format(statPointFormat, characterData == null ? "N/A" : characterData.StatPoint.ToString("N0"));

        if (textSkillPoint != null)
            textSkillPoint.text = string.Format(skillPointFormat, characterData == null ? "N/A" : characterData.SkillPoint.ToString("N0"));

        if (textGold != null)
            textGold.text = string.Format(goldFormat, characterData == null ? "N/A" : characterData.Gold.ToString("N0"));

        if (textClassTitle != null)
            textClassTitle.text = string.Format(classTitleFormat, characterData == null ? "N/A" : characterData.GetClass().title);

        if (textClassDescription != null)
            textClassDescription.text = characterData == null ? "N/A" : characterData.GetClass().description;

        if (imageClassIcon != null)
            imageClassIcon.sprite = characterData == null ? null : characterData.GetClass().icon;
    }

    public void OnClickSelect()
    {
        if (eventOnSelect != null)
            eventOnSelect.Invoke(this);
    }
}

[System.Serializable]
public class UICharacterEvent : UnityEvent<UICharacter> { }