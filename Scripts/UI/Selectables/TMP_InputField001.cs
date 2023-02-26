using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    /// <summary>
    /// Made this component for navigation demo, because built-in Unity's components not good enough
    /// </summary>
    public class TMP_InputField001 : InputField
    {
        public float selectedScaleDuration = 1f;
        public float selectedScaling = 0.05f;
        public bool selectDisabledSelectable = false;
        public Transform scalingTransform;
        public List<Selectable> upSelectables = new List<Selectable>();
        public List<Selectable> downSelectables = new List<Selectable>();
        public List<Selectable> leftSelectables = new List<Selectable>();
        public List<Selectable> rightSelectables = new List<Selectable>();
        private SelectionState _currentSelectionState;
        private Vector3 _defaultLocalScale = Vector3.one;

        protected override void Awake()
        {
            base.Awake();
            if (scalingTransform == null)
                scalingTransform = transform;
            _defaultLocalScale = scalingTransform.localScale;
        }

        protected Selectable GetFirstSelectable(List<Selectable> list, Selectable defaultExplicit)
        {
            if (list == null)
                return null;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] != null && list[i].IsInteractable() && list[i].gameObject.activeInHierarchy)
                {
                    return list[i];
                }
            }

            return defaultExplicit;
        }

        public void AddUpSelectable(Selectable selectable, bool prioritize = false)
        {
            AddSelectable(upSelectables, selectable, prioritize);
        }

        public void AddDownSelectable(Selectable selectable, bool prioritize = false)
        {
            AddSelectable(downSelectables, selectable, prioritize);
        }

        public void AddLeftSelectable(Selectable selectable, bool prioritize = false)
        {
            AddSelectable(leftSelectables, selectable, prioritize);
        }

        public void AddRightSelectable(Selectable selectable, bool prioritize = false)
        {
            AddSelectable(rightSelectables, selectable, prioritize);
        }

        protected void AddSelectable(List<Selectable> list, Selectable selectable, bool prioritize)
        {
            if (list == null)
            {
                list = new List<Selectable>();
            }

            if (prioritize)
            {
                list.Insert(0, selectable);
            }
            else
            {
                list.Add(selectable);
            }
        }

        public void Clear()
        {
            upSelectables.Clear();
            downSelectables.Clear();
            leftSelectables.Clear();
            rightSelectables.Clear();
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
            Selectable selectable = GetFirstSelectable(leftSelectables, base.FindSelectableOnLeft());
            if (selectable != null && !selectable.interactable && !selectDisabledSelectable)
                selectable = null;
            return selectable;
        }

        public override Selectable FindSelectableOnRight()
        {
            Selectable selectable = GetFirstSelectable(rightSelectables, base.FindSelectableOnRight());
            if (selectable != null && !selectable.interactable && !selectDisabledSelectable)
                selectable = null;
            return selectable;
        }

        public override Selectable FindSelectableOnUp()
        {
            Selectable selectable = GetFirstSelectable(upSelectables, base.FindSelectableOnUp());
            if (selectable != null && !selectable.interactable && !selectDisabledSelectable)
                selectable = null;
            return selectable;
        }

        public override Selectable FindSelectableOnDown()
        {
            Selectable selectable = GetFirstSelectable(downSelectables, base.FindSelectableOnDown());
            if (selectable != null && !selectable.interactable && !selectDisabledSelectable)
                selectable = null;
            return selectable;
        }
    }
}