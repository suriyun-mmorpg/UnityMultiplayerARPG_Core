using UnityEngine;

namespace MultiplayerARPG
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.SKILL_ITEM_FILE, menuName = GameDataMenuConsts.SKILL_ITEM_MENU, order = GameDataMenuConsts.SKILL_ITEM_ORDER)]
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

        [Category(3, "Skill Settings")]
        [SerializeField]
        private BaseSkill usingSkill = null;
        public BaseSkill UsingSkill
        {
            get { return usingSkill; }
        }

        [SerializeField]
        private int usingSkillLevel = 0;
        public int UsingSkillLevel
        {
            get { return usingSkillLevel; }
        }

        [SerializeField]
        private float useItemCooldown = 0f;
        public float UseItemCooldown
        {
            get { return useItemCooldown; }
        }

        public void UseItem(BaseCharacterEntity characterEntity, int itemIndex, CharacterItem characterItem)
        {
            // TODO: May changes this function later.
        }

        public bool HasCustomAimControls()
        {
            return UsingSkill.HasCustomAimControls();
        }

        public AimPosition UpdateAimControls(Vector2 aimAxes, params object[] data)
        {
            return UsingSkill.UpdateAimControls(aimAxes, UsingSkillLevel);
        }

        public void FinishAimControls(bool isCancel)
        {
            UsingSkill.FinishAimControls(isCancel);
        }

        public bool IsChanneledAbility()
        {
            return UsingSkill.IsChanneledAbility();
        }

        public override void PrepareRelatesData()
        {
            base.PrepareRelatesData();
            GameInstance.AddSkills(UsingSkill);
        }
    }
}
