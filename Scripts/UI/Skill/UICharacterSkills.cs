using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterSkills : UIBase
    {
        public ICharacterData character { get; protected set; }

        [Header("UI Elements")]
        public UICharacterSkill uiSkillDialog;
        public UICharacterSkill uiCharacterSkillPrefab;
        public List<string> filterCategories;
        public List<SkillType> filterSkillTypes;
        public Transform uiCharacterSkillContainer;

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
                    cacheSkillList.uiPrefab = uiCharacterSkillPrefab.gameObject;
                    cacheSkillList.uiContainer = uiCharacterSkillContainer;
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

        protected virtual void OnEnable()
        {
            CacheSkillSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSkill);
            CacheSkillSelectionManager.eventOnSelect.AddListener(OnSelectCharacterSkill);
            CacheSkillSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterSkill);
            CacheSkillSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterSkill);
            if (uiSkillDialog != null)
                uiSkillDialog.onHide.AddListener(OnSkillDialogHide);
            UpdateOwningCharacterData();
            RegisterOwningCharacterEvents();
        }

        protected virtual void OnDisable()
        {
            if (uiSkillDialog != null)
                uiSkillDialog.onHide.RemoveListener(OnSkillDialogHide);
            CacheSkillSelectionManager.DeselectSelectedUI();
            UnregisterOwningCharacterEvents();
        }

        public void RegisterOwningCharacterEvents()
        {
            UnregisterOwningCharacterEvents();
            if (notForOwningCharacter || !BasePlayerCharacterController.OwningCharacter) return;
            BasePlayerCharacterController.OwningCharacter.onDataIdChange += OnDataIdChange;
            BasePlayerCharacterController.OwningCharacter.onEquipWeaponSetChange += OnEquipWeaponSetChange;
            BasePlayerCharacterController.OwningCharacter.onSelectableWeaponSetsOperation += OnSelectableWeaponSetsOperation;
            BasePlayerCharacterController.OwningCharacter.onEquipItemsOperation += OnEquipItemsOperation;
            BasePlayerCharacterController.OwningCharacter.onSkillsOperation += OnSkillsOperation;
        }

        public void UnregisterOwningCharacterEvents()
        {
            if (!BasePlayerCharacterController.OwningCharacter) return;
            BasePlayerCharacterController.OwningCharacter.onDataIdChange -= OnDataIdChange;
            BasePlayerCharacterController.OwningCharacter.onEquipWeaponSetChange -= OnEquipWeaponSetChange;
            BasePlayerCharacterController.OwningCharacter.onSelectableWeaponSetsOperation -= OnSelectableWeaponSetsOperation;
            BasePlayerCharacterController.OwningCharacter.onEquipItemsOperation -= OnEquipItemsOperation;
            BasePlayerCharacterController.OwningCharacter.onSkillsOperation -= OnSkillsOperation;
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

        private void UpdateOwningCharacterData()
        {
            if (notForOwningCharacter || !BasePlayerCharacterController.OwningCharacter) return;
            UpdateData(BasePlayerCharacterController.OwningCharacter);
        }

        protected void OnSkillDialogHide()
        {
            CacheSkillSelectionManager.DeselectSelectedUI();
        }

        protected void OnSelectCharacterSkill(UICharacterSkill ui)
        {
            if (uiSkillDialog != null)
            {
                uiSkillDialog.selectionManager = CacheSkillSelectionManager;
                uiSkillDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiSkillDialog.Show();
            }
        }

        protected void OnDeselectCharacterSkill(UICharacterSkill ui)
        {
            if (uiSkillDialog != null)
            {
                uiSkillDialog.onHide.RemoveListener(OnSkillDialogHide);
                uiSkillDialog.Hide();
                uiSkillDialog.onHide.AddListener(OnSkillDialogHide);
            }
        }

        public void UpdateData(ICharacterData character)
        {
            this.character = character;
            int selectedSkillId = CacheSkillSelectionManager.SelectedUI != null ? CacheSkillSelectionManager.SelectedUI.Skill.DataId : 0;
            CacheSkillSelectionManager.DeselectSelectedUI();
            CacheSkillSelectionManager.Clear();

            if (character == null)
            {
                CacheSkillList.HideAll();
                return;
            }

            BaseCharacter database = character.GetDatabase();
            if (database != null)
            {
                // Generate UIs
                UICharacterSkill tempUiCharacterSkill;
                CharacterSkill tempCharacterSkill;
                BaseSkill tempSkill;
                int tempIndexOfSkill;
                // Combine skills from database (skill that can level up) with increased skill and equipment skill
                CacheSkillList.Generate(character.GetCaches().Skills, (index, skillLevel, ui) =>
                {
                    tempUiCharacterSkill = ui.GetComponent<UICharacterSkill>();
                    if (string.IsNullOrEmpty(skillLevel.Key.category) ||
                        filterCategories == null || filterCategories.Count == 0 ||
                        filterCategories.Contains(skillLevel.Key.category))
                    {
                        if (filterSkillTypes == null || filterSkillTypes.Count == 0 ||
                            filterSkillTypes.Contains(skillLevel.Key.SkillType))
                        {
                            tempSkill = skillLevel.Key;
                            tempIndexOfSkill = character.IndexOfSkill(tempSkill.DataId);
                            // Set character skill data
                            tempCharacterSkill = CharacterSkill.Create(tempSkill, skillLevel.Value);
                            // Set UI data
                            tempUiCharacterSkill.Setup(new UICharacterSkillData(tempCharacterSkill), character, tempIndexOfSkill);
                            tempUiCharacterSkill.Show();
                            UICharacterSkillDragHandler dragHandler = tempUiCharacterSkill.GetComponentInChildren<UICharacterSkillDragHandler>();
                            if (dragHandler != null)
                                dragHandler.SetupForSkills(tempUiCharacterSkill);
                            CacheSkillSelectionManager.Add(tempUiCharacterSkill);
                            if (selectedSkillId == skillLevel.Key.DataId)
                                tempUiCharacterSkill.OnClickSelect();
                        }
                        else
                        {
                            tempUiCharacterSkill.Hide();
                        }
                    }
                    else
                    {
                        tempUiCharacterSkill.Hide();
                    }
                });
            }
        }
    }
}
