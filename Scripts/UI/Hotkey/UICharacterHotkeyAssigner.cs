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
            
            CacheSkillList.doNotRemoveContainerChildren = true;
            CacheItemList.doNotRemoveContainerChildren = true;

            // Setup skill list
            UICharacterSkill tempUiCharacterSkill;
            CharacterSkill tempCharacterSkill;
            BaseSkill tempSkill;
            int tempIndexOfSkill;
            CacheSkillList.Generate(owningCharacter.GetCaches().Skills, (index, skillLevel, ui) =>
            {
                tempUiCharacterSkill = ui.GetComponent<UICharacterSkill>();
                tempSkill = skillLevel.Key;
                tempIndexOfSkill = owningCharacter.IndexOfSkill(tempSkill.DataId);
                // Set character skill data
                tempCharacterSkill = CharacterSkill.Create(tempSkill, skillLevel.Value);
                if (uiCharacterHotkey.CanAssignCharacterSkill(tempCharacterSkill))
                {
                    tempUiCharacterSkill.Setup(new UICharacterSkillData(tempCharacterSkill, skillLevel.Value), owningCharacter, tempIndexOfSkill);
                    tempUiCharacterSkill.Show();
                    CacheSkillSelectionManager.Add(tempUiCharacterSkill);
                }
                else
                {
                    tempUiCharacterSkill.Hide();
                }
            });

            // Setup item list
            UICharacterItem tempUiCharacterItem;
            CacheItemList.Generate(owningCharacter.NonEquipItems, (index, characterItem, ui) =>
            {
                tempUiCharacterItem = ui.GetComponent<UICharacterItem>();
                if (uiCharacterHotkey.CanAssignCharacterItem(characterItem))
                {
                    tempUiCharacterItem.Setup(new UICharacterItemData(characterItem, characterItem.level, InventoryType.NonEquipItems), owningCharacter, index);
                    tempUiCharacterItem.Show();
                    CacheItemSelectionManager.Add(tempUiCharacterItem);
                }
                else
                {
                    tempUiCharacterItem.Hide();
                }
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
