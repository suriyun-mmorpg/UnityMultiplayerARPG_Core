using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UICharacterSkillSelectionManager))]
    public partial class UICharacterSkills : UIBase
    {
        public ICharacterData character { get; protected set; }
        public UICharacterSkill uiSkillDialog;
        public UICharacterSkill uiCharacterSkillPrefab;
        public Transform uiCharacterSkillContainer;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiCharacterSkillPrefab.gameObject;
                    cacheList.uiContainer = uiCharacterSkillContainer;
                }
                return cacheList;
            }
        }

        private UICharacterSkillSelectionManager selectionManager;
        public UICharacterSkillSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICharacterSkillSelectionManager>();
                selectionManager.selectionMode = UISelectionMode.SelectSingle;
                return selectionManager;
            }
        }

        private Dictionary<Skill, short> displayingSkills;

        public override void Show()
        {
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacterSkill);
            SelectionManager.eventOnSelect.AddListener(OnSelectCharacterSkill);
            SelectionManager.eventOnDeselect.RemoveListener(OnDeselectCharacterSkill);
            SelectionManager.eventOnDeselect.AddListener(OnDeselectCharacterSkill);
            base.Show();
        }

        public override void Hide()
        {
            SelectionManager.DeselectSelectedUI();
            base.Hide();
        }

        protected void OnSelectCharacterSkill(UICharacterSkill ui)
        {
            if (uiSkillDialog != null)
            {
                uiSkillDialog.selectionManager = SelectionManager;
                uiSkillDialog.Setup(ui.Data, character, ui.indexOfData);
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

            var selectedSkillId = SelectionManager.SelectedUI != null ? SelectionManager.SelectedUI.Skill.DataId : 0;
            SelectionManager.DeselectSelectedUI();
            SelectionManager.Clear();

            if (character == null)
            {
                CacheList.HideAll();
                return;
            }

            displayingSkills = character.GetSkills();

            Skill tempSkill;
            short tempLevel;
            var skillLevels = character.GetDatabase().CacheSkillLevels;
            CacheList.Generate(skillLevels, (index, skillLevel, ui) =>
            {
                var uiCharacterSkill = ui.GetComponent<UICharacterSkill>();
                tempSkill = skillLevel.Key;
                tempLevel = 0;
                if (displayingSkills.ContainsKey(tempSkill))
                    tempLevel = displayingSkills[tempSkill];
                uiCharacterSkill.Setup(new SkillTuple(tempSkill, tempLevel), character, character.IndexOfSkill(tempSkill.DataId));
                uiCharacterSkill.Show();
                SelectionManager.Add(uiCharacterSkill);
                if (selectedSkillId.Equals(skillLevel.Key))
                    uiCharacterSkill.OnClickSelect();
            });
        }
    }
}
