using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkey : UISelectionEntry<CharacterHotkey>, IDropHandler
    {
        public int indexOfData { get; protected set; }
        public string hotkeyId { get { return Data.hotkeyId; } }
        public KeyCode key;
        public UICharacterSkill uiCharacterSkill;
        public UICharacterItem uiCharacterItem;
        public UICharacterHotkeyAssigner uiAssigner;

        private RectTransform dropRect;
        public RectTransform DropRect
        {
            get
            {
                if (dropRect == null)
                    dropRect = transform as RectTransform;
                return dropRect;
            }
        }

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
                        uiCharacterSkill.Setup(new CharacterSkillTuple(characterSkill, characterSkill.level), owningCharacter, index);
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

        public void OnDrop(PointerEventData eventData)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(DropRect, Input.mousePosition))
            {
                Debug.LogError("Drop");
                BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
                if (owningCharacter == null)
                    return;
                UIDragHandler dragHandler = eventData.pointerDrag.GetComponent<UIDragHandler>();
                if (dragHandler != null)
                {
                    UICharacterItemDragHandler draggedItemUI = dragHandler as UICharacterItemDragHandler;
                    if (draggedItemUI != null)
                    {
                        if (CanAssignCharacterItem(draggedItemUI.CacheUI.Data.characterItem))
                        {
                            // Assign item to hotkey
                            owningCharacter.RequestAssignHotkey(Data.hotkeyId, HotkeyType.Item, draggedItemUI.CacheUI.Data.characterItem.dataId);
                        }
                    }
                    UICharacterSkillDragHandler draggedSkillUI = dragHandler as UICharacterSkillDragHandler;
                    if (draggedSkillUI != null)
                    {
                        if (CanAssignCharacterSkill(draggedSkillUI.CacheUI.Data.characterSkill))
                        {
                            // Assign item to hotkey
                            owningCharacter.RequestAssignHotkey(Data.hotkeyId, HotkeyType.Skill, draggedSkillUI.CacheUI.Data.characterSkill.dataId);
                        }
                    }
                }
            }
        }

        public bool CanAssignCharacterItem(CharacterItem characterItem)
        {
            if (characterItem == null)
                return false;
            Item item = characterItem.GetItem();
            if (item != null && characterItem.level > 0 && characterItem.amount > 0 &&
                (item.IsPotion() || item.IsBuilding() || item.IsPet()))
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
