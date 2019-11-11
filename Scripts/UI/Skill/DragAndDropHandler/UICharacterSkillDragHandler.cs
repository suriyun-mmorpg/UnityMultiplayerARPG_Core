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
        public UICharacterSkill uiCharacterSkill { get; private set; }
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

        public override bool CanDrag
        {
            get
            {
                switch (sourceLocation)
                {
                    case SourceLocation.Skills:
                        return uiCharacterSkill != null;
                    case SourceLocation.Hotkey:
                        return uiCharacterHotkey != null;
                }
                return false;
            }
        }

        protected override void Start()
        {
            base.Start();
            rootTransform = CacheUI.CacheRoot.transform;
        }

        public void SetupForSkills(UICharacterSkill uiCharacterSkill)
        {
            sourceLocation = SourceLocation.Skills;
            this.uiCharacterSkill = uiCharacterSkill;
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
            if (sourceLocation == SourceLocation.Hotkey)
                BasePlayerCharacterController.OwningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.None, string.Empty);
        }
    }
}
