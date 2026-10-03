using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MultiplayerARPG
{
    public class UIVehicleSeatChanger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
        IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("A child panel. Keep this observer outside the panel it hides.")]
        public GameObject controlsRoot;
        public RectTransform seatsContainer;
        public UIVehicleSeat seatPrefab;
        public Vector2 seatLayoutSize = new Vector2(104f, 140f);
        [Tooltip("Minimum swipe length in UI units. Swipe toward a seat; releasing over its icon is not required.")]
        [Min(1f)] public float minimumSwipeDistance = 16f;

        private BasePlayerCharacterEntity _character;
        private IVehicleEntity _vehicle;
        private readonly List<UIVehicleSeat> _entries = new List<UIVehicleSeat>();
        private int _pointerId = int.MinValue;
        private int _previewSeat = -1;
        private IVehicleEntity _gestureVehicle;
        private byte _gestureStartingSeat;
        private Vector2 _gestureStartPosition;
        private Camera _gestureCamera;
        public IVehicleEntity Vehicle => _vehicle;

        private void OnEnable()
        {
            GameInstance.OnSetPlayingCharacterEvent += OnPlayingCharacterChanged;
            OnPlayingCharacterChanged(GameInstance.PlayingCharacter);
        }

        private void OnDisable()
        {
            GameInstance.OnSetPlayingCharacterEvent -= OnPlayingCharacterChanged;
            OnPlayingCharacterChanged(null);
        }

        private void OnPlayingCharacterChanged(IPlayerCharacterData character)
        {
            BasePlayerCharacterEntity nextCharacter = character as BasePlayerCharacterEntity;
            if (ReferenceEquals(_character, nextCharacter))
            {
                Refresh();
                return;
            }
            if (_character != null)
                _character.onSetPassengingVehicle -= OnCharacterVehicleChanged;
            _character = nextCharacter;
            if (_character != null)
                _character.onSetPassengingVehicle += OnCharacterVehicleChanged;
            CancelGesture();
            Refresh();
        }

        private void OnCharacterVehicleChanged(BaseGameEntity entity)
        {
            if (entity != _character)
                return;
            if (_pointerId != int.MinValue &&
                (!ReferenceEquals(_gestureVehicle, _character.PassengingVehicleEntity) ||
                 _gestureStartingSeat != _character.PassengingVehicleSeatIndex))
                CancelGesture();
            Refresh();
        }

        private void BindVehicle(IVehicleEntity vehicle)
        {
            if (ReferenceEquals(_vehicle, vehicle))
                return;
            if (!ReferenceEquals(_vehicle, null))
            {
                _vehicle.onPassengersChanged -= Refresh;
                if (_vehicle.Entity != null)
                    _vehicle.Entity.onDestroy -= OnVehicleDestroyed;
            }
            _vehicle = vehicle;
            CancelGesture();
            if (!vehicle.IsNull())
            {
                vehicle.onPassengersChanged += Refresh;
                vehicle.Entity.onDestroy += OnVehicleDestroyed;
            }
        }

        private void OnVehicleDestroyed(BaseGameEntity entity)
        {
            BindVehicle(null);
            HideControls();
        }

        public void Refresh()
        {
            IVehicleEntity vehicle = _character != null ? _character.PassengingVehicleEntity : null;
            BindVehicle(vehicle);
            if (vehicle.IsNull() || vehicle.Seats == null || vehicle.Seats.Count == 0 ||
                _character.PassengingVehicleSeatIndex >= vehicle.Seats.Count ||
                seatsContainer == null || seatPrefab == null)
            {
                HideControls();
                return;
            }
            SetControlsActive(true);
            int count = Mathf.Min(256, vehicle.Seats.Count);
            for (int i = 0; i < count; ++i)
            {
                if (_entries.Count <= i)
                {
                    UIVehicleSeat entry = Instantiate(seatPrefab, seatsContainer);
                    entry.name = "Seat " + (i + 1);
                    entry.Setup(this, (byte)i);
                    _entries.Add(entry);
                }
                UIVehicleSeat seat = _entries[i];
                seat.gameObject.SetActive(true);
                seat.RectTransform.anchoredPosition = Vector2.Scale(seatLayoutSize,
                    VehicleSeatUILayout.GetNormalizedPosition(vehicle, i));
            }
            for (int i = count; i < _entries.Count; ++i)
                _entries[i].gameObject.SetActive(false);
            RefreshStates();
        }

        private void HideControls()
        {
            CancelGesture();
            SetControlsActive(false);
            foreach (UIVehicleSeat entry in _entries)
                entry.gameObject.SetActive(false);
        }

        private void SetControlsActive(bool active)
        {
            if (controlsRoot != null && !transform.IsChildOf(controlsRoot.transform))
                controlsRoot.SetActive(active);
        }

        private void RefreshStates()
        {
            for (int i = 0; i < _entries.Count; ++i)
            {
                bool current = _character != null && i == _character.PassengingVehicleSeatIndex;
                bool selectable = _character != null && !_vehicle.IsNull() &&
                    _character.CanChangeVehicleSeat((byte)i, out _);
                _entries[i].SetState(current, selectable, selectable && i == _previewSeat);
            }
        }

        public bool SelectSeat(int seatIndex)
        {
            if (seatIndex < 0 || seatIndex > byte.MaxValue || _character == null ||
                _vehicle.IsNull() || !ReferenceEquals(_character.PassengingVehicleEntity, _vehicle) ||
                !_character.CanChangeVehicleSeat((byte)seatIndex, out _))
                return false;
            SendChangeSeatRequest(_vehicle.Entity.ObjectId, (byte)seatIndex);
            return true;
        }

        protected virtual void SendChangeSeatRequest(uint vehicleObjectId, byte seatIndex)
        {
            _character.CallCmdChangeVehicleSeat(vehicleObjectId, seatIndex);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
                BeginGesture(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!eventData.dragging)
                EndGesture(eventData, false);
        }

        public void OnInitializePotentialDrag(PointerEventData eventData) => eventData.useDragThreshold = false;
        public void OnBeginDrag(PointerEventData eventData) => UpdateGesture(eventData);
        public void OnDrag(PointerEventData eventData) => UpdateGesture(eventData);
        public void OnEndDrag(PointerEventData eventData) => EndGesture(eventData, true);

        internal void BeginGesture(PointerEventData eventData)
        {
            if (_pointerId != int.MinValue || _character == null || _vehicle.IsNull())
                return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(seatsContainer,
                eventData.position, eventData.pressEventCamera, out _gestureStartPosition))
                return;
            _pointerId = eventData.pointerId;
            _gestureVehicle = _vehicle;
            _gestureStartingSeat = _character.PassengingVehicleSeatIndex;
            _gestureCamera = eventData.pressEventCamera;
            _previewSeat = -1;
        }

        internal void UpdateGesture(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left || _pointerId != eventData.pointerId)
                return;
            _previewSeat = -1;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(seatsContainer,
                eventData.position, _gestureCamera, out Vector2 position))
                _previewSeat = GetSeatInSwipeDirection(position - _gestureStartPosition);
            RefreshStates();
        }

        private int GetSeatInSwipeDirection(Vector2 swipe)
        {
            float minimumDistance = Mathf.Max(1f, minimumSwipeDistance);
            if (swipe.sqrMagnitude < minimumDistance * minimumDistance)
                return -1;
            UIVehicleSeat current = _entries.Find(entry => entry.SeatIndex == _gestureStartingSeat);
            if (current == null)
                return -1;
            Vector2 origin = seatsContainer.InverseTransformPoint(current.RectTransform.position);
            Vector2 direction = swipe.normalized;
            int result = -1;
            float bestAlignment = 0.7071f;
            float bestDistance = float.PositiveInfinity;
            foreach (UIVehicleSeat entry in _entries)
            {
                if (!entry.gameObject.activeSelf || !_character.CanChangeVehicleSeat(entry.SeatIndex, out _))
                    continue;
                Vector2 offset = (Vector2)seatsContainer.InverseTransformPoint(entry.RectTransform.position) - origin;
                float distance = offset.sqrMagnitude;
                if (distance < 0.0001f)
                    continue;
                float alignment = Vector2.Dot(direction, offset.normalized);
                // Prefer the seat aligned with the swipe. Collinear seats choose the
                // closest available one; seats behind or sideways are not candidates.
                if (alignment >= 0.7071f &&
                    (alignment > bestAlignment + 0.0001f ||
                     (Mathf.Abs(alignment - bestAlignment) <= 0.0001f && distance < bestDistance)))
                {
                    result = entry.SeatIndex;
                    bestAlignment = alignment;
                    bestDistance = distance;
                }
            }
            return result;
        }

        internal void EndGesture(PointerEventData eventData, bool select)
        {
            if (eventData.button != PointerEventData.InputButton.Left || _pointerId != eventData.pointerId)
                return;
            if (select)
                UpdateGesture(eventData);
            int seatIndex = _previewSeat;
            bool valid = select && _character != null && ReferenceEquals(_gestureVehicle, _vehicle) &&
                _gestureStartingSeat == _character.PassengingVehicleSeatIndex;
            CancelGesture();
            if (valid)
                SelectSeat(seatIndex);
            RefreshStates();
        }

        private void CancelGesture()
        {
            _pointerId = int.MinValue;
            _previewSeat = -1;
            _gestureVehicle = null;
            _gestureCamera = null;
        }
    }
}
