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
        public bool autoHideIfNothingToAssign;

        private UIList cacheSkillList;
        public UIList CacheSkillList
        {
            get
            {
                if (cacheSkillList == null)
                {
                    cacheSkillList = gameObject.AddComponent<UIList>();
                    if (uiCharacterSkillPrefab != null)
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
                    if (uiCharacterItemPrefab != null)
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
                    cacheSkillSelectionManager = gameObject.GetOrAddComponent<UICharacterSkillSelectionManager>();
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
                    cacheItemSelectionManager = gameObject.GetOrAddComponent<UICharacterItemSelectionManager>();
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
            UICharacterHotkeys.SetUsingHotkey(null);
            if (GameInstance.PlayingCharacterEntity == null)
            {
                CacheSkillList.HideAll();
                CacheItemList.HideAll();
                return;
            }
            base.Show();
        }

        public override void OnShow()
        {
            CacheSkillSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSkill);
            CacheSkillSelectionManager.eventOnSelect.AddListener(OnSelectCharacterSkill);
            CacheItemSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterItem);
            CacheItemSelectionManager.eventOnSelect.AddListener(OnSelectCharacterItem);

            CacheSkillList.doNotRemoveContainerChildren = true;
            CacheItemList.doNotRemoveContainerChildren = true;

            int countAssignable = 0;

            // Setup skill list
            UICharacterSkill tempUiCharacterSkill;
            CharacterSkill tempCharacterSkill;
            BaseSkill tempSkill;
            int tempIndexOfSkill;
            CacheSkillList.Generate(GameInstance.PlayingCharacterEntity.GetCaches().Skills, (index, skillLevel, ui) =>
            {
                if (!ui)
                    return;
                tempUiCharacterSkill = ui.GetComponent<UICharacterSkill>();
                tempSkill = skillLevel.Key;
                tempIndexOfSkill = GameInstance.PlayingCharacterEntity.IndexOfSkill(tempSkill.DataId);
                // Set character skill data
                tempCharacterSkill = CharacterSkill.Create(tempSkill, skillLevel.Value);
                if (uiCharacterHotkey.CanAssignCharacterSkill(tempCharacterSkill))
                {
                    tempUiCharacterSkill.Setup(new UICharacterSkillData(tempCharacterSkill), GameInstance.PlayingCharacterEntity, tempIndexOfSkill);
                    tempUiCharacterSkill.Show();
                    CacheSkillSelectionManager.Add(tempUiCharacterSkill);
                    ++countAssignable;
                }
                else
                {
                    tempUiCharacterSkill.Hide();
                }
            });

            // Setup item list
            UICharacterItem tempUiCharacterItem;
            CacheItemList.Generate(GameInstance.PlayingCharacterEntity.NonEquipItems, (index, characterItem, ui) =>
            {
                if (!ui)
                    return;
                tempUiCharacterItem = ui.GetComponent<UICharacterItem>();
                if (uiCharacterHotkey.CanAssignCharacterItem(characterItem))
                {
                    tempUiCharacterItem.Setup(new UICharacterItemData(characterItem, InventoryType.NonEquipItems), GameInstance.PlayingCharacterEntity, index);
                    tempUiCharacterItem.Show();
                    CacheItemSelectionManager.Add(tempUiCharacterItem);
                    ++countAssignable;
                }
                else
                {
                    tempUiCharacterItem.Hide();
                }
            });

            if (autoHideIfNothingToAssign && countAssignable == 0)
                Hide();
        }

        public override void OnHide()
        {
            CacheSkillSelectionManager.DeselectSelectedUI();
            CacheItemSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCharacterSkill(UICharacterSkill ui)
        {
            GameInstance.PlayingCharacterEntity.AssignSkillHotkey(uiCharacterHotkey.hotkeyId, ui.CharacterSkill);
            Hide();
        }

        protected void OnSelectCharacterItem(UICharacterItem ui)
        {
            GameInstance.PlayingCharacterEntity.AssignItemHotkey(uiCharacterHotkey.hotkeyId, ui.CharacterItem);
            Hide();
        }

        public void OnClickUnAssign()
        {
            GameInstance.PlayingCharacterEntity.UnAssignHotkey(uiCharacterHotkey.hotkeyId);
            Hide();
        }
    }
}
