using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class UICharacterSkillDragHandler : UIDragHandler
    {
        public enum SourceLocation
        {
            Skills,
            Hotkey,
        }

        public SourceLocation sourceLocation;
        public string hotkeyId;

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

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
                return;
            if (sourceLocation == SourceLocation.Hotkey)
                owningCharacter.RequestAssignHotkey(hotkeyId, HotkeyType.None, 0);
        }
    }
}
