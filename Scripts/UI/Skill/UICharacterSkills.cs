using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public partial class UICharacterSkills : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterSkill uiSkillDialog;
        public UICharacterSkill uiCharacterSkillPrefab;
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

            displayingSkills = character.GetSkills();

            BaseCharacter database = character.GetDatabase();
            if (database != null)
            {
                CharacterSkill tempCharacterSkill;
                Skill tempSkill;
                int tempIndexOfSkill;
                short tempLevel;
                Dictionary<Skill, short> skillLevels = database.CacheSkillLevels;
                if (filterSkillTypes != null && filterSkillTypes.Count > 0)
                {
                    // Filter skills to show by specific skill types
                    Dictionary<Skill, short> filteredSkillLevels = new Dictionary<Skill, short>();
                    foreach (KeyValuePair<Skill, short> skillLevel in skillLevels)
                    {
                        if (filterSkillTypes.Contains(skillLevel.Key.skillType))
                            filteredSkillLevels.Add(skillLevel.Key, skillLevel.Value);
                    }
                    skillLevels = filteredSkillLevels;
                }
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
                        dragHandler.SetupForSkills(this);
                    CacheCharacterSkillSelectionManager.Add(uiCharacterSkill);
                    if (selectedSkillId.Equals(skillLevel.Key))
                        uiCharacterSkill.OnClickSelect();
                });
            }
        }
    }
}
