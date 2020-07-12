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
        public UICharacterHotkeys UICharacterHotkeys { get; private set; }

        [FormerlySerializedAs("uiAssigner")]
        public UICharacterHotkeyAssigner uiCharacterHotkeyAssigner;
        public UICharacterSkill uiCharacterSkill;
        public UICharacterItem uiCharacterItem;
        public KeyCode key;

        private IUsableItem usingItem;
        private BaseSkill usingSkill;
        private short usingSkillLevel;

        public void Setup(UICharacterHotkeys uiCharacterHotkeys, UICharacterHotkeyAssigner uiCharacterHotkeyAssigner, CharacterHotkey data, int indexOfData)
        {
            UICharacterHotkeys = uiCharacterHotkeys;
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
            usingItem = null;
            usingSkill = null;
            usingSkillLevel = 0;

            // Find skill by relate Id
            bool foundUsingSkill = false;
            if (Data.type == HotkeyType.Skill)
            {
                // Get all skills included equipment skills
                Dictionary<BaseSkill, short> skills = OwningCharacter.GetCaches().Skills;
                foundUsingSkill = GameInstance.Skills.TryGetValue(BaseGameData.MakeDataId(Data.relateId), out usingSkill) && usingSkill != null && skills.TryGetValue(usingSkill, out usingSkillLevel);
            }

            // Find item by relate Id
            bool foundUsingItem = false;
            int itemIndex = -1;
            CharacterItem characterItem = null;
            if (Data.type == HotkeyType.Item)
            {
                InventoryType inventoryType;
                byte equipWeaponSet;
                OwningCharacter.IsEquipped(
                    Data.relateId,
                    out inventoryType,
                    out itemIndex,
                    out equipWeaponSet,
                    out characterItem);

                switch (inventoryType)
                {
                    case InventoryType.EquipItems:
                    case InventoryType.NonEquipItems:
                        foundUsingItem = itemIndex >= 0;
                        break;
                    case InventoryType.EquipWeaponRight:
                    case InventoryType.EquipWeaponLeft:
                        foundUsingItem = true;
                        break;
                }

                if (foundUsingItem)
                    usingItem = characterItem.GetUsableItem();
            }

            if (uiCharacterSkill == null && UICharacterHotkeys != null && UICharacterHotkeys.uiCharacterSkillPrefab != null)
            {
                uiCharacterSkill = Instantiate(UICharacterHotkeys.uiCharacterSkillPrefab, transform);
                GenericUtils.SetAndStretchToParentSize(uiCharacterSkill.transform as RectTransform, transform as RectTransform);
                uiCharacterSkill.transform.SetAsFirstSibling();
            }

            if (uiCharacterItem == null && UICharacterHotkeys != null && UICharacterHotkeys.uiCharacterItemPrefab != null)
            {
                uiCharacterItem = Instantiate(UICharacterHotkeys.uiCharacterItemPrefab, transform);
                GenericUtils.SetAndStretchToParentSize(uiCharacterItem.transform as RectTransform, transform as RectTransform);
                uiCharacterItem.transform.SetAsFirstSibling();
            }

            if (uiCharacterSkill != null)
            {
                if (!foundUsingSkill)
                {
                    uiCharacterSkill.Hide();
                }
                else
                {
                    // Found skill, so create new skill entry if it's not existed in learn skill list
                    int skillIndex = OwningCharacter.IndexOfSkill(BaseGameData.MakeDataId(Data.relateId));
                    CharacterSkill characterSkill = skillIndex >= 0 ? OwningCharacter.Skills[skillIndex] : CharacterSkill.Create(usingSkill, usingSkillLevel);
                    uiCharacterSkill.Setup(new UICharacterSkillData(characterSkill, usingSkillLevel), OwningCharacter, skillIndex);
                    uiCharacterSkill.Show();
                    UICharacterSkillDragHandler dragHandler = uiCharacterSkill.GetComponentInChildren<UICharacterSkillDragHandler>();
                    if (dragHandler != null)
                        dragHandler.SetupForHotkey(this);
                }
            }

            if (uiCharacterItem != null)
            {
                // Prepare item data

                if (!foundUsingItem)
                {
                    uiCharacterItem.Hide();
                }
                else
                {
                    // Show only existed items
                    uiCharacterItem.Setup(new UICharacterItemData(characterItem, characterItem.level, InventoryType.NonEquipItems), OwningCharacter, itemIndex);
                    uiCharacterItem.Show();
                    UICharacterItemDragHandler dragHandler = uiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                    if (dragHandler != null)
                        dragHandler.SetupForHotkey(this);
                }
            }
        }

        public Vector3? UpdateAimControls(Vector2 axes)
        {
            if (usingItem != null &&
                usingItem.HasCustomAimControls())
            {
                return usingItem.UpdateAimControls(axes);
            }
            if (usingSkill != null && usingSkillLevel > 0 &&
                usingSkill.IsActive() &&
                usingSkill.HasCustomAimControls())
            {
                return usingSkill.UpdateAimControls(axes, usingSkillLevel);
            }
            return null;
        }

        public void FinishAimControls(bool isCancel)
        {
            if (usingItem != null)
                usingItem.FinishAimControls(isCancel);
            if (usingSkill != null)
                usingSkill.FinishAimControls(isCancel);
        }

        public void OnClickAssign()
        {
            if (uiCharacterHotkeyAssigner != null)
            {
                uiCharacterHotkeyAssigner.Setup(this);
                uiCharacterHotkeyAssigner.Show();
            }
        }

        /// <summary>
        /// NOTE: This event should be call by PC UIs only
        /// </summary>
        public void OnClickUse()
        {
            if (UICharacterHotkeys.UsingHotkey != null)
            {
                if (UICharacterHotkeys.UsingHotkey == this)
                {
                    UICharacterHotkeys.SetUsingHotkey(null);
                    return;
                }
                UICharacterHotkeys.SetUsingHotkey(null);
            }
            
            if (usingItem != null &&
                usingItem.HasCustomAimControls())
            {
                UICharacterHotkeys.SetUsingHotkey(this);
            }
            else if (usingSkill != null && usingSkillLevel > 0 &&
                usingSkill.IsActive() &&
                usingSkill.HasCustomAimControls())
            {
                UICharacterHotkeys.SetUsingHotkey(this);
            }
            else
            {
                Use(null);
            }
        }

        public void Use(Vector3? aimPosition)
        {
            if (BasePlayerCharacterController.Singleton != null && Data != null)
                BasePlayerCharacterController.Singleton.UseHotkey(Data.type, Data.relateId, aimPosition);
        }

        public bool CanAssignCharacterItem(CharacterItem characterItem)
        {
            if (characterItem.IsEmpty() || characterItem.IsEmptySlot())
                return false;
            if (UICharacterHotkeys.filterItemTypes.Contains(characterItem.GetItem().ItemType))
                return true;
            return false;
        }

        public bool CanAssignCharacterSkill(CharacterSkill characterSkill)
        {
            if (characterSkill.IsEmpty())
                return false;
            if (characterSkill.GetSkill().IsAvailable(OwningCharacter) &&
                UICharacterHotkeys.filterSkillTypes.Contains(characterSkill.GetSkill().SkillType))
                return true;
            return false;
        }

        public bool IsAssigned()
        {
            // Just check visibility because it will be hidden if skill or item can't be found
            return (uiCharacterSkill && uiCharacterSkill.IsVisible()) ||
                (uiCharacterItem && uiCharacterItem.IsVisible());
        }
    }
}
