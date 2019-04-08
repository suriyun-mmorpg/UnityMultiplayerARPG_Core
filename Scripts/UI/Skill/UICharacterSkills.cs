using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterSkills : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterSkill uiSkillDialog;
        public UICharacterSkill uiCharacterSkillPrefab;
        public List<string> filterCategories;
        public List<SkillType> filterSkillTypes;
        public Transform uiCharacterSkillContainer;

        private UIList cacheCharacterSkillList;
        public UIList CacheCharacterSkillList
        {
            get
            {
                if (cacheCharacterSkillList == null)
                {
                    cacheCharacterSkillList = gameObject.AddComponent<UIList>();
                    cacheCharacterSkillList.uiPrefab = uiCharacterSkillPrefab.gameObject;
                    cacheCharacterSkillList.uiContainer = uiCharacterSkillContainer;
                }
                return cacheCharacterSkillList;
            }
        }

        private UICharacterSkillSelectionManager cacheCharacterSkillSelectionManager;
        public UICharacterSkillSelectionManager CacheCharacterSkillSelectionManager
        {
            get
            {
                if (cacheCharacterSkillSelectionManager == null)
                    cacheCharacterSkillSelectionManager = GetComponent<UICharacterSkillSelectionManager>();
                if (cacheCharacterSkillSelectionManager == null)
                    cacheCharacterSkillSelectionManager = gameObject.AddComponent<UICharacterSkillSelectionManager>();
                cacheCharacterSkillSelectionManager.selectionMode = UISelectionMode.SelectSingle;
                return cacheCharacterSkillSelectionManager;
            }
        }

        private Dictionary<Skill, short> displayingSkills;

        public override void Show()
        {
            CacheCharacterSkillSelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSkill);
            CacheCharacterSkillSelectionManager.eventOnSelect.AddListener(OnSelectCharacterSkill);
            CacheCharacterSkillSelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterSkill);
            CacheCharacterSkillSelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterSkill);
            base.Show();
        }

        public override void Hide()
        {
            CacheCharacterSkillSelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterSkill(UICharacterSkill ui)
        {
            if (uiSkillDialog != null)
            {
                uiSkillDialog.selectionManager = CacheCharacterSkillSelectionManager;
                uiSkillDialog.Setup(ui.Data, character, ui.IndexOfData);
                uiSkillDialog.Show();
            }
        }

        protected void OnDeselectCharacterSkill(UICharacterSkill ui)
        {
            if (uiSkillDialog != null)
                uiSkillDialog.Hide();
        }

        public void UpdateData(ICharacterData character)
        {
            this.character = character;
            int selectedSkillId = CacheCharacterSkillSelectionManager.SelectedUI != null ? CacheCharacterSkillSelectionManager.SelectedUI.Skill.DataId : 0;
            CacheCharacterSkillSelectionManager.DeselectSelectedUI();
            CacheCharacterSkillSelectionManager.Clear();

            if (character == null)
            {
                CacheCharacterSkillList.HideAll();
                return;
            }

            // All skills included equipment skill levels
            displayingSkills = character.GetSkills();

            BaseCharacter database = character.GetDatabase();
            if (database != null)
            {
                CharacterSkill tempCharacterSkill;
                Skill tempSkill;
                int tempIndexOfSkill;
                short tempLevel;
                // Combine skills from database (skill that can level up) and equipment skills
                Dictionary<Skill, short> skillLevels = new Dictionary<Skill, short>();
                skillLevels = GameDataHelpers.CombineSkills(skillLevels, database.CacheSkillLevels);
                skillLevels = GameDataHelpers.CombineSkills(skillLevels, character.GetEquipmentSkills());
                // Filter skills to show by specific skill types / categories
                Dictionary<Skill, short> filteredSkillLevels = new Dictionary<Skill, short>();
                foreach (KeyValuePair<Skill, short> skillLevel in skillLevels)
                {
                    if (string.IsNullOrEmpty(skillLevel.Key.category) ||
                        filterCategories == null || filterCategories.Count == 0 ||
                        filterCategories.Contains(skillLevel.Key.category))
                    {
                        if (filterSkillTypes == null || filterSkillTypes.Count == 0 ||
                            filterSkillTypes.Contains(skillLevel.Key.skillType))
                            filteredSkillLevels.Add(skillLevel.Key, skillLevel.Value);
                    }
                }
                skillLevels = filteredSkillLevels;
                // Generate UIs
                CacheCharacterSkillList.Generate(skillLevels, (index, skillLevel, ui) =>
                {
                    UICharacterSkill uiCharacterSkill = ui.GetComponent<UICharacterSkill>();
                    tempSkill = skillLevel.Key;
                    tempIndexOfSkill = character.IndexOfSkill(tempSkill.DataId);
                    // Set character skill data
                    if (tempIndexOfSkill >= 0)
                        tempCharacterSkill = character.Skills[tempIndexOfSkill];
                    else
                        tempCharacterSkill = CharacterSkill.Create(tempSkill, 0);
                    // Set skill level data
                    tempLevel = 0;
                    if (displayingSkills.ContainsKey(tempSkill))
                        tempLevel = displayingSkills[tempSkill];
                    // Set UI data
                    uiCharacterSkill.Setup(new CharacterSkillTuple(tempCharacterSkill, tempLevel), character, tempIndexOfSkill);
                    uiCharacterSkill.Show();
                    UICharacterSkillDragHandler dragHandler = uiCharacterSkill.GetComponentInChildren<UICharacterSkillDragHandler>();
                    if (dragHandler != null)
                        dragHandler.SetupForSkills(uiCharacterSkill);
                    CacheCharacterSkillSelectionManager.Add(uiCharacterSkill);
                    if (selectedSkillId == skillLevel.Key.DataId)
                        uiCharacterSkill.OnClickSelect();
                });
            }
        }
    }
}
