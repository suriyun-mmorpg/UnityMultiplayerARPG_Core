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

        private UIList _cacheSkillList;
        public UIList CacheSkillList
        {
            get
            {
                if (_cacheSkillList == null)
                {
                    _cacheSkillList = gameObject.AddComponent<UIList>();
                    if (uiCharacterSkillPrefab != null)
                        _cacheSkillList.uiPrefab = uiCharacterSkillPrefab.gameObject;
                    _cacheSkillList.uiContainer = uiCharacterSkillContainer;
                }
                return _cacheSkillList;
            }
        }

        private UIList _cacheItemList;
        public UIList CacheItemList
        {
            get
            {
                if (_cacheItemList == null)
                {
                    _cacheItemList = gameObject.AddComponent<UIList>();
                    if (uiCharacterItemPrefab != null)
                        _cacheItemList.uiPrefab = uiCharacterItemPrefab.gameObject;
                    _cacheItemList.uiContainer = uiCharacterItemContainer;
                }
                return _cacheItemList;
            }
        }

        private UICharacterSkillSelectionManager _cacheSkillSelectionManager;
        public UICharacterSkillSelectionManager CacheSkillSelectionManager
        {
            get
            {
                if (_cacheSkillSelectionManager == null)
                    _cacheSkillSelectionManager = gameObject.GetOrAddComponent<UICharacterSkillSelectionManager>();
                _cacheSkillSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return _cacheSkillSelectionManager;
            }
        }

        private UICharacterItemSelectionManager _cacheItemSelectionManager;
        public UICharacterItemSelectionManager CacheItemSelectionManager
        {
            get
            {
                if (_cacheItemSelectionManager == null)
                    _cacheItemSelectionManager = gameObject.GetOrAddComponent<UICharacterItemSelectionManager>();
                _cacheItemSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return _cacheItemSelectionManager;
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
