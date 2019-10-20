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

            // Skills
            List<CharacterSkill> filterSkills = new List<CharacterSkill>();
            List<int> filterSkillsIndexes = new List<int>();
            // Items
            List<CharacterItem> filterItems = new List<CharacterItem>();
            List<int> filterItemsIndexes = new List<int>();
            
            CharacterSkill tempCharacterSkill;
            foreach (KeyValuePair<BaseSkill, short> characterSkill in owningCharacter.GetCaches().Skills)
            {
                tempCharacterSkill = CharacterSkill.Create(characterSkill.Key, characterSkill.Value);
                if (uiCharacterHotkey.CanAssignCharacterSkill(tempCharacterSkill))
                {
                    filterSkills.Add(tempCharacterSkill);
                    filterSkillsIndexes.Add(owningCharacter.IndexOfSkill(tempCharacterSkill.dataId));
                }
            }

            int counter = 0;
            foreach (CharacterItem characterItem in owningCharacter.NonEquipItems)
            {
                if (uiCharacterHotkey.CanAssignCharacterItem(characterItem))
                {
                    filterItems.Add(characterItem);
                    filterItemsIndexes.Add(counter);
                }
                ++counter;
            }

            CacheSkillList.doNotRemoveContainerChildren = true;
            CacheItemList.doNotRemoveContainerChildren = true;

            CacheSkillList.Generate(filterSkills, (index, characterSkill, ui) =>
            {
                UICharacterSkill uiCharacterSkill = ui.GetComponent<UICharacterSkill>();
                uiCharacterSkill.Setup(new UICharacterSkillData(characterSkill, characterSkill.level), BasePlayerCharacterController.OwningCharacter, filterSkillsIndexes[index]);
                uiCharacterSkill.Show();
                CacheSkillSelectionManager.Add(uiCharacterSkill);
            });

            CacheItemList.Generate(filterItems, (index, characterItem, ui) =>
            {
                UICharacterItem uiCharacterItem = ui.GetComponent<UICharacterItem>();
                uiCharacterItem.Setup(new UICharacterItemData(characterItem, characterItem.level, InventoryType.NonEquipItems), BasePlayerCharacterController.OwningCharacter, filterItemsIndexes[index]);
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
            BasePlayerCharacterController.OwningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.Skill, ui.Skill.Id);
            Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            BasePlayerCharacterController.OwningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.Item, ui.Data.characterItem.id);
            Hide();
        }

        public void OnClickUnAssign()
        {
            BasePlayerCharacterController.OwningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.None, string.Empty);
            Hide();
        }
    }
}
