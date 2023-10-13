using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public partial class UIGuildSkillDragHandler : UIDragHandler
    {
        public enum SourceLocation : byte
        {
            Skills,
            Hotkey,
        }

        [Tooltip("If this is `TRUE`, it have to be dropped on drop handler to proceed activities")]
        public bool requireDropArea = false;
        public bool enableUnassignHotkeyAction = true;

        public SourceLocation Location { get; protected set; }
        public UIGuildSkill UISkill { get; protected set; }
        public UICharacterHotkey UIHotkey { get; protected set; }

        protected UIGuildSkill _cacheUI;
        public UIGuildSkill CacheUI
        {
            get
            {
                if (_cacheUI == null)
                    _cacheUI = GetComponent<UIGuildSkill>();
                return _cacheUI;
            }
        }

        public override bool CanDrag
        {
            get
            {
                switch (Location)
                {
                    case SourceLocation.Skills:
                        return UISkill != null;
                    case SourceLocation.Hotkey:
                        return UIHotkey != null;
                }
                return false;
            }
        }

        protected override void Start()
        {
            base.Start();
            rootTransform = CacheUI.CacheRoot.transform;
        }

        public void SetupForSkills(UIGuildSkill uiGuildSkill)
        {
            Location = SourceLocation.Skills;
            UISkill = uiGuildSkill;
        }

        public void SetupForHotkey(UICharacterHotkey uiGuildHotkey)
        {
            Location = SourceLocation.Hotkey;
            UIHotkey = uiGuildHotkey;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (IsScrolling)
            {
                base.OnEndDrag(eventData);
                return;
            }
            base.OnEndDrag(eventData);
            if (IsDropped || !CanDrag)
                return;
            if (requireDropArea)
                return;
            if (enableUnassignHotkeyAction && Location == SourceLocation.Hotkey)
                GameInstance.PlayingCharacterEntity.UnAssignHotkey(UIHotkey.hotkeyId);
        }
    }
}
