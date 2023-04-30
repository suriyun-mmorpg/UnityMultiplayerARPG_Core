using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkey : UISelectionEntry<CharacterHotkey>
    {
        public int indexOfData { get; protected set; }
        public string hotkeyId { get { return Data.hotkeyId; } }
        public UICharacterHotkeys UICharacterHotkeys { get; private set; }

        [FormerlySerializedAs("uiAssigner")]
        public UICharacterHotkeyAssigner uiCharacterHotkeyAssigner;
        public UICharacterSkill uiCharacterSkill;
        public UICharacterItem uiCharacterItem;
        public GameObject[] placeHolders;
        public KeyCode key;
        public string buttonName;

        [Header("Options")]
        public bool autoAssignItem;

        private IUsableItem _usingItem;
        private BaseSkill _usingSkill;
        private int _usingSkillLevel;
        private bool _channeledActionStarted;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation += OnNonEquipItemsOperation;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onNonEquipItemsOperation -= OnNonEquipItemsOperation;
        }

        private void OnNonEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            if (!autoAssignItem)
                return;
            if (!GetAssignedSkill(out _, out _) && !GetAssignedItem(out _, out _, out _))
            {
                foreach (CharacterItem nonEquipItem in GameInstance.PlayingCharacter.NonEquipItems)
                {
                    if (!CanAssignCharacterItem(nonEquipItem))
                        continue;
                    GameInstance.PlayingCharacterEntity.AssignItemHotkey(hotkeyId, nonEquipItem);
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

            if (placeHolders != null && placeHolders.Length > 0)
            {
                bool assigned = GetAssignedSkill(out _, out _) || GetAssignedItem(out _, out _, out _);
                foreach (GameObject placeHolder in placeHolders)
                {
                    placeHolder.SetActive(!assigned);
                }
            }

            if (GenericUtils.IsFocusInputField())
                return;

            if (IsChanneledAbility())
            {
                bool pressing = InputManager.GetKey(key) || InputManager.GetButton(buttonName);
                if (pressing && !_channeledActionStarted)
                {
                    _channeledActionStarted = true;
                    UICharacterHotkeys.SetUsingHotkey(this);
                }
                else if (!pressing && _channeledActionStarted)
                {
                    _channeledActionStarted = false;
                    UICharacterHotkeys.FinishHotkeyAimControls(false);
                }
            }
            else
            {
                bool pressing = InputManager.GetKeyDown(key) || InputManager.GetButtonDown(buttonName);
                if (pressing)
                    OnClickUse();
            }
        }

        public bool GetAssignedSkill(out BaseSkill skill, out int skillLevel)
        {
            skill = null;
            skillLevel = 0;
            if (Data.type == HotkeyType.Skill)
            {
                // Get all skills included equipment skills
                Dictionary<BaseSkill, int> skills = GameInstance.PlayingCharacter.GetCaches().Skills;
                int dataId = BaseGameData.MakeDataId(Data.relateId);
                return GameInstance.Skills.TryGetValue(dataId, out skill) &&
                    skill != null && skills.TryGetValue(skill, out skillLevel);
            }
            return false;
        }

        public bool GetAssignedItem(out InventoryType inventoryType, out int itemIndex, out CharacterItem characterItem)
        {
            inventoryType = InventoryType.NonEquipItems;
            itemIndex = -1;
            characterItem = null;
            if (Data.type == HotkeyType.Item)
            {
                int dataId = BaseGameData.MakeDataId(Data.relateId);
                if (GameInstance.Items.ContainsKey(dataId))
                {
                    // Find usable items
                    inventoryType = InventoryType.NonEquipItems;
                    itemIndex = GameInstance.PlayingCharacter.IndexOfNonEquipItem(dataId);
                    if (itemIndex >= 0)
                    {
                        characterItem = GameInstance.PlayingCharacter.NonEquipItems[itemIndex];
                        return true;
                    }
                }
                else
                {
                    return GameInstance.PlayingCharacter.FindItemById(
                        Data.relateId,
                        out inventoryType,
                        out itemIndex,
                        out _,
                        out characterItem);
                }
            }
            return false;
        }

        protected override void UpdateData()
        {
            UpdateSkillUI();
            UpdateItemUI();
        }

        private void UpdateSkillUI()
        {
            // Find skill by relate Id
            _usingSkill = null;
            _usingSkillLevel = 0;
            bool foundUsingSkill = GetAssignedSkill(out _usingSkill, out _usingSkillLevel);

            if (uiCharacterSkill == null && UICharacterHotkeys != null && UICharacterHotkeys.uiCharacterSkillPrefab != null)
            {
                uiCharacterSkill = Instantiate(UICharacterHotkeys.uiCharacterSkillPrefab, transform);
                GenericUtils.SetAndStretchToParentSize(uiCharacterSkill.transform as RectTransform, transform as RectTransform);
                uiCharacterSkill.transform.SetAsFirstSibling();
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
                    uiCharacterSkill.Setup(new UICharacterSkillData(_usingSkill, _usingSkillLevel), GameInstance.PlayingCharacter, GameInstance.PlayingCharacter.IndexOfSkill(_usingSkill.DataId));
                    uiCharacterSkill.Show();
                    UICharacterSkillDragHandler dragHandler = uiCharacterSkill.GetComponentInChildren<UICharacterSkillDragHandler>();
                    if (dragHandler != null)
                        dragHandler.SetupForHotkey(this);
                }
            }
        }

        private void UpdateItemUI()
        {
            // Find item by relate Id
            _usingItem = null;
            InventoryType inventoryType;
            int itemIndex;
            CharacterItem characterItem;
            bool foundUsingItem = GetAssignedItem(out inventoryType, out itemIndex, out characterItem);
            if (foundUsingItem)
                _usingItem = characterItem.GetUsableItem();

            if (uiCharacterItem == null && UICharacterHotkeys != null && UICharacterHotkeys.uiCharacterItemPrefab != null)
            {
                uiCharacterItem = Instantiate(UICharacterHotkeys.uiCharacterItemPrefab, transform);
                GenericUtils.SetAndStretchToParentSize(uiCharacterItem.transform as RectTransform, transform as RectTransform);
                uiCharacterItem.transform.SetAsFirstSibling();
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
                    uiCharacterItem.Setup(new UICharacterItemData(characterItem, inventoryType), GameInstance.PlayingCharacter, itemIndex);
                    uiCharacterItem.Show();
                    UICharacterItemDragHandler dragHandler = uiCharacterItem.GetComponentInChildren<UICharacterItemDragHandler>();
                    if (dragHandler != null)
                        dragHandler.SetupForHotkey(this);
                }
            }
        }

        public bool HasCustomAimControls()
        {
            if (_usingItem != null &&
                _usingItem.HasCustomAimControls())
            {
                return true;
            }
            else if (_usingSkill != null && _usingSkillLevel > 0 &&
                _usingSkill.IsActive &&
                _usingSkill.HasCustomAimControls())
            {
                return true;
            }
            return false;
        }

        public AimPosition UpdateAimControls(Vector2 axes)
        {
            if (_usingItem != null &&
                _usingItem.HasCustomAimControls())
            {
                return _usingItem.UpdateAimControls(axes);
            }
            if (_usingSkill != null && _usingSkillLevel > 0 &&
                _usingSkill.IsActive &&
                _usingSkill.HasCustomAimControls())
            {
                return _usingSkill.UpdateAimControls(axes, _usingSkillLevel);
            }
            return default;
        }

        public void FinishAimControls(bool isCancel, AimPosition aimPosition)
        {
            ICustomAimController ability = null;
            if (_usingItem != null)
                ability = _usingItem;
            if (_usingSkill != null)
                ability = _usingSkill;
            if (ability != null)
            {
                ability.FinishAimControls(isCancel);
                if (ability.IsChanneledAbility())
                {
                    StopChanneledAbility();
                    return;
                }
            }
            if (!isCancel)
                Use(aimPosition);
        }

        public bool IsChanneledAbility()
        {
            if (_usingItem != null &&
                _usingItem.IsChanneledAbility())
            {
                return true;
            }
            if (_usingSkill != null && _usingSkillLevel > 0 &&
                _usingSkill.IsActive &&
                _usingSkill.IsChanneledAbility())
            {
                return true;
            }
            return false;
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
            if (uiCharacterHotkeyAssigner != null && uiCharacterHotkeyAssigner.IsVisible())
                return;

            if (IsChanneledAbility())
                return;

            if (UICharacterHotkeys.UsingHotkey != null)
            {
                if (UICharacterHotkeys.UsingHotkey == this)
                {
                    UICharacterHotkeys.SetUsingHotkey(null);
                    return;
                }
                UICharacterHotkeys.SetUsingHotkey(null);
            }

            if (_usingSkill != null && GameInstance.PlayingCharacter.IndexOfSkillUsage(_usingSkill.DataId, SkillUsageType.Skill) >= 0)
            {
                // Skill this cooling down, can't use it
                return;
            }
            
            if (HasCustomAimControls())
            {
                UICharacterHotkeys.SetUsingHotkey(this);
            }
            else
            {
                Use(default);
            }
        }

        public void Use(AimPosition aimPosition)
        {
            if (IsChanneledAbility())
                return;
            if (BasePlayerCharacterController.Singleton != null && Data != null)
                BasePlayerCharacterController.Singleton.UseHotkey(Data.type, Data.relateId, aimPosition);
        }

        public void StartChanneledAbility()
        {

        }

        public void StopChanneledAbility()
        {

        }

        public bool CanAssignCharacterItem(CharacterItem characterItem)
        {
            if (UICharacterHotkeys.doNotIncludeItems)
                return false;
            if (characterItem.IsEmptySlot())
                return false;
            if (UICharacterHotkeys.filterCategories.Count > 0 &&
                !UICharacterHotkeys.filterCategories.Contains(characterItem.GetItem().Category))
                return false;
            if (UICharacterHotkeys.filterItemTypes.Count > 0 &&
                !UICharacterHotkeys.filterItemTypes.Contains(characterItem.GetItem().ItemType))
                return false;
            return true;
        }

        public bool CanAssignCharacterSkill(CharacterSkill characterSkill)
        {
            if (UICharacterHotkeys.doNotIncludeSkills)
                return false;
            if (characterSkill.IsEmpty())
                return false;
            if (!characterSkill.GetSkill().IsAvailable(GameInstance.PlayingCharacter))
                return false;
            if (UICharacterHotkeys.filterCategories.Count > 0 &&
                !UICharacterHotkeys.filterCategories.Contains(characterSkill.GetSkill().Category))
                return false;
            if (UICharacterHotkeys.filterSkillTypes.Count > 0 &&
                !UICharacterHotkeys.filterSkillTypes.Contains(characterSkill.GetSkill().SkillType))
                return false;
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
