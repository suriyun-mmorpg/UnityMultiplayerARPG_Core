using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace MultiplayerARPG
{
    public partial class UICharacterSkills : UIBase
    {
        [Header("Filter")]
        public List<string> filterCategories;
        public List<SkillType> filterSkillTypes;

        [Header("UI Elements")]
        public GameObject listEmptyObject;
        [FormerlySerializedAs("uiSkillDialog")]
        public UICharacterSkill uiDialog;
        [FormerlySerializedAs("uiCharacterSkillPrefab")]
        public UICharacterSkill uiPrefab;
        [FormerlySerializedAs("uiCharacterSkillContainer")]
        public Transform uiContainer;

        [Header("Options")]
        [Tooltip("If this is `TRUE` it won't update data when controlling character's data changes")]
        public bool notForOwningCharacter;

        public bool NotForOwningCharacter
        {
            get { return notForOwningCharacter; }
            set
            {
                notForOwningCharacter = value;
                RegisterOwningCharacterEvents();
            }
        }

        private UIList cacheSkillList;
        public UIList CacheSkillList
        {
            get
            {
                if (cacheSkillList == null)
                {
                    cacheSkillList = gameObject.AddComponent<UIList>();
                    cacheSkillList.uiPrefab = uiPrefab.gameObject;
                    cacheSkillList.uiContainer = uiContainer;
                }
                return cacheSkillList;
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

        public ICharacterData Character { get; protected set; }

        protected virtual void OnEnable()
        {
            CacheSkillSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSkill);
            CacheSkillSelectionManager.eventOnSelect.AddListener(OnSelectCharacterSkill);
            CacheSkillSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterSkill);
            CacheSkillSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterSkill);
            if (uiDialog != null)
                uiDialog.onHide.AddListener(OnSkillDialogHide);
            UpdateOwningCharacterData();
            RegisterOwningCharacterEvents();
        }

        protected virtual void OnDisable()
        {
            if (uiDialog != null)
                uiDialog.onHide.RemoveListener(OnSkillDialogHide);
            CacheSkillSelectionManager.DeselectSelectedUI();
            UnregisterOwningCharacterEvents();
        }

        public void RegisterOwningCharacterEvents()
        {
            UnregisterOwningCharacterEvents();
            if (notForOwningCharacter || !GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onDataIdChange += OnDataIdChange;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation += OnEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onSkillsOperation += OnSkillsOperation;
        }

        public void UnregisterOwningCharacterEvents()
        {
            if (!GameInstance.PlayingCharacterEntity) return;
            GameInstance.PlayingCharacterEntity.onDataIdChange -= OnDataIdChange;
            GameInstance.PlayingCharacterEntity.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
            GameInstance.PlayingCharacterEntity.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
            GameInstance.PlayingCharacterEntity.onEquipItemsOperation -= OnEquipItemsOperation;
            GameInstance.PlayingCharacterEntity.onSkillsOperation -= OnSkillsOperation;
        }

        private void OnDataIdChange(int dataId)
        {
            UpdateOwningCharacterData();
        }

        private void OnEquipWeaponSetChange(byte equipWeaponSet)
        {
            UpdateOwningCharacterData();
        }

        private void OnSelectableWeaponSetsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnEquipItemsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        private void OnSkillsOperation(LiteNetLibSyncList.Operation operation, int index)
        {
            UpdateOwningCharacterData();
        }

        public void UpdateOwningCharacterData()
        {
            if (notForOwningCharacter || GameInstance.PlayingCharacter == null) return;
            UpdateData(GameInstance.PlayingCharacter);
        }

        protected void OnSkillDialogHide()
        {
            CacheSkillSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCharacterSkill(UICharacterSkill ui)
        {
            if (uiDialog != null)
            {
                uiDialog.selectionManager = CacheSkillSelectionManager;
                uiDialog.Setup(ui.Data, Character, ui.IndexOfData);
                uiDialog.Show();
            }
        }

        protected void OnDeselectCharacterSkill(UICharacterSkill ui)
        {
            if (uiDialog != null)
            {
                uiDialog.onHide.RemoveListener(OnSkillDialogHide);
                uiDialog.Hide();
                uiDialog.onHide.AddListener(OnSkillDialogHide);
            }
        }

        public void UpdateData(ICharacterData character)
        {
            Character = character;
            int selectedSkillId = CacheSkillSelectionManager.SelectedUI != null ? CacheSkillSelectionManager.SelectedUI.Skill.DataId : 0;
            CacheSkillSelectionManager.Clear();

            if (character == null || character.GetDatabase() == null)
            {
                if (uiDialog != null)
                    uiDialog.Hide();
                CacheSkillList.HideAll();
                if (listEmptyObject != null)
                    listEmptyObject.SetActive(true);
                return;
            }

            int showingCount = 0;
            UICharacterSkill tempUI;
            CharacterSkill tempCharacterSkill;
            BaseSkill tempSkill;
            int tempIndexOfSkill;
            short tempSkillLevel;
            // Combine skills from database (skill that can level up) with increased skill and equipment skill
            CacheSkillList.Generate(character.GetCaches().Skills, (index, skillLevel, ui) =>
            {
                tempUI = ui.GetComponent<UICharacterSkill>();
                if (string.IsNullOrEmpty(skillLevel.Key.category) ||
                    filterCategories == null || filterCategories.Count == 0 ||
                    filterCategories.Contains(skillLevel.Key.category))
                {
                    if (filterSkillTypes == null || filterSkillTypes.Count == 0 ||
                        filterSkillTypes.Contains(skillLevel.Key.SkillType))
                    {
                        tempSkill = skillLevel.Key;
                        tempIndexOfSkill = character.IndexOfSkill(tempSkill.DataId);
                        tempSkillLevel = (short)(tempIndexOfSkill >= 0 ? character.Skills[tempIndexOfSkill].level : 0);
                        // Set character skill data
                        tempCharacterSkill =  CharacterSkill.Create(tempSkill, tempSkillLevel);
                        // Set UI data
                        tempUI.Setup(new UICharacterSkillData(tempCharacterSkill, skillLevel.Value), character, tempIndexOfSkill);
                        tempUI.Show();
                        UICharacterSkillDragHandler dragHandler = tempUI.GetComponentInChildren<UICharacterSkillDragHandler>();
                        if (dragHandler != null)
                            dragHandler.SetupForSkills(tempUI);
                        CacheSkillSelectionManager.Add(tempUI);
                        if (selectedSkillId == skillLevel.Key.DataId)
                            tempUI.OnClickSelect();
                        showingCount++;
                    }
                    else
                    {
                        // Hide because skill's type not matches in the filter list
                        tempUI.Hide();
                    }
                }
                else
                {
                    // Hide because skill's category not matches in the filter list
                    tempUI.Hide();
                }
            });
            if (listEmptyObject != null)
                listEmptyObject.SetActive(showingCount == 0);
        }
    }
}
