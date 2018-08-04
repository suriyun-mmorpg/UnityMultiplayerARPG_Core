using UnityEngine.UI;

namespace MultiplayerARPG
{
    [System.Serializable]
    public struct UISkillTextPair
    {
        public Skill skill;
        public Text text;
        public TextWrapper uiText;
    }
}
