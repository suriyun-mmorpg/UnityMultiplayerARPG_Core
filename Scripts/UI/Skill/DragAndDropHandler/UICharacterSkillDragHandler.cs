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

        public SourceLocation sourceLocation { get; private set; }
        // Skills data
        public UICharacterSkills uiCharacterSkills { get; private set; }
        // Hotkey data
        public UICharacterHotkey uiCharacterHotkey { get; private set; }

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

        public void SetupForSkills(UICharacterSkills uiCharacterSkills)
        {
            sourceLocation = SourceLocation.Skills;
            this.uiCharacterSkills = uiCharacterSkills;
        }

        public void SetupForHotkey(UICharacterHotkey uiCharacterHotkey)
        {
            sourceLocation = SourceLocation.Hotkey;
            this.uiCharacterHotkey = uiCharacterHotkey;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            if (isDropped || !CanDrag)
                return;
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
                return;
            if (sourceLocation == SourceLocation.Hotkey)
                owningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.None, 0);
        }
    }
}
