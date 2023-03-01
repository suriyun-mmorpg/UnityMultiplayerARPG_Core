using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(Selectable))]
    public class NavigationChild : MonoBehaviour
    {
        public NavigationGroup group;
        public bool shouldBeFirstSelected;
        public bool vertical = true;
        public bool horizontal = true;

        public Selectable Selectable { get; private set; }

        private void Awake()
        {
            Selectable = GetComponent<Selectable>();
        }

        private void Start()
        {
            if (group == null)
                group = GetComponentInParent<NavigationGroup>();
            if (group != null)
                group.AddChild(this);
        }

        private void Update()
        {
            if (group != null && EventSystem.current.currentSelectedGameObject == gameObject)
                group.SetLastSelectedChild(this);
        }

        private void OnDestroy()
        {
            if (group != null)
                group.RemoveChild(this);
        }
    }
}
