using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public partial class UICharacterSkillDragHandler : UIDragHandler
    {
        public enum SourceLocation
        {
            Skills,
            Hotkey,
        }

        [Tooltip("If this is `TRUE`, it have to be dropped on drop handler to proceed activities")]
        public bool requireDropArea;

        public SourceLocation sourceLocation { get; protected set; }
        // Skills data
        public UICharacterSkill uiCharacterSkill { get; protected set; }
        // Hotkey data
        public UICharacterHotkey uiCharacterHotkey { get; protected set; }

        protected UICharacterSkill cacheUI;
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
            if (sourceLocation == SourceLocation.Hotkey)
                GameInstance.PlayingCharacterEntity.UnAssignHotkey(uiCharacterHotkey.hotkeyId);
        }
    }
}
