using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MultiplayerARPG.Tests
{
    public class VehicleSeatUIActivatorTests
    {
        private GameObject _root;
        private GameObject _entitiesRoot;
        private VehicleSeatUIActivator _activator;
        private GameObject _normalControls;
        private GameObject _vehicleHud;
        private PlayerCharacterEntity _character;
        private VehicleEntity _vehicle;
        private VehicleType _carType;
        private VehicleType _otherType;
        private IPlayerCharacterData _previousPlayingCharacter;
        private object _previousPlayingCharacterEntity;
        private string _previousSelectedCharacterId;
        private object _previousPlayingCharacterEvent;
        private static readonly BindingFlags _privateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo _playingCharacterEventField = typeof(GameInstance)
            .GetField("OnSetPlayingCharacterEvent", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly PropertyInfo _playingCharacterEntityProperty = typeof(GameInstance)
            .GetProperty("PlayingCharacterEntity", BindingFlags.Static | BindingFlags.Public);
        private static readonly FieldInfo _passengingVehicleEventField = typeof(BaseGameEntity)
            .GetField("onSetPassengingVehicle", _privateInstance);

        [SetUp]
        public void SetUp()
        {
            _previousPlayingCharacter = GameInstance.PlayingCharacter;
            _previousPlayingCharacterEntity = _playingCharacterEntityProperty.GetValue(null);
            _previousSelectedCharacterId = GameInstance.SelectedCharacterId;
            _previousPlayingCharacterEvent = _playingCharacterEventField.GetValue(null);
            // Isolate notifications from UI already open in the Editor, then restore its listeners.
            _playingCharacterEventField.SetValue(null, null);
            GameInstance.PlayingCharacter = null;
            SetCachedPlayingCharacterEntity(null);
            _root = new GameObject("Vehicle UI test");
            _root.SetActive(false);
            _entitiesRoot = CreateChild("Inactive entities");
            _entitiesRoot.SetActive(false);
            _activator = CreateChild("UI observer").AddComponent<VehicleSeatUIActivator>();
            _normalControls = CreateChild("Normal controls");
            _vehicleHud = CreateChild("Car HUD");
            _character = CreateChild("Player", _entitiesRoot.transform).AddComponent<PlayerCharacterEntity>();
            _vehicle = CreateChild("Vehicle", _entitiesRoot.transform).AddComponent<VehicleEntity>();
            _vehicle.Seats.Add(new VehicleSeat());
            _vehicle.Seats.Add(new VehicleSeat());
            _carType = ScriptableObject.CreateInstance<VehicleType>();
            _carType.name = "Vehicle UI test car";
            _otherType = ScriptableObject.CreateInstance<VehicleType>();
            _otherType.name = "Vehicle UI test other";
            SetVehicleType(_carType);
            GameInstance.PlayingCharacter = _character;
            _activator.activateObjects = new[] { _vehicleHud, null };
            _activator.deactivateObjects = new[] { _normalControls, null };
            _activator.conditions = new[] { new VehicleSeatUIActivator.VehicleSeatCondition { vehicleType = _carType, seatIndex = 0 } };
            // Enable only the UI; entity lifecycle and networking are outside this fixture.
            _root.SetActive(true);
            InvokeLifecycle("OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            // Edit Mode does not dispatch these runtime lifecycle callbacks automatically.
            if (_activator != null && _activator.enabled)
                InvokeLifecycle("OnDisable");
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_carType);
            Object.DestroyImmediate(_otherType);
            GameInstance.PlayingCharacter = _previousPlayingCharacter;
            SetCachedPlayingCharacterEntity(_previousPlayingCharacterEntity);
            GameInstance.SelectedCharacterId = _previousSelectedCharacterId;
            _playingCharacterEventField.SetValue(null, _previousPlayingCharacterEvent);
        }

        [Test]
        public void PanelsFollowEventsWhenEnteringChangingSeatsAndExiting()
        {
            AssertPanels(false);
            Mount(0);
            AssertPanels(true);
            Mount(1);
            AssertPanels(false);
            Mount(0);
            AssertPanels(true);
            SetVehicleType(_otherType);
            _activator.Refresh();
            AssertPanels(false);
            SetVehicleType(_carType);
            Mount(2);
            AssertPanels(false);
            _character.SetPassengingVehicle(0, null);
            AssertPanels(false);
        }

        [Test]
        public void ConditionsSupportAnySeatAnyVehicleAndAlternativeRules()
        {
            _activator.conditions = new[]
            {
                new VehicleSeatUIActivator.VehicleSeatCondition { vehicleType = _carType },
                new VehicleSeatUIActivator.VehicleSeatCondition { vehicleType = _otherType, seatIndex = 1 },
                null,
            };
            Mount(1);
            AssertPanels(true);
            SetVehicleType(_otherType);
            _activator.Refresh();
            AssertPanels(true);
            Mount(0);
            AssertPanels(false);
            _activator.conditions = new[] { new VehicleSeatUIActivator.VehicleSeatCondition { seatIndex = 0 } };
            _activator.Refresh();
            AssertPanels(true);
        }

        [Test]
        public void MatchingUsesDataIdForSeparatelyLoadedVehicleTypes()
        {
            VehicleType duplicate = Object.Instantiate(_carType);
            duplicate.name = _carType.name;
            try
            {
                SetVehicleType(duplicate);
                Mount(0);
                AssertPanels(true);
            }
            finally { Object.DestroyImmediate(duplicate); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EmptyRulesMatchAnyVehicleAndMissingContextRestoresNormalControls(bool nullConditions)
        {
            _activator.conditions = nullConditions ? null : new VehicleSeatUIActivator.VehicleSeatCondition[0];
            SetVehicleType(_otherType);
            Mount(1);
            AssertPanels(true);
            GameInstance.PlayingCharacter = null;
            AssertPanels(false);
            GameInstance.PlayingCharacter = _character;
            AssertPanels(true);
            Object.DestroyImmediate(_vehicle.gameObject);
            _activator.Refresh();
            AssertPanels(false);
            _activator.activateObjects = null;
            _activator.deactivateObjects = null;
            Assert.DoesNotThrow(() => _activator.Refresh());
        }

        [Test]
        public void TargetingTheObserverOrItsAncestorsCannotStopNotifications()
        {
            _activator.deactivateObjects = new[] { _normalControls, _activator.gameObject, _root };
            Mount(0);
            AssertPanels(true);
            Assert.That(_activator.gameObject.activeInHierarchy, Is.True);
            Assert.That(_root.activeSelf, Is.True);
            Mount(1);
            AssertPanels(false);
            Assert.That(_activator.gameObject.activeInHierarchy, Is.True);
            Assert.That(_root.activeSelf, Is.True);
        }

        [Test]
        public void EnablingReadsAnAlreadyMountedPlayingCharacter()
        {
            SetActivatorEnabled(false);
            Mount(0);
            AssertPanels(false);
            SetActivatorEnabled(true);
            AssertPanels(true);
            Assert.That(CountSubscriptions(_passengingVehicleEventField, _character), Is.EqualTo(1));
        }

        [Test]
        public void ChangingPlayingCharacterRebindsVehicleNotifications()
        {
            Mount(0);
            AssertPanels(true);
            PlayerCharacterEntity nextCharacter = CreateChild("Next player", _entitiesRoot.transform)
                .AddComponent<PlayerCharacterEntity>();
            GameInstance.PlayingCharacter = nextCharacter;
            AssertPanels(false);
            Assert.That(CountSubscriptions(_passengingVehicleEventField, _character), Is.Zero);
            Assert.That(CountSubscriptions(_passengingVehicleEventField, nextCharacter), Is.EqualTo(1));
            nextCharacter.SetPassengingVehicle(0, _vehicle);
            AssertPanels(true);
            _character.SetPassengingVehicle(0, null);
            AssertPanels(true);
            GameInstance.PlayingCharacter = new PlayerCharacterData();
            AssertPanels(false);
            Assert.That(CountSubscriptions(_passengingVehicleEventField, nextCharacter), Is.Zero);
        }

        [Test]
        public void DisablingUnsubscribesAndReenablingBindsTheCurrentCharacterOnce()
        {
            Mount(0);
            AssertPanels(true);
            SetActivatorEnabled(false);
            AssertPanels(false);
            Assert.That(CountSubscriptions(_playingCharacterEventField, null), Is.Zero);
            Assert.That(CountSubscriptions(_passengingVehicleEventField, _character), Is.Zero);
            PlayerCharacterEntity nextCharacter = CreateChild("Next player", _entitiesRoot.transform)
                .AddComponent<PlayerCharacterEntity>();
            GameInstance.PlayingCharacter = nextCharacter;
            nextCharacter.SetPassengingVehicle(0, _vehicle);
            AssertPanels(false);
            Assert.That(CountSubscriptions(_passengingVehicleEventField, nextCharacter), Is.Zero);
            for (int i = 0; i < 3; ++i)
            {
                SetActivatorEnabled(true);
                AssertPanels(true);
                Assert.That(CountSubscriptions(_playingCharacterEventField, null), Is.EqualTo(1));
                Assert.That(CountSubscriptions(_passengingVehicleEventField, nextCharacter), Is.EqualTo(1));
                SetActivatorEnabled(false);
                AssertPanels(false);
                Assert.That(CountSubscriptions(_playingCharacterEventField, null), Is.Zero);
                Assert.That(CountSubscriptions(_passengingVehicleEventField, nextCharacter), Is.Zero);
            }
        }

        private GameObject CreateChild(string name, Transform parent = null)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(parent != null ? parent : _root.transform);
            return obj;
        }

        private void Mount(byte seatIndex) => _character.SetPassengingVehicle(seatIndex, _vehicle);

        private void SetActivatorEnabled(bool enabled)
        {
            _activator.enabled = enabled;
            InvokeLifecycle(enabled ? "OnEnable" : "OnDisable");
        }

        private void InvokeLifecycle(string method)
        {
            // Exercise runtime entry points while keeping this fixture in Edit Mode.
            typeof(VehicleSeatUIActivator).GetMethod(method, _privateInstance).Invoke(_activator, null);
        }

        private void SetVehicleType(VehicleType type) => typeof(VehicleEntity)
            .GetField("vehicleType", _privateInstance).SetValue(_vehicle, type);

        private static void SetCachedPlayingCharacterEntity(object character)
        {
            // WZM caches this property; UnityMultiplayerARPG derives it from PlayingCharacter.
            if (_playingCharacterEntityProperty.CanWrite)
                _playingCharacterEntityProperty.SetValue(null, character);
        }

        private int CountSubscriptions(FieldInfo eventField, object target)
        {
            var callbacks = eventField.GetValue(target) as System.Delegate;
            if (callbacks == null)
                return 0;
            int count = 0;
            foreach (var callback in callbacks.GetInvocationList())
            {
                if (ReferenceEquals(callback.Target, _activator))
                    ++count;
            }
            return count;
        }

        private void AssertPanels(bool driving)
        {
            // Assert after the notification itself, without manually refreshing the UI.
            Assert.That(_vehicleHud.activeSelf, Is.EqualTo(driving));
            Assert.That(_normalControls.activeSelf, Is.EqualTo(!driving));
        }
    }
}
