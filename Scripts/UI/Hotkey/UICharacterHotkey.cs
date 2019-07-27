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
                // Prepare skill data
                Skill skill = null;
                short skillLevel = 1;
                // All skills included equipment skills
                Dictionary<Skill, short> skills = owningCharacter.GetSkills();

                if (!GameInstance.Skills.TryGetValue(BaseGameData.MakeDataId(Data.relateId), out skill) ||
                    skill == null || !skills.TryGetValue(skill, out skillLevel))
                {
                    uiCharacterSkill.Hide();
                }
                else
                {
                    // Found skill, so create new skill entry if it's not existed in learn skill list
                    int skillIndex = owningCharacter.IndexOfSkill(BaseGameData.MakeDataId(Data.relateId));
                    CharacterSkill characterSkill = skillIndex >= 0 ? owningCharacter.Skills[skillIndex] : CharacterSkill.Create(skill, skillLevel);
                    uiCharacterSkill.Setup(new CharacterSkillTuple(characterSkill, skillLevel), owningCharacter, skillIndex);
                    uiCharacterSkill.Show();
                    UICharacterSkillDragHandler dragHandler = uiCharacterSkill.GetComponentInChildren<UICharacterSkillDragHandler>();
                    if (dragHandler != null)
                        dragHandler.SetupForHotkey(this);
                }
            }

            if (uiCharacterItem != null)
            {
                // Prepare item data
                int itemIndex = -1;
                CharacterItem characterItem;
                InventoryType inventoryType;
                owningCharacter.IsEquipped(Data.relateId, out itemIndex, out characterItem, out inventoryType);

                bool isFound = false;
                switch (inventoryType)
                {
                    case InventoryType.EquipItems:
                    case InventoryType.NonEquipItems:
                        isFound = itemIndex >= 0;
                        break;
                    case InventoryType.EquipWeaponRight:
                    case InventoryType.EquipWeaponLeft:
                        isFound = true;
                        break;
                }

                if (!isFound)
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    // Show only existed items
                    uiCharacterItem.Setup(new CharacterItemTuple(characterItem, characterItem.level, InventoryType.NonEquipItems), owningCharacter, itemIndex);
                    uiCharacterItem.Show();
                    UICharacterItemDragHandler dragHandler = uiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                    if (dragHandler != null)
                        dragHandler.SetupForHotkey(this);
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
                (item.IsEquipment() || item.IsUsable() || item.IsBuilding()))
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
