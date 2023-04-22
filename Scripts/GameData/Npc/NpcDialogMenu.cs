using Cysharp.Threading.Tasks;
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
        public Sprite icon;
        public NpcDialogCondition[] showConditions;
        public bool isCloseMenu;
        [BoolShowConditional(nameof(isCloseMenu), false)]
        public BaseNpcDialog dialog;

        public string Title
        {
            get { return Language.GetText(titles, title); }
        }

        public async UniTask<bool> IsPassConditions(IPlayerCharacterData character)
        {
            if (dialog != null && !await dialog.IsPassMenuCondition(character))
                return false;
            foreach (NpcDialogCondition showCondition in showConditions)
            {
                if (!await showCondition.IsPass(character))
                    return false;
            }
            return true;
        }
    }
}