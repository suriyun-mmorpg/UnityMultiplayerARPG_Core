namespace MultiplayerARPG
{
    public class UICharacterItemDragHandler : UIDragHandler
    {
        private UICharacterItem cacheUI;
        public UICharacterItem CacheUI
        {
            get
            {
                if (cacheUI == null)
                    cacheUI = GetComponent<UICharacterItem>();
                return cacheUI;
            }
        }

        private void Start()
        {
            rootTransform = CacheUI.CacheRoot.transform;
        }
    }
}
