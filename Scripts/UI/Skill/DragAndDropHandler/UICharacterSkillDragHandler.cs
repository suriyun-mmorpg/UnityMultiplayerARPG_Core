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
                        return uiCharacterSkill != null && uiCharacterSkill.IndexOfData >= 0;
                    case SourceLocation.Hotkey:
                        return uiCharacterHotkey != null;
                }
                return false;
            }
        }

        private void Start()
        {
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
            BasePlayerCharacterEntity owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter == null)
                return;
            if (sourceLocation == SourceLocation.Hotkey)
                owningCharacter.RequestAssignHotkey(uiCharacterHotkey.hotkeyId, HotkeyType.None, 0);
        }
    }
}
