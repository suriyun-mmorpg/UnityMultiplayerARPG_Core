using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LiteNetLibManager;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace MultiplayerARPG.Tests
{
    public class SeatChangerTestUI : UIVehicleSeatChanger
    {
        public readonly List<byte> requests = new List<byte>();
        public uint requestedVehicle;
        protected override void SendChangeSeatRequest(uint vehicleObjectId, byte seatIndex)
        {
            requestedVehicle = vehicleObjectId;
            requests.Add(seatIndex);
        }
    }

    public class SeatChangerTestManager : LiteNetLibGameManager
    {
        protected override void OnDestroy() { }
    }

    public class SeatChangerTestInputModule : StandaloneInputModule
    {
        public void InitializeForTest() => base.OnEnable();
        public void Press(PointerEventData pointer, PointerEventData.FramePressState state)
        {
            ProcessMousePress(new MouseButtonEventData { buttonData = pointer, buttonState = state });
        }
        public void Drag(PointerEventData pointer) => ProcessDrag(pointer);
    }

    public class VehicleSeatChangerTests
    {
        private const BindingFlags _private = BindingFlags.Instance | BindingFlags.NonPublic;
        private GameObject _root;
        private Transform _entities;
        private SeatChangerTestManager _manager;
        private VehicleEntity _vehicle;
        private PlayerCharacterEntity _character;
        private SyncListUInt _ids;
        private SeatChangerTestUI _ui;
        private EventSystem _events;
        private object _previousEvent;
        private IPlayerCharacterData _previousCharacter;
        private object _previousCachedCharacter;
        private string _previousSelectedId;
        private static readonly FieldInfo _characterEvent = typeof(GameInstance)
            .GetField("OnSetPlayingCharacterEvent", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly PropertyInfo _cachedCharacter = typeof(GameInstance)
            .GetProperty("PlayingCharacterEntity", BindingFlags.Public | BindingFlags.Static);

        [SetUp]
        public void SetUp()
        {
            _previousEvent = _characterEvent.GetValue(null);
            _previousCharacter = GameInstance.PlayingCharacter;
            _previousCachedCharacter = _cachedCharacter.GetValue(null);
            _previousSelectedId = GameInstance.SelectedCharacterId;
            _characterEvent.SetValue(null, null);
            GameInstance.PlayingCharacter = null;
            _root = new GameObject("Seat changing tests");
            _root.SetActive(false);
            _entities = Child("Entities", _root.transform).transform;
            _entities.gameObject.SetActive(false);
            var managerObject = Child("Manager", _entities);
            managerObject.AddComponent<LiteNetLibAssets>();
            _manager = managerObject.AddComponent<SeatChangerTestManager>();
            SetProperty(_manager, "IsServer", true);
            SetProperty(_manager, "Assets", managerObject.GetComponent<LiteNetLibAssets>());
            SetProperty(_manager.Assets, "Manager", _manager);
            _manager.Assets.manuallyApplyOwnerChanges = true;
            _vehicle = Child("Car", _entities).AddComponent<VehicleEntity>();
            _character = Child("Player", _entities).AddComponent<PlayerCharacterEntity>();
            SetIdentity(_vehicle, 50, 10);
            SetIdentity(_character, 100, 10);
            _ids = (SyncListUInt)typeof(VehicleEntity).GetField("passengerIds", _private).GetValue(_vehicle);
            for (int i = 0; i < 4; ++i)
            {
                _vehicle.Seats.Add(new VehicleSeat());
                _ids.Add(0);
            }
            typeof(LiteNetLibElement).GetMethod("Setup", _private).Invoke(_ids, new object[] { _vehicle, 0 });
            SetProperty(_vehicle.Identity, "IsSpawned", true);
            _ids.onOperation += (op, index, oldItem, newItem) => typeof(VehicleEntity)
                .GetMethod("OnPassengerIdsOperation", _private).Invoke(_vehicle, new object[] { op, index, oldItem, newItem });

            var canvas = Child("Canvas", _root.transform, typeof(UnityEngine.Canvas));
            canvas.GetComponent<UnityEngine.Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            _ui = Child("Observer", canvas.transform).AddComponent<SeatChangerTestUI>();
            _ui.controlsRoot = Child("Controls", _ui.transform);
            _ui.seatsContainer = (RectTransform)Child("Seats", _ui.controlsRoot.transform).transform;
            _ui.seatsContainer.sizeDelta = new Vector2(200, 200);
            var template = Child("Template", _ui.transform).AddComponent<UIVehicleSeat>();
            template.background = template.gameObject.AddComponent<UnityEngine.UI.Image>();
            template.button = template.GetComponent<UnityEngine.UI.Button>();
            template.button.targetGraphic = template.background;
            template.RectTransform.sizeDelta = new Vector2(40, 40);
            template.gameObject.SetActive(false);
            _ui.seatPrefab = template;
            _events = Child("Events", _root.transform).AddComponent<EventSystem>();
            GameInstance.PlayingCharacter = _character;
            _root.SetActive(true);
            Lifecycle("OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            if (_ui != null) Lifecycle("OnDisable");
            if (_vehicle != null) SetProperty(_vehicle.Identity, "IsSpawned", false);
            Object.DestroyImmediate(_root);
            GameInstance.PlayingCharacter = _previousCharacter;
            if (_cachedCharacter.CanWrite) _cachedCharacter.SetValue(null, _previousCachedCharacter);
            GameInstance.SelectedCharacterId = _previousSelectedId;
            _characterEvent.SetValue(null, _previousEvent);
        }

        [Test]
        public void TransfersSeatsWithoutExitingDuplicatingOrMovingThePassenger()
        {
            Mount(0);
            _character.transform.position = new Vector3(4, 5, 6);
            Assert.That(_character.ChangeVehicleSeat(2), Is.True);
            Assert.That(_ids.ToArray(), Is.EqualTo(new uint[] { 0, 0, 100, 0 }));
            Assert.That(_character.PassengingVehicleEntity, Is.SameAs(_vehicle));
            Assert.That(_character.PassengingVehicleSeatIndex, Is.EqualTo(2));
            Assert.That(_character.transform.position, Is.EqualTo(new Vector3(4, 5, 6)));
            Assert.That(_vehicle.GetAllPassengers(), Is.EquivalentTo(new[] { _character }));
            Assert.That(_vehicle.HasDriver, Is.False);
            AssertOwner(-1);
            Assert.That(_character.ChangeVehicleSeat(0), Is.True);
            Assert.That(_vehicle.HasDriver, Is.True);
            AssertOwner(10);
        }

        [TestCase(0)]
        [TestCase(4)]
        [TestCase(255)]
        public void RejectsCurrentOrInvalidSeats(int target)
        {
            Mount(0);
            Assert.That(_character.ChangeVehicleSeat((byte)target), Is.False);
            Assert.That(_ids.ToArray(), Is.EqualTo(new uint[] { 100, 0, 0, 0 }));
        }

        [Test]
        public void RejectsOccupiedSeatsAndPassengersWhoseSourceSeatDoesNotMatch()
        {
            Mount(0);
            var other = Child("Other player", _entities).AddComponent<PlayerCharacterEntity>();
            SetIdentity(other, 200, 20);
            _ids[1] = 200;
            Assert.That(_character.ChangeVehicleSeat(1), Is.False);
            _character.SetPassengingVehicle(2, _vehicle);
            Assert.That(_vehicle.TryChangePassengerSeat(_character, 3), Is.False);
            Assert.That(_ids[1], Is.EqualTo(200));
        }

        [Test]
        public void ClientsCannotMutateSeatsAndUnMountedCharactersCannotSelectSeats()
        {
            Assert.That(_character.CanChangeVehicleSeat(1, out _), Is.False);
            Mount(0);
            SetProperty(_manager, "IsServer", false);
            Assert.That(_character.ChangeVehicleSeat(1), Is.False);
            Assert.That(_vehicle.TryChangePassengerSeat(_character, 1), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PanelAndSeatStatesFollowMountAndOccupancyEvents(bool useTmp)
        {
            var label = Child("Seat number", _ui.seatPrefab.transform);
            var wrapper = label.AddComponent<TextWrapper>();
            if (useTmp)
                wrapper.textMeshText = label.AddComponent<TMPro.TextMeshProUGUI>();
            else
                wrapper.unityText = label.AddComponent<UnityEngine.UI.Text>();
            _ui.seatPrefab.textSeatNumber = wrapper;
            Assert.That(_ui.controlsRoot.activeSelf, Is.False);
            Mount(0);
            Assert.That(_ui.controlsRoot.activeSelf, Is.True);
            Assert.That(Seats().Length, Is.EqualTo(4));
            for (byte i = 0; i < 4; ++i)
            {
                var text = Seat(i).textSeatNumber;
                Assert.That(text.text, Is.EqualTo((i + 1).ToString()));
                Assert.That(useTmp ? text.textMeshText.text : text.unityText.text, Is.EqualTo((i + 1).ToString()));
            }
            Assert.That(Seat(0).background.color, Is.EqualTo(Seat(0).currentColor));
            Assert.That(Seat(1).button.interactable, Is.True);
            var other = Child("Other player", _entities).AddComponent<PlayerCharacterEntity>();
            SetIdentity(other, 200, 20);
            _ids[1] = 200;
            Assert.That(Seat(1).button.interactable, Is.False);
            Assert.That(Seat(1).background.color, Is.EqualTo(Seat(1).occupiedColor));
            _ids[1] = 0;
            Assert.That(Seat(1).button.interactable, Is.True);
            _character.ClearPassengingVehicle();
            Assert.That(_ui.controlsRoot.activeSelf, Is.False);
        }

        [Test]
        public void TapRequestsTheChosenSeatAndWaitsForServerState()
        {
            Mount(0);
            Seat(3).button.onClick.Invoke();
            Assert.That(_ui.requests, Is.EqualTo(new byte[] { 3 }));
            Assert.That(_ui.requestedVehicle, Is.EqualTo(50));
            Assert.That(_character.PassengingVehicleSeatIndex, Is.Zero);
            Assert.That(_ui.SelectSeat(0), Is.False);
            Assert.That(_ui.SelectSeat(256), Is.False);
        }

        [Test]
        public void SwipingFromTheDisabledCurrentSeatSelectsTheAlignedSeatOnce()
        {
            Mount(0);
            var pointer = Pointer(5, Seat(0));
            Seat(0).OnPointerDown(pointer);
            pointer.dragging = true;
            Seat(0).OnBeginDrag(pointer);
            pointer.position = ScreenPoint(Seat(3));
            Seat(0).OnDrag(pointer);
            Assert.That(Seat(3).background.color, Is.EqualTo(Seat(3).previewColor));
            Seat(0).OnPointerUp(pointer);
            Seat(0).OnEndDrag(pointer);
            Seat(0).button.onClick.Invoke();
            Assert.That(_ui.requests, Is.EqualTo(new byte[] { 3 }));
        }

        [Test]
        public void InputModuleDeliversAShortDirectionalSwipeWithoutReachingAnotherSeat()
        {
            Mount(0);
            CursorLockMode previousLock = Cursor.lockState;
            try
            {
                Cursor.lockState = CursorLockMode.None;
                var module = _events.gameObject.AddComponent<SeatChangerTestInputModule>();
                module.InitializeForTest();
                var pointer = Pointer(PointerInputModule.kMouseLeftId, Seat(0));
                pointer.pointerCurrentRaycast = new RaycastResult { gameObject = Seat(0).gameObject, module = _ui.GetComponentInParent<UnityEngine.UI.GraphicRaycaster>() };
                module.Press(pointer, PointerEventData.FramePressState.Pressed);
                Assert.That(pointer.pointerDrag, Is.SameAs(Seat(0).gameObject));
                Vector2 previousPosition = pointer.position;
                pointer.position = SwipePoint(pointer, Vector2.right * 18f);
                pointer.delta = pointer.position - previousPosition;
                module.Drag(pointer);
                Assert.That(pointer.dragging, Is.True);
                module.Press(pointer, PointerEventData.FramePressState.Released);
                Assert.That(_ui.requests, Is.EqualTo(new byte[] { 1 }));
            }
            finally { Cursor.lockState = previousLock; }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RepeatedContextNotificationsDoNotCancelAnOngoingSwipe(bool playingCharacterNotification)
        {
            Mount(0);
            var pointer = Pointer(5, Seat(0));
            Seat(0).OnPointerDown(pointer);
            pointer.dragging = true;
            pointer.position = ScreenPoint(Seat(3));
            Seat(0).OnBeginDrag(pointer);
            if (playingCharacterNotification)
                GameInstance.PlayingCharacter = _character;
            else
                _ids[0] = _character.ObjectId;
            Seat(0).OnEndDrag(pointer);
            Assert.That(_ui.requests, Is.EqualTo(new byte[] { 3 }));
        }

        [TestCase(18f, 0f, 1)]
        [TestCase(0f, -18f, 2)]
        [TestCase(18f, -18f, 3)]
        public void ShortSwipesSelectAvailableSeatsInTheirDirection(float x, float y, int target)
        {
            Mount(0);
            var pointer = Pointer(5, Seat(0));
            Seat(0).OnPointerDown(pointer);
            pointer.position = SwipePoint(pointer, new Vector2(x, y));
            pointer.dragging = true;
            Seat(0).OnDrag(pointer);
            Assert.That(Seat(target).background.color, Is.EqualTo(Seat(target).previewColor));
            Assert.That(_ui.requests, Is.Empty);
            Seat(0).OnEndDrag(pointer);
            Assert.That(_ui.requests, Is.EqualTo(new byte[] { (byte)target }));
            Assert.That(_character.PassengingVehicleSeatIndex, Is.Zero);
        }

        [TestCase(8f, 0f)]
        [TestCase(-18f, 0f)]
        [TestCase(0f, 18f)]
        public void ShortMovementsAndDirectionsWithoutSeatsDoNotSendRequests(float x, float y)
        {
            Mount(0);
            var pointer = Pointer(5, Seat(0));
            Seat(0).OnPointerDown(pointer);
            pointer.position = SwipePoint(pointer, new Vector2(x, y));
            pointer.dragging = true;
            Seat(0).OnEndDrag(pointer);
            Assert.That(_ui.requests, Is.Empty);
        }

        [Test]
        public void DirectionalSwipeCanFinishOutsideThePanel()
        {
            Mount(0);
            var pointer = Pointer(5, Seat(0));
            _ui.OnPointerDown(pointer);
            pointer.position = SwipePoint(pointer, Vector2.right * 1000f);
            pointer.dragging = true;
            _ui.OnEndDrag(pointer);
            Assert.That(_ui.requests, Is.EqualTo(new byte[] { 1 }));
        }

        [Test]
        public void DirectionalSwipeCannotSelectAnOccupiedSeat()
        {
            Mount(0);
            var other = Child("Other player", _entities).AddComponent<PlayerCharacterEntity>();
            SetIdentity(other, 200, 20);
            _ids[1] = 200;
            var pointer = Pointer(5, Seat(0));
            _ui.OnPointerDown(pointer);
            pointer.position = SwipePoint(pointer, Vector2.right * 18f);
            pointer.dragging = true;
            _ui.OnEndDrag(pointer);
            Assert.That(_ui.requests, Is.Empty);
        }

        [TestCase(false, 1)]
        [TestCase(true, 2)]
        public void CollinearSeatsChooseTheClosestAvailableSeat(bool closestOccupied, int target)
        {
            for (int i = 0; i < 4; ++i)
            {
                var seat = Child("Seat", _vehicle.transform).transform;
                seat.localPosition = i == 3 ? new Vector3(1, 0, 2) : new Vector3(0, 0, 2 - i);
                _vehicle.Seats[i].passengingTransform = seat;
            }
            Mount(0);
            if (closestOccupied)
            {
                var other = Child("Other player", _entities).AddComponent<PlayerCharacterEntity>();
                SetIdentity(other, 200, 20);
                _ids[1] = 200;
            }
            var pointer = Pointer(5, Seat(0));
            _ui.OnPointerDown(pointer);
            pointer.position = SwipePoint(pointer, Vector2.down * 18f);
            pointer.dragging = true;
            _ui.OnEndDrag(pointer);
            Assert.That(_ui.requests, Is.EqualTo(new byte[] { (byte)target }));
        }

        [Test]
        public void ReturningSwipeToItsStartingPositionCancels()
        {
            Mount(0);
            var pointer = Pointer(5, Seat(0));
            Seat(0).OnPointerDown(pointer);
            pointer.dragging = true;
            pointer.position = ScreenPoint(Seat(3));
            Seat(0).OnDrag(pointer);
            pointer.position = ScreenPoint(Seat(0));
            Seat(0).OnEndDrag(pointer);
            Assert.That(_ui.requests, Is.Empty);
        }

        [Test]
        public void AnotherTouchAndChangingCharacterCannotCompleteAnOldGesture()
        {
            Mount(0);
            var pointer = Pointer(5, Seat(0));
            Seat(0).OnPointerDown(pointer);
            var otherPointer = Pointer(6, Seat(3));
            _ui.OnEndDrag(otherPointer);
            Assert.That(_ui.requests, Is.Empty);
            GameInstance.PlayingCharacter = null;
            pointer.position = ScreenPoint(Seat(3));
            Seat(0).OnEndDrag(pointer);
            Assert.That(_ui.requests, Is.Empty);
            Assert.That(_ui.controlsRoot.activeSelf, Is.False);
        }

        [Test]
        public void BackgroundSwipesAndReEnablingTheObserverReadCurrentContext()
        {
            Mount(0);
            Lifecycle("OnDisable");
            Assert.That(_ui.controlsRoot.activeSelf, Is.False);
            Lifecycle("OnEnable");
            var pointer = Pointer(5, Seat(0));
            _ui.OnPointerDown(pointer);
            pointer.dragging = true;
            pointer.position = ScreenPoint(Seat(1));
            _ui.OnEndDrag(pointer);
            Assert.That(_ui.requests, Is.EqualTo(new byte[] { 1 }));
        }

        [Test]
        public void LayoutUsesVehicleLocalPositionsAndFallsBackForMissingTransforms()
        {
            Assert.That(VehicleSeatUILayout.GetNormalizedPosition(_vehicle, 0), Is.EqualTo(new Vector2(-0.5f, 0.5f)));
            for (int i = 0; i < 4; ++i)
            {
                var seat = Child("Seat", _vehicle.transform).transform;
                seat.localPosition = new Vector3(i % 2 == 0 ? -1 : 1, 0, i / 2 == 0 ? 1 : -1);
                _vehicle.Seats[i].passengingTransform = seat;
            }
            _vehicle.transform.rotation = Quaternion.Euler(0, 87, 0);
            Assert.That(Vector2.Distance(VehicleSeatUILayout.GetNormalizedPosition(_vehicle, 3), new Vector2(0.5f, -0.5f)), Is.LessThan(0.0001f));
            _vehicle.Seats[1].passengingTransform = null;
            Assert.That(VehicleSeatUILayout.GetNormalizedPosition(_vehicle, 3), Is.EqualTo(new Vector2(0.5f, -0.5f)));
        }

        [Test]
        public void LatePassengerSpawnBindsOnlyTheLatestSeat()
        {
            var spawned = (Dictionary<uint, LiteNetLibIdentity>)typeof(LiteNetLibAssets)
                .GetField("SpawnedObjects", _private).GetValue(_manager.Assets);
            spawned.Remove(_character.ObjectId);
            Mount(0);
            _ids[3] = _character.ObjectId;
            _ids[0] = 0;
            spawned[_character.ObjectId] = _character.Identity;
            _manager.Assets.onObjectSpawn.Invoke(_character.Identity);
            Assert.That(_character.PassengingVehicleSeatIndex, Is.EqualTo(3));
            Assert.That(_vehicle.GetAllPassengers(), Is.EquivalentTo(new[] { _character }));
            Assert.That(_vehicle.HasDriver, Is.False);
        }

        [Test]
        public void ClearingPassengersClearsPendingSpawnBindings()
        {
            var spawned = (Dictionary<uint, LiteNetLibIdentity>)typeof(LiteNetLibAssets)
                .GetField("SpawnedObjects", _private).GetValue(_manager.Assets);
            spawned.Remove(_character.ObjectId);
            Mount(0);
            Assert.DoesNotThrow(() => _ids.Clear());
            var pending = (Dictionary<uint, UnityEngine.Events.UnityAction<LiteNetLibIdentity>>)typeof(VehicleEntity)
                .GetField("_spawnEvents", _private).GetValue(_vehicle);
            Assert.That(pending, Is.Empty);
            Assert.That(_vehicle.GetAllPassengers(), Is.Empty);
        }

        private void Mount(byte seat) => _ids[seat] = _character.ObjectId;
        private UIVehicleSeat[] Seats() => _ui.seatsContainer.GetComponentsInChildren<UIVehicleSeat>(true);
        private UIVehicleSeat Seat(int index) => Seats().Single(seat => seat.SeatIndex == index);
        private void Lifecycle(string method) => typeof(UIVehicleSeatChanger).GetMethod(method, _private).Invoke(_ui, null);
        private PointerEventData Pointer(int id, UIVehicleSeat seat) => new PointerEventData(_events)
            { pointerId = id, position = ScreenPoint(seat), button = PointerEventData.InputButton.Left };
        private Vector2 SwipePoint(PointerEventData pointer, Vector2 localOffset)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_ui.seatsContainer,
                pointer.position, null, out Vector2 start);
            return RectTransformUtility.WorldToScreenPoint(null, _ui.seatsContainer.TransformPoint(start + localOffset));
        }
        private static Vector2 ScreenPoint(UIVehicleSeat seat)
        {
            UnityEngine.Canvas.ForceUpdateCanvases();
            return RectTransformUtility.WorldToScreenPoint(null, seat.RectTransform.position);
        }
        private void SetIdentity(BaseGameEntity entity, uint id, long owner)
        {
            SetProperty(entity.Identity, "Manager", _manager);
            SetProperty(entity.Identity, "ObjectId", id);
            SetProperty(entity.Identity, "ConnectionId", owner);
            var spawned = (Dictionary<uint, LiteNetLibIdentity>)typeof(LiteNetLibAssets).GetField("SpawnedObjects", _private).GetValue(_manager.Assets);
            spawned[id] = entity.Identity;
        }
        private void AssertOwner(long owner)
        {
            var owners = (Dictionary<uint, long>)typeof(LiteNetLibAssets).GetField("ChangingOwnerObjects", _private).GetValue(_manager.Assets);
            Assert.That(owners[_vehicle.ObjectId], Is.EqualTo(owner));
        }
        private static void SetProperty(object target, string name, object value)
        {
            for (Type type = target.GetType(); type != null; type = type.BaseType)
            {
                var property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                if (property == null) continue;
                property.SetValue(target, value);
                return;
            }
            throw new MissingMemberException(target.GetType().Name, name);
        }
        private static GameObject Child(string name, Transform parent, params Type[] components)
        {
            var obj = new GameObject(name, new[] { typeof(RectTransform) }.Concat(components).ToArray());
            obj.transform.SetParent(parent, false);
            return obj;
        }
    }
}
