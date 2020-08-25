using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = "Skill Item", menuName = "Create GameData/Item/Skill Item", order = -4882)]
    public partial class SkillItem : BaseItem, ISkillItem
    {
        public override string TypeTitle
        {
            get { return LanguageManager.GetText(UIItemTypeKeys.UI_ITEM_TYPE_SKILL.ToString()); }
        }

        public override ItemType ItemType
        {
            get { return ItemType.Skill; }
        }

        [Header("Skill Configs")]
        [SerializeField]
        private BaseSkill usingSkill;
        public BaseSkill UsingSkill
        {
            get { return usingSkill; }
        }

        [SerializeField]
        private short usingSkillLevel;
        public short UsingSkillLevel
        {
            get { return usingSkillLevel; }
        }

        public void UseItem(BaseCharacterEntity characterEntity, short itemIndex, CharacterItem characterItem)
        {
            // TODO: May changes this function later.
        }

        public bool HasCustomAimControls()
        {
            return UsingSkill.HasCustomAimControls();
        }

        public Vector3? UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            return UsingSkill.UpdateAimControls(aimAxes, UsingSkillLevel);
        }

        public void FinishAimControls(bool isCancel)
        {
            UsingSkill.FinishAimControls(isCancel);
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddSkills(UsingSkill);
        }
    }
}
