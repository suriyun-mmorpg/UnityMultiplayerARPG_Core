using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct NpcDialogMenu
    {
        [Tooltip("Default title")]
        public string title;
        [Tooltip("Titles by language keys")]
        public LanguageData[] titles;
        public NpcDialogCondition[] showConditions;
        public bool isCloseMenu;
        [BoolShowConditional(conditionFieldName: "isCloseMenu", conditionValue: false)]
        public NpcDialog dialog;

        public string Title
        {
            get { return Language.GetText(titles, title); }
        }

        public bool IsPassConditions(IPlayerCharacterData character)
        {
            if (dialog != null && dialog.type == NpcDialogType.Quest)
            {
                if (dialog.quest == null)
                    return false;
                int indexOfQuest = character.IndexOfQuest(dialog.quest.DataId);
                if (indexOfQuest >= 0 && character.Quests[indexOfQuest].isComplete)
                    return false;
            }
            foreach (NpcDialogCondition showCondition in showConditions)
            {
                if (!showCondition.IsPass(character))
                    return false;
            }
            return true;
        }
    }
}