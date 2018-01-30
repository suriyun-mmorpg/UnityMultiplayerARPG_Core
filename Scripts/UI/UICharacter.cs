using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [Header("Save data")]
    public CharacterEntity characterEntity;
    protected CharacterEntity dirtyCharacterEntity;

    protected virtual void Update()
    {
        if (dirtyCharacterEntity != null && characterEntity != dirtyCharacterEntity)
            Destroy(dirtyCharacterEntity.gameObject);
        dirtyCharacterEntity = characterEntity;

        if (textName != null)
            textName.text = characterEntity == null ? "" : characterEntity.characterName;

        if (textLevel != null)
            textLevel.text = characterEntity == null ? "N/A" : characterEntity.level.ToString("N0");

        if (textExp != null)
        {
            if (characterEntity == null)
                textExp.text = "N/A";
            else if (characterEntity.NextLevelExp > 0)
                textExp.text = characterEntity.exp.ToString("N0") + "/" + characterEntity.NextLevelExp.ToString("N0");
            else
                textExp.text = "Max";
        }

        if (textHp != null)
            textHp.text = characterEntity == null ? "N/A" : characterEntity.currentHp.ToString("N0") + "/" + characterEntity.MaxHp.ToString("N0");

        if (textMp != null)
            textMp.text = characterEntity == null ? "N/A" : characterEntity.currentMp.ToString("N0") + "/" + characterEntity.MaxMp.ToString("N0");

        if (textStatPoint != null)
            textStatPoint.text = characterEntity == null ? "N/A" : characterEntity.statPoint.ToString("N0");

        if (textSkillPoint != null)
            textSkillPoint.text = characterEntity == null ? "N/A" : characterEntity.skillPoint.ToString("N0");

        if (textGold != null)
            textGold.text = characterEntity == null ? "N/A" : characterEntity.gold.ToString("N0");

        if (textClassTitle != null)
            textClassTitle.text = characterEntity == null ? "N/A" : characterEntity.Class.title;

        if (textClassDescription != null)
            textClassDescription.text = characterEntity == null ? "N/A" : characterEntity.Class.description;

        if (imageClassIcon != null)
            imageClassIcon.sprite = characterEntity == null ? null : characterEntity.Class.icon;
    }

    protected virtual void OnDestroy()
    {
        if (dirtyCharacterEntity != null)
            Destroy(dirtyCharacterEntity.gameObject);
    }
}
