using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkey : UISelectionEntry<CharacterHotkey>
    {
        public int indexOfData { get; protected set; }
        public string hotkeyId { get { return Data.hotkeyId; } }
        public KeyCode key;
        public UICharacterHotkeys uiCharacterHotkeys;
        public UICharacterSkill uiCharacterSkill;
        public UICharacterItem uiCharacterItem;
        public UICharacterHotkeyAssigner uiAssigner;

        public void Setup(UICharacterHotkeys uiCharacterHotkeys, CharacterHotkey data, int indexOfData)
        {
            this.uiCharacterHotkeys = uiCharacterHotkeys;
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
            if (uiCharacterSkill == null && uiCharacterHotkeys != null && uiCharacterHotkeys.uiCharacterSkillPrefab != null)
            {
                uiCharacterSkill = Instantiate(uiCharacterHotkeys.uiCharacterSkillPrefab, transform);
                GenericUtils.SetAndStretchToParentSize(uiCharacterSkill.transform as RectTransform, transform as RectTransform);
                uiCharacterSkill.transform.SetAsFirstSibling();
            }

            if (uiCharacterItem == null && uiCharacterHotkeys != null && uiCharacterHotkeys.uiCharacterItemPrefab != null)
            {
                uiCharacterItem = Instantiate(uiCharacterHotkeys.uiCharacterItemPrefab, transform);
                GenericUtils.SetAndStretchToParentSize(uiCharacterItem.transform as RectTransform, transform as RectTransform);
                uiCharacterItem.transform.SetAsFirstSibling();
            }

            if (uiCharacterSkill != null)
            {
                if (skill == null)
                    uiCharacterSkill.Hide();
                else
                {
                    Dictionary<Skill, short> allSkills = owningCharacter.GetSkills();
                    short skillLevel = 0;
                    if (allSkills.TryGetValue(characterHotkey.GetSkill(), out skillLevel))
                    {
                        int index = owningCharacter.IndexOfSkill(characterHotkey.dataId);
                        CharacterSkill characterSkill = index >= 0 ? owningCharacter.Skills[index] : CharacterSkill.Create(characterHotkey.GetSkill(), skillLevel);
                        uiCharacterSkill.Setup(new CharacterSkillTuple(characterSkill, skillLevel), owningCharacter, index);
                        uiCharacterSkill.Show();
                        UICharacterSkillDragHandler dragHandler = uiCharacterSkill.GetComponentInChildren<UICharacterSkillDragHandler>();
                        if (dragHandler != null)
                            dragHandler.SetupForHotkey(this);
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
                        UICharacterItemDragHandler dragHandler = uiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                        if (dragHandler != null)
                            dragHandler.SetupForHotkey(this);
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
                uiAssigner.Setup(this);
                uiAssigner.Show();
            }
        }

        public void OnClickUse()
        {
            BasePlayerCharacterController owningCharacterController = BasePlayerCharacterController.Singleton;
            if (owningCharacterController != null)
                owningCharacterController.UseHotkey(indexOfData);
        }

        public bool CanAssignCharacterItem(CharacterItem characterItem)
        {
            if (characterItem == null)
                return false;
            Item item = characterItem.GetItem();
            if (item != null && characterItem.level > 0 && characterItem.amount > 0 &&
                (item.IsPotion() || item.IsBuilding() || item.IsPet() || item.IsMount()))
                return true;
            return false;
        }

        public bool CanAssignCharacterSkill(CharacterSkill characterSkill)
        {
            if (characterSkill == null)
                return false;
            Skill skill = characterSkill.GetSkill();
            if (skill != null && characterSkill.level > 0 &&
                (skill.skillType == SkillType.Active || skill.skillType == SkillType.CraftItem))
                return true;
            return false;
        }
    }
}
