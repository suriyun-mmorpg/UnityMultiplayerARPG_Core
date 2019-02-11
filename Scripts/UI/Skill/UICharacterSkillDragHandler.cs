namespace MultiplayerARPG
{
    public class UICharacterSkillDragHandler : UIDragHandler
    {
        private UICharacterSkill cacheUI;
        public UICharacterSkill CacheUI
        {
            get
            {
                if (cacheUI == null)
                    cacheUI = GetComponent<UICharacterSkill>();
                return cacheUI;
            }
        }

        private void Start()
        {
            rootTransform = CacheUI.CacheRoot.transform;
        }
    }
}
