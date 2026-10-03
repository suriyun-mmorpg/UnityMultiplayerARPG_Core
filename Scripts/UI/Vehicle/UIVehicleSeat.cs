using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    [RequireComponent(typeof(UnityEngine.UI.Button))]
    public class UIVehicleSeat : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public UnityEngine.UI.Button button;
        public UnityEngine.UI.Image background;
        public TMPro.TMP_Text textSeatNumber;
        public GameObject driverIndicator;
        public Color availableColor = new Color(0.86f, 0.9f, 0.94f);
        public Color currentColor = new Color(1f, 0.74f, 0.18f);
        public Color occupiedColor = new Color(0.38f, 0.4f, 0.44f);
        public Color previewColor = new Color(0.28f, 0.78f, 1f);

        private UIVehicleSeatChanger _owner;
        private byte _seatIndex;
        private bool _ignoreClick;
        public byte SeatIndex => _seatIndex;
        public RectTransform RectTransform => (RectTransform)transform;

        public void Setup(UIVehicleSeatChanger owner, byte seatIndex)
        {
            if (button == null)
                button = GetComponent<UnityEngine.UI.Button>();
            button.onClick.RemoveListener(OnClick);
            _owner = owner;
            _seatIndex = seatIndex;
            button.onClick.AddListener(OnClick);
            if (textSeatNumber != null)
                textSeatNumber.text = (seatIndex + 1).ToString();
            if (driverIndicator != null)
                driverIndicator.SetActive(seatIndex == 0);
        }

        public void SetState(bool current, bool selectable, bool preview)
        {
            if (button != null)
                button.interactable = selectable;
            if (background != null)
                background.color = preview ? previewColor : current ? currentColor :
                    selectable ? availableColor : occupiedColor;
        }

        private void OnClick()
        {
            if (!_ignoreClick && _owner != null)
                _owner.SelectSeat(_seatIndex);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            _ignoreClick = false;
            if (eventData.button == PointerEventData.InputButton.Left && _owner != null)
                _owner.BeginGesture(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!eventData.dragging && _owner != null)
                _owner.EndGesture(eventData, false);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;

        public void OnBeginDrag(PointerEventData eventData)
        {
            _ignoreClick = true;
            _owner?.UpdateGesture(eventData);
        }

        public void OnDrag(PointerEventData eventData) => _owner?.UpdateGesture(eventData);
        public void OnEndDrag(PointerEventData eventData) => _owner?.EndGesture(eventData, true);
    }
}
