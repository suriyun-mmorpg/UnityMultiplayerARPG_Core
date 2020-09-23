using LiteNetLibManager;
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

        [Header("Options")]
        public bool autoAssignItem;

        private IUsableItem usingItem;
        private BaseSkill usingSkill;
        private short usingSkillLevel;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!BasePlayerCharacterController.OwningCharacter) return;
            BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!BasePlayerCharacterController.OwningCharacter) return;
            BasePlayerCharacterController.OwningCharacter.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (!autoAssignItem)
                return;
            BaseSkill skill;
            short skillLevel;
            int itemIndex;
            CharacterItem characterItem;
            if (!GetAssignedSkill(out skill, out skillLevel) && !GetAssignedItem(out itemIndex, out characterItem))
            {
                foreach (CharacterItem nonEquipItem in OwningCharacter.NonEquipItems)
                {
                    if (!CanAssignCharacterItem(nonEquipItem))
                        continue;
                    OwningCharacter.AssignItemHotkey(hotkeyId, nonEquipItem);
                    break;
                }
            }
        }

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

        public bool GetAssignedSkill(out BaseSkill skill, out short skillLevel)
        {
            skill = null;
            skillLevel = 0;
            if (Data.type == HotkeyType.Skill)
            {
                // Get all skills included equipment skills
                Dictionary<BaseSkill, short> skills = OwningCharacter.GetCaches().Skills;
                int dataId = BaseGameData.MakeDataId(Data.relateId);
                return GameInstance.Skills.TryGetValue(dataId, out skill) &&
                    skill != null && skills.TryGetValue(skill, out skillLevel);
            }
            return false;
        }

        public bool GetAssignedItem(out int itemIndex, out CharacterItem characterItem)
        {
            itemIndex = -1;
            characterItem = null;
            if (Data.type == HotkeyType.Item)
            {
                int dataId = BaseGameData.MakeDataId(Data.relateId);
                if (GameInstance.Items.ContainsKey(dataId))
                {
                    // Find usable items
                    itemIndex = OwningCharacter.IndexOfNonEquipItem(dataId);
                    if (itemIndex >= 0)
                    {
                        characterItem = OwningCharacter.NonEquipItems[itemIndex];
                        return true;
                    }
                }
                else
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
                            return itemIndex >= 0;
                        case InventoryType.EquipWeaponRight:
                        case InventoryType.EquipWeaponLeft:
                            return true;
                    }
                }
            }
            return false;
        }

        protected override void UpdateData()
        {
            usingItem = null;
            usingSkill = null;
            usingSkillLevel = 0;

            // Find skill by relate Id
            bool foundUsingSkill = GetAssignedSkill(out usingSkill, out usingSkillLevel);

            // Find item by relate Id
            int itemIndex;
            CharacterItem characterItem;
            bool foundUsingItem = GetAssignedItem(out itemIndex, out characterItem);
            if (foundUsingItem)
                usingItem = characterItem.GetUsableItem();

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
            
            if (HasCustomAimControls())
            {
                UICharacterHotkeys.SetUsingHotkey(this);
            }
            else
            {
                Use(null);
            }
        }

        public bool HasCustomAimControls()
        {
            if (usingItem != null &&
                usingItem.HasCustomAimControls())
            {
                return true;
            }
            else if (usingSkill != null && usingSkillLevel > 0 &&
                usingSkill.IsActive() &&
                usingSkill.HasCustomAimControls())
            {
                return true;
            }
            return false;
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
            if (UICharacterHotkeys.filterCategories.Count > 0 &&
                !UICharacterHotkeys.filterCategories.Contains(characterItem.GetItem().category))
                return false;
            if (UICharacterHotkeys.filterItemTypes.Count > 0 &&
                !UICharacterHotkeys.filterItemTypes.Contains(characterItem.GetItem().ItemType))
                return false;
            return true;
        }

        public bool CanAssignCharacterSkill(CharacterSkill characterSkill)
        {
            if (characterSkill.IsEmpty())
                return false;
            if (!characterSkill.GetSkill().IsAvailable(OwningCharacter))
                return false;
            if (UICharacterHotkeys.filterCategories.Count > 0 &&
                !UICharacterHotkeys.filterCategories.Contains(characterSkill.GetSkill().category))
                return false;
            if (UICharacterHotkeys.filterSkillTypes.Count > 0 &&
                !UICharacterHotkeys.filterSkillTypes.Contains(characterSkill.GetSkill().SkillType))
                return true;
            return true;
        }

        public bool IsAssigned()
        {
            // Just check visibility because it will be hidden if skill or item can't be found
            return (uiCharacterSkill && uiCharacterSkill.IsVisible()) ||
                (uiCharacterItem && uiCharacterItem.IsVisible());
        }
    }
}
