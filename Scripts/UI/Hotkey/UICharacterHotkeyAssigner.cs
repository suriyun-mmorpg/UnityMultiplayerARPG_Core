using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkeyAssigner : UIBase
    {
        public UICharacterHotkey uiCharacterHotkey;
        public UICharacterSkill uiCharacterSkillPrefab;
        public UICharacterItem uiCharacterItemPrefab;
        public Transform uiCharacterSkillContainer;
        public Transform uiCharacterItemContainer;

        private UIList cacheSkillList;
        public UIList CacheSkillList
        {
            get
            {
                if (cacheSkillList == null)
                {
                    cacheSkillList = gameObject.AddComponent<UIList>();
                    cacheSkillList.uiPrefab = uiCharacterSkillPrefab.gameObject;
                    cacheSkillList.uiContainer = uiCharacterSkillContainer;
                }
                return cacheSkillList;
            }
        }

        private UIList cacheItemList;
        public UIList CacheItemList
        {
            get
            {
                if (cacheItemList == null)
                {
                    cacheItemList = gameObject.AddComponent<UIList>();
                    cacheItemList.uiPrefab = uiCharacterItemPrefab.gameObject;
                    cacheItemList.uiContainer = uiCharacterItemContainer;
                }
                return cacheItemList;
            }
        }

        private UICharacterSkillSelectionManager cacheSkillSelectionManager;
        public UICharacterSkillSelectionManager CacheSkillSelectionManager
        {
            get
            {
                if (cacheSkillSelectionManager == null)
                    cacheSkillSelectionManager = GetComponent<UICharacterSkillSelectionManager>();
                if (cacheSkillSelectionManager == null)
                    cacheSkillSelectionManager = gameObject.AddComponent<UICharacterSkillSelectionManager>();
                cacheSkillSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheSkillSelectionManager;
            }
        }

        private UICharacterItemSelectionManager cacheItemSelectionManager;
        public UICharacterItemSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = GetComponent<UICharacterItemSelectionManager>();
                if (cacheItemSelectionManager == null)
                    cacheItemSelectionManager = gameObject.AddComponent<UICharacterItemSelectionManager>();
                cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheItemSelectionManager;
            }
        }

        public void Setup(UICharacterHotkey uiCharacterHotkey)
        {
            this.uiCharacterHotkey = uiCharacterHotkey;
        }

        public override void Show()
        {
            CacheSkillSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSkill);
            CacheSkillSelectionManager.eventOnSelect.AddListener(OnSelectCharacterSkill);
            CacheItemSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
            {
                CacheSkillList.HideAll();
                CacheItemList.HideAll();
                return;
            }
            // Equipment Increase Skill Levels
            Dictionary<Skill, short> increaseSkillLevels = new Dictionary<Skill, short>();
            // Skills
            List<CharacterSkill> filterSkills = new List<CharacterSkill>();
            List<int> filterSkillsIndexes = new List<int>();
            // Items
            List<CharacterItem> filterItems = new List<CharacterItem>();
            List<int> filterItemsIndexes = new List<int>();

            // Increase skill from equipments
            foreach (CharacterItem characterItem in owningCharacter.EquipItems)
            {
                if (characterItem.GetEquipmentItem() == null)
                    continue;
                increaseSkillLevels = GameDataHelpers.CombineSkills(characterItem.GetEquipmentItem().increaseSkillLevels, increaseSkillLevels);
            }

            // Increase skill from right hand equipment
            if (owningCharacter.EquipWeapons.rightHand != null &&
                owningCharacter.EquipWeapons.rightHand.GetEquipmentItem() != null)
                increaseSkillLevels = GameDataHelpers.CombineSkills(owningCharacter.EquipWeapons.rightHand.GetEquipmentItem().increaseSkillLevels, increaseSkillLevels);

            // Increase skill from left hand equipment
            if (owningCharacter.EquipWeapons.leftHand != null &&
                owningCharacter.EquipWeapons.leftHand.GetEquipmentItem() != null)
                increaseSkillLevels = GameDataHelpers.CombineSkills(owningCharacter.EquipWeapons.leftHand.GetEquipmentItem().increaseSkillLevels, increaseSkillLevels);

            int counter = 0;
            List<CharacterSkill> characterSkills = new List<CharacterSkill>();
            CharacterSkill tempCharacterSkill;
            short tempIncreaseSkillLevel;
            foreach (CharacterSkill characterSkill in owningCharacter.Skills)
            {
                if (uiCharacterHotkey.CanAssignCharacterSkill(characterSkill))
                {
                    tempCharacterSkill = new CharacterSkill()
                    {
                        dataId = characterSkill.dataId,
                        level = characterSkill.level
                    };
                    if (increaseSkillLevels.TryGetValue(tempCharacterSkill.GetSkill(), out tempIncreaseSkillLevel))
                    {
                        tempCharacterSkill.level += tempIncreaseSkillLevel;
                        increaseSkillLevels.Remove(tempCharacterSkill.GetSkill());
                    }
                    filterSkills.Add(tempCharacterSkill);
                    filterSkillsIndexes.Add(counter);
                }
                ++counter;
            }
            // Remaining increase skill levels from equipment items
            foreach (KeyValuePair<Skill, short> characterSkill in increaseSkillLevels)
            {
                tempCharacterSkill = new CharacterSkill()
                {
                    dataId = characterSkill.Key.DataId,
                    level = characterSkill.Value
                };
                filterSkills.Add(tempCharacterSkill);
                // Set skill indexes to -1 because this skill didn't level up
                filterSkillsIndexes.Add(-1);
            }

            counter = 0;
            List<CharacterItem> characterItems = new List<CharacterItem>();
            foreach (CharacterItem characterItem in owningCharacter.NonEquipItems)
            {
                if (uiCharacterHotkey.CanAssignCharacterItem(characterItem))
                {
                    filterItems.Add(characterItem);
                    filterItemsIndexes.Add(counter);
                }
                ++counter;
            }

            CacheSkillList.Generate(filterSkills, (index, characterSkill, ui) =>
            {
                UICharacterSkill uiCharacterSkill = ui.GetComponent<UICharacterSkill>();
                uiCharacterSkill.Setup(new CharacterSkillTuple(characterSkill, characterSkill.level), null, filterSkillsIndexes[index]);
                uiCharacterSkill.Show();
                CacheSkillSelectionManager.Add(uiCharacterSkill);
            });

            CacheItemList.Generate(filterItems, (index, characterItem, ui) =>
            {
                UICharacterItem uiCharacterItem = ui.GetComponent<UICharacterItem>();
                uiCharacterItem.Setup(new CharacterItemTuple(characterItem, characterItem.level, InventoryType.NonEquipItems), null, filterItemsIndexes[index]);
                uiCharacterItem.Show();
                CacheItemSelectionManager.Add(uiCharacterItem);
            });
            base.Show();
        }

        public override void Hide()
        {
            CacheSkillSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterSkill(UICharacterSkill ui)
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.Skill, ui.Skill.DataId);
            Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.Item, ui.Data.characterItem.dataId);
            Hide();
        }

        public void OnClickUnAssign()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.None, 0);
            Hide();
        }
    }
}
