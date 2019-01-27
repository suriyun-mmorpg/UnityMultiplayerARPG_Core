using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterHotkeyAssigner : UIBase
    {
        public string hotkeyId { get; protected set; }

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

        public void Setup(string hotkeyId)
        {
            this.hotkeyId = hotkeyId;
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
            List<CharacterSkill> filterSkills = new List<CharacterSkill>();
            List<int> filterSkillsIndexes = new List<int>();
            List<CharacterItem> filterItems = new List<CharacterItem>();
            List<int> filterItemsIndexes = new List<int>();
            IList<CharacterSkill> characterSkills = owningCharacter.Skills;
            IList<CharacterItem> characterItems = owningCharacter.NonEquipItems;
            int counter = 0;
            foreach (CharacterSkill characterSkill in characterSkills)
            {
                Skill skill = characterSkill.GetSkill();
                if (skill != null && characterSkill.level > 0 &&
                    (skill.skillType == SkillType.Active || skill.skillType == SkillType.CraftItem))
                {
                    filterSkills.Add(characterSkill);
                    filterSkillsIndexes.Add(counter);
                }
                ++counter;
            }
            counter = 0;
            foreach (CharacterItem characterItem in characterItems)
            {
                Item item = characterItem.GetItem();
                if (item != null && characterItem.level > 0 && characterItem.amount > 0 &&
                    (item.IsPotion() || item.IsBuilding() || item.IsPet()))
                {
                    filterItems.Add(characterItem);
                    filterItemsIndexes.Add(counter);
                }
                ++counter;
            }
            CacheSkillList.Generate(filterSkills, (index, characterSkill, ui) =>
            {
                UICharacterSkill uiCharacterSkill = ui.GetComponent<UICharacterSkill>();
                uiCharacterSkill.Setup(new SkillTuple(characterSkill.GetSkill(), characterSkill.level), null, filterSkillsIndexes[index]);
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
                owningCharacter.RequestAssignHotkey(hotkeyId, HotkeyType.Skill, ui.Skill.DataId);
            Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestAssignHotkey(hotkeyId, HotkeyType.Item, ui.Data.characterItem.dataId);
            Hide();
        }

        public void OnClickUnAssign()
        {
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.RequestAssignHotkey(hotkeyId, HotkeyType.None, 0);
            Hide();
        }
    }
}
