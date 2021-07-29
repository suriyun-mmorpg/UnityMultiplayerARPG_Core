using UnityEditor;

namespace MultiplayerARPG
{
    [CustomEditor(typeof(CharacterModel))]
    [CanEditMultipleObjects]
    [System.Obsolete]
    public class CharacterModelEditor : BaseCharacterModelEditor
    {
        protected override void SetFieldCondition()
        {
            base.SetFieldCondition();
            ShowOnEnum("animatorType", "Animator", "animator");
            ShowOnEnum("animatorType", "Animator", "animatorController");
            ShowOnEnum("animatorType", "Animator", "defaultAnimatorData");
            ShowOnEnum("animatorType", "Animator", "actionStateLayer");
            ShowOnEnum("animatorType", "Animator", "castSkillStateLayer");
            ShowOnEnum("animatorType", "LegacyAnimtion", "legacyAnimation");
            ShowOnEnum("animatorType", "LegacyAnimtion", "legacyAnimationData");
        }
    }
}
