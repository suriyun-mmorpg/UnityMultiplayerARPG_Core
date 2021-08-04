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
            CharacterModel model = target as CharacterModel;
            ShowOnEnum(nameof(model.animatorType), nameof(CharacterModel.AnimatorType.Animator), nameof(model.animator));
            ShowOnEnum(nameof(model.animatorType), nameof(CharacterModel.AnimatorType.Animator), nameof(model.animatorController));
            ShowOnEnum(nameof(model.animatorType), nameof(CharacterModel.AnimatorType.Animator), nameof(model.defaultAnimatorData));
            ShowOnEnum(nameof(model.animatorType), nameof(CharacterModel.AnimatorType.Animator), nameof(model.actionStateLayer));
            ShowOnEnum(nameof(model.animatorType), nameof(CharacterModel.AnimatorType.Animator), nameof(model.castSkillStateLayer));
            ShowOnEnum(nameof(model.animatorType), nameof(CharacterModel.AnimatorType.LegacyAnimtion), nameof(model.legacyAnimation));
            ShowOnEnum(nameof(model.animatorType), nameof(CharacterModel.AnimatorType.LegacyAnimtion), nameof(model.legacyAnimationData));
        }
    }
}
