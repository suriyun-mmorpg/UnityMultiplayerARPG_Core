using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

        public BasePlayerCharacterEntity OwningCharacter { get { return BasePlayerCharacterController.OwningCharacter; } }
        
        private BaseSkill hotkeySkill;
        private short hotkeySkillLevel;

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
            hotkeySkill = null;
            hotkeySkillLevel = 0;

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
                // All skills included equipment skills
                Dictionary<BaseSkill, short> skills = OwningCharacter.GetSkills();

                if (!GameInstance.Skills.TryGetValue(BaseGameData.MakeDataId(Data.relateId), out hotkeySkill) ||
                    hotkeySkill == null || !skills.TryGetValue(hotkeySkill, out hotkeySkillLevel))
                {
                    uiCharacterSkill.Hide();
                }
                else
                {
                    // Found skill, so create new skill entry if it's not existed in learn skill list
                    int skillIndex = OwningCharacter.IndexOfSkill(BaseGameData.MakeDataId(Data.relateId));
                    CharacterSkill characterSkill = skillIndex >= 0 ? OwningCharacter.Skills[skillIndex] : CharacterSkill.Create(hotkeySkill, hotkeySkillLevel);
                    uiCharacterSkill.Setup(new UICharacterSkillData(characterSkill, hotkeySkillLevel), OwningCharacter, skillIndex);
                    uiCharacterSkill.Show();
                    UICharacterSkillDragHandler dragHandler = uiCharacterSkill.GetComponentInChildren<UICharacterSkillDragHandler>();
                    if (dragHandler != null)
                        dragHandler.SetupForHotkey(this);
                }
            }

            if (uiCharacterItem != null)
            {
                // Prepare item data
                InventoryType inventoryType;
                int itemIndex;
                byte equipWeaponSet;
                CharacterItem characterItem;
                OwningCharacter.IsEquipped(
                    Data.relateId,
                    out inventoryType,
                    out itemIndex,
                    out equipWeaponSet,
                    out characterItem);

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
                    uiCharacterItem.Setup(new UICharacterItemData(characterItem, characterItem.level, InventoryType.NonEquipItems), OwningCharacter, itemIndex);
                    uiCharacterItem.Show();
                    // Setup skill item
                    if (characterItem.GetSkillItem() != null)
                    {
                        hotkeySkill = characterItem.GetSkillItem().skillLevel.skill;
                        hotkeySkillLevel = characterItem.GetSkillItem().skillLevel.level;
                    }
                    UICharacterItemDragHandler dragHandler = uiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                    if (dragHandler != null)
                        dragHandler.SetupForHotkey(this);
                }
            }
        }

        public Vector3? UpdateAimAxes(Vector3 axes)
        {
            if (hotkeySkill != null && hotkeySkillLevel > 0 &&
                hotkeySkill.GetSkillType() == SkillType.Active &&
                hotkeySkill.HasCustomAimControls())
            {
                return hotkeySkill.UpdateAimControls(axes, hotkeySkillLevel);
            }
            return null;
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
            OnClickUse(null);
        }

        public void OnClickUse(Vector3? aimPosition)
        {
            if (BasePlayerCharacterController.Singleton != null)
                BasePlayerCharacterController.Singleton.UseHotkey(indexOfData, aimPosition);
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
            BaseSkill skill = characterSkill.GetSkill();
            if (skill != null && characterSkill.level > 0 &&
                (skill.GetSkillType() == SkillType.Active || skill.GetSkillType() == SkillType.CraftItem))
                return true;
            return false;
        }
    }
}
