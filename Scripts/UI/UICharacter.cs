using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacter : UIBase
{

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
            textName.text = characterData == null ? "" : characterData.CharacterName;

        if (textLevel != null)
            textLevel.text = characterData == null ? "N/A" : characterData.Level.ToString("N0");

        if (textExp != null)
        {
            if (characterData == null)
                textExp.text = "N/A";
            else if (characterData.GetNextLevelExp() > 0)
                textExp.text = characterData.Exp.ToString("N0") + "/" + characterData.GetNextLevelExp().ToString("N0");
            else
                textExp.text = "Max";
        }

        if (textHp != null)
            textHp.text = characterData == null ? "N/A" : characterData.CurrentHp.ToString("N0") + "/" + characterData.GetMaxHp().ToString("N0");

        if (textMp != null)
            textMp.text = characterData == null ? "N/A" : characterData.CurrentMp.ToString("N0") + "/" + characterData.GetMaxMp().ToString("N0");

        if (textStatPoint != null)
            textStatPoint.text = characterData == null ? "N/A" : characterData.StatPoint.ToString("N0");

        if (textSkillPoint != null)
            textSkillPoint.text = characterData == null ? "N/A" : characterData.SkillPoint.ToString("N0");

        if (textGold != null)
            textGold.text = characterData == null ? "N/A" : characterData.Gold.ToString("N0");

        if (textClassTitle != null)
            textClassTitle.text = characterData == null ? "N/A" : characterData.GetClass().title;

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