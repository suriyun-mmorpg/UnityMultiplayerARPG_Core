using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    /// <summary>
    /// Made this component for navigation demo, because built-in Unity's components not good enough
    /// </summary>
    public class InputField001 : InputField
    {
        public float selectedScaleDuration = 1f;
        public float selectedScaling = 0.05f;
        public bool selectDisabledSelectable = false;
        public Transform scalingTransform;
        private SelectionState _currentSelectionState;
        private Vector3 _defaultLocalScale = Vector3.one;

        protected override void Awake()
        {
            base.Awake();
            if (scalingTransform == null)
                scalingTransform = transform;
            _defaultLocalScale = scalingTransform.localScale;
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            _currentSelectionState = state;
        }

        void Update()
        {
            if (_currentSelectionState == SelectionState.Selected)
                scalingTransform.localScale = new Vector3(_defaultLocalScale.x + Mathf.PingPong(Time.time, selectedScaleDuration) * selectedScaling, _defaultLocalScale.y + Mathf.PingPong(Time.time, selectedScaleDuration) * selectedScaling, scalingTransform.localScale.z);
            else
                scalingTransform.localScale = _defaultLocalScale;
        }

        public override Selectable FindSelectableOnLeft()
        {
            Selectable selectable = base.FindSelectableOnLeft();
            if (selectable != null && !selectable.interactable && !selectDisabledSelectable)
                selectable = null;
            return selectable;
        }

        public override Selectable FindSelectableOnRight()
        {
            Selectable selectable = base.FindSelectableOnRight();
            if (selectable != null && !selectable.interactable && !selectDisabledSelectable)
                selectable = null;
            return selectable;
        }

        public override Selectable FindSelectableOnUp()
        {
            Selectable selectable = base.FindSelectableOnUp();
            if (selectable != null && !selectable.interactable && !selectDisabledSelectable)
                selectable = null;
            return selectable;
        }

        public override Selectable FindSelectableOnDown()
        {
            Selectable selectable = base.FindSelectableOnDown();
            if (selectable != null && !selectable.interactable && !selectDisabledSelectable)
                selectable = null;
            return selectable;
        }
    }
}