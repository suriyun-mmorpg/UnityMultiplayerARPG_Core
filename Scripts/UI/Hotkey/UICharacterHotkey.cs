using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UICharacterHotkey : UISelectionEntry<CharacterHotkey>
{
    public int indexOfData { get; protected set; }
    public KeyCode key;
    public UICharacterSkill uiCharacterSkill;
    public UICharacterItem uiCharacterItem;
    public UICharacterHotkeyAssigner uiAssigner;
    
    public void Setup(CharacterHotkey data, int indexOfData)
    {
        this.indexOfData = indexOfData;
        Data = data;
    }

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            bool canUse = true;
            var fields = FindObjectsOfType<InputField>();
            foreach (var field in fields)
            {
                if (field.isFocused)
                {
                    canUse = false;
                    break;
                }
            }
            if (canUse)
                OnClickUse();
        }
    }

    protected override void UpdateData()
    {
        var characterHotkey = Data;
        var skill = characterHotkey.GetSkill();
        var item = characterHotkey.GetItem();

        var owningCharacter = BasePlayerCharacterController.OwningCharacter;
        if (uiCharacterSkill != null)
        {
            if (skill == null)
                uiCharacterSkill.Hide();
            else
            {
                var index = owningCharacter.skills.IndexOf(characterHotkey.dataId);
                if (index >= 0 && index < owningCharacter.skills.Count)
                {
                    var characterSkill = owningCharacter.skills[index];
                    uiCharacterSkill.Setup(new KeyValuePair<CharacterSkill, int>(characterSkill, characterSkill.level), owningCharacter, index);
                    uiCharacterSkill.Show();
                }
                else
                    uiCharacterSkill.Hide();
            }
        }

        if (uiCharacterItem != null)
        {
            if (item == null)
                uiCharacterItem.Hide();
            else
            {
                var index = owningCharacter.nonEquipItems.IndexOf(characterHotkey.dataId);
                if (index >= 0 && index < owningCharacter.nonEquipItems.Count)
                {
                    var characterItem = owningCharacter.nonEquipItems[index];
                    uiCharacterItem.Setup(new KeyValuePair<CharacterItem, int>(characterItem, characterItem.level), owningCharacter, index, string.Empty);
                    uiCharacterItem.Show();
                }
                else
                    uiCharacterItem.Hide();
            }
        }
    }

    public void OnClickAssign()
    {
        if (uiAssigner != null)
        {
            uiAssigner.Setup(Data.hotkeyId);
            uiAssigner.Show();
        }
    }

    public void OnClickUse()
    {
        var owningCharacterController = BasePlayerCharacterController.OwningCharacterController;
        if (owningCharacterController != null)
            owningCharacterController.UseHotkey(indexOfData);
    }
}

[System.Serializable]
public class UICharacterHotkeyEvent : UnityEvent<UICharacterHotkey> { }
