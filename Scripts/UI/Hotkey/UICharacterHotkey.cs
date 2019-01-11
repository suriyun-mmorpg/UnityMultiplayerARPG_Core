using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkey : UISelectionEntry<CharacterHotkey>
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

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(key))
            {
                bool canUse = true;
                InputField[] fields = FindObjectsOfType<InputField>();
                foreach (InputField field in fields)
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
            CharacterHotkey characterHotkey = Data;
            Skill skill = characterHotkey.GetSkill();
            Item item = characterHotkey.GetItem();

            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (uiCharacterSkill != null)
            {
                if (skill == null)
                    uiCharacterSkill.Hide();
                else
                {
                    int index = owningCharacter.IndexOfSkill(characterHotkey.dataId);
                    if (index >= 0 && index < owningCharacter.Skills.Count)
                    {
                        CharacterSkill characterSkill = owningCharacter.Skills[index];
                        uiCharacterSkill.Setup(new SkillTuple(characterSkill.GetSkill(), characterSkill.level), owningCharacter, index);
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
                    int index = owningCharacter.IndexOfNonEquipItem(characterHotkey.dataId);
                    if (index >= 0 && index < owningCharacter.NonEquipItems.Count)
                    {
                        CharacterItem characterItem = owningCharacter.NonEquipItems[index];
                        uiCharacterItem.Setup(new CharacterItemTuple(characterItem, characterItem.level, InventoryType.NonEquipItems), owningCharacter, index);
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
            BasePlayerCharacterController owningCharacterController = BasePlayerCharacterController.Singleton;
            if (owningCharacterController != null)
                owningCharacterController.UseHotkey(indexOfData);
        }
    }
}
