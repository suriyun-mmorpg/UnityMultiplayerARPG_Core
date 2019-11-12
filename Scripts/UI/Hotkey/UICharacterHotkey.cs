using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkey : UISelectionEntry<CharacterHotkey>
    {
        public int indexOfData { get; protected set; }
        public string hotkeyId { get { return Data.hotkeyId; } }
        public BasePlayerCharacterEntity OwningCharacter { get { return BasePlayerCharacterController.OwningCharacter; } }
        public UICharacterHotkeys uiCharacterHotkeys { get; private set; }

        [FormerlySerializedAs("uiAssigner")]
        public UICharacterHotkeyAssigner uiCharacterHotkeyAssigner;
        public UICharacterSkill uiCharacterSkill;
        public UICharacterItem uiCharacterItem;
        public KeyCode key;
        
        private BaseSkill hotkeySkill;
        private short hotkeySkillLevel;

        public void Setup(UICharacterHotkeys uiCharacterHotkeys, UICharacterHotkeyAssigner uiCharacterHotkeyAssigner, CharacterHotkey data, int indexOfData)
        {
            this.uiCharacterHotkeys = uiCharacterHotkeys;
            if (this.uiCharacterHotkeyAssigner == null)
                this.uiCharacterHotkeyAssigner = uiCharacterHotkeyAssigner;
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
                Dictionary<BaseSkill, short> skills = OwningCharacter.GetCaches().Skills;

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

        public Vector3? UpdateAimControls(Vector2 axes)
        {
            if (hotkeySkill != null && hotkeySkillLevel > 0 &&
                hotkeySkill.GetSkillType() == SkillType.Active &&
                hotkeySkill.HasCustomAimControls())
            {
                return hotkeySkill.UpdateAimControls(axes, hotkeySkillLevel);
            }
            return null;
        }

        public void FinishAimControls()
        {
            if (hotkeySkill != null)
                hotkeySkill.FinishAimControls();
        }

        public void OnClickAssign()
        {
            if (uiCharacterHotkeyAssigner != null)
            {
                uiCharacterHotkeyAssigner.Setup(this);
                uiCharacterHotkeyAssigner.Show();
            }
        }

        public void OnClickUse()
        {
            if (UICharacterHotkeys.UsingHotkey != null)
            {
                if (UICharacterHotkeys.UsingHotkey == this)
                {
                    uiCharacterHotkeys.SetUsingHotkey(null);
                    return;
                }
                uiCharacterHotkeys.SetUsingHotkey(null);
            }

            if (hotkeySkill != null && hotkeySkillLevel > 0 &&
                hotkeySkill.GetSkillType() == SkillType.Active &&
                hotkeySkill.HasCustomAimControls())
            {
                uiCharacterHotkeys.SetUsingHotkey(this);
            }
            else
            {
                Use(null);
            }
        }

        public void Use(Vector3? aimPosition)
        {
            if (BasePlayerCharacterController.Singleton != null)
                BasePlayerCharacterController.Singleton.UseHotkey(indexOfData, aimPosition);
        }

        public bool CanAssignCharacterItem(CharacterItem characterItem)
        {
            if (characterItem.IsEmpty() || characterItem.IsEmptySlot())
                return false;
            if (uiCharacterHotkeys.filterItemTypes.Contains(characterItem.GetItem().itemType))
                return true;
            return false;
        }

        public bool CanAssignCharacterSkill(CharacterSkill characterSkill)
        {
            if (characterSkill.IsEmpty())
                return false;
            if (characterSkill.GetSkill().IsAvailable(OwningCharacter) &&
                uiCharacterHotkeys.filterSkillTypes.Contains(characterSkill.GetSkill().GetSkillType()))
                return true;
            return false;
        }
    }
}
