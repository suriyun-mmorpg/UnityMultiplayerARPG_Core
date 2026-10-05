using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MultiplayerARPG.Tests
{
    public class SimpleVehicleSeatUIActivatorTests
    {
        private GameObject _root;
        private GameObject _entitiesRoot;
        private SimpleVehicleSeatUIActivator _activator;
        private GameObject _normalControls;
        private GameObject _driverControls;
        private GameObject _passengerControls;
        private GameObject _sharedHud;
        private PlayerCharacterEntity _character;
        private VehicleEntity _vehicle;
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
            _playingCharacterEventField.SetValue(null, null);
            GameInstance.PlayingCharacter = null;
            SetCachedPlayingCharacterEntity(null);
            _root = new GameObject("Simple vehicle UI test");
            _root.SetActive(false);
            _entitiesRoot = CreateChild("Inactive entities");
            _entitiesRoot.SetActive(false);
            _activator = CreateChild("UI observer").AddComponent<SimpleVehicleSeatUIActivator>();
            _normalControls = CreateChild("Normal controls");
            _driverControls = CreateChild("Driver controls");
            _passengerControls = CreateChild("Passenger controls");
            _sharedHud = CreateChild("Fuel speed and exit");
            _character = CreateChild("Player", _entitiesRoot.transform).AddComponent<PlayerCharacterEntity>();
            _vehicle = CreateChild("Vehicle", _entitiesRoot.transform).AddComponent<VehicleEntity>();
            for (int i = 0; i < 3; ++i)
                _vehicle.Seats.Add(new VehicleSeat());
            GameInstance.PlayingCharacter = _character;
            _activator.deactivateObjects = new[] { _normalControls, null };
            _activator.driverActivateObjects = new[] { _driverControls, _sharedHud, null };
            _activator.passengerActivateObjects = new[] { _passengerControls, _sharedHud, null };
            // Enable only the UI; entity lifecycle and networking are outside this fixture.
            _root.SetActive(true);
            InvokeLifecycle("OnEnable");
        }

        [TearDown]
        public void TearDown()
        {
            if (_activator != null && _activator.enabled)
                InvokeLifecycle("OnDisable");
            Object.DestroyImmediate(_root);
            GameInstance.PlayingCharacter = _previousPlayingCharacter;
            SetCachedPlayingCharacterEntity(_previousPlayingCharacterEntity);
            GameInstance.SelectedCharacterId = _previousSelectedCharacterId;
            _playingCharacterEventField.SetValue(null, _previousPlayingCharacterEvent);
        }

        [Test]
        public void PanelsFollowEnteringSeatChangesAndExitingWithoutConditions()
        {
            AssertPanels(false, false);
            Mount(0);
            AssertPanels(true, true);
            Mount(1);
            AssertPanels(true, false);
            Mount(2);
            AssertPanels(true, false);
            Mount(0);
            AssertPanels(true, true);
            _character.SetPassengingVehicle(0, null);
            AssertPanels(false, false);
        }

        [Test]
        public void InvalidSeatAndDespawnedVehicleRestoreDefaultControls()
        {
            Mount(0);
            AssertPanels(true, true);
            Mount(3);
            AssertPanels(false, false);
            Mount(1);
            AssertPanels(true, false);
            Object.DestroyImmediate(_vehicle.gameObject);
            _activator.Refresh();
            AssertPanels(false, false);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EmptyOrNullListsAreSupported(bool nullLists)
        {
            _activator.deactivateObjects = nullLists ? null : new GameObject[0];
            _activator.driverActivateObjects = nullLists ? null : new GameObject[0];
            _activator.passengerActivateObjects = nullLists ? null : new GameObject[0];
            Assert.DoesNotThrow(() => Mount(0));
            Assert.DoesNotThrow(() => Mount(1));
            Assert.DoesNotThrow(() => _character.SetPassengingVehicle(0, null));
        }

        [Test]
        public void ObserverAndAncestorsStayActiveInAllRoles()
        {
            _activator.deactivateObjects = new[] { _normalControls, _root, _activator.gameObject };
            _activator.driverActivateObjects = new[] { _driverControls, _sharedHud, _root, _activator.gameObject };
            _activator.passengerActivateObjects = new[] { _passengerControls, _sharedHud, _root, _activator.gameObject };
            _activator.Refresh();
            Assert.That(_activator.gameObject.activeInHierarchy, Is.True);
            Mount(0);
            AssertPanels(true, true);
            Assert.That(_activator.gameObject.activeInHierarchy, Is.True);
            Mount(1);
            AssertPanels(true, false);
            Assert.That(_activator.gameObject.activeInHierarchy, Is.True);
            _character.SetPassengingVehicle(0, null);
            AssertPanels(false, false);
            Assert.That(_activator.gameObject.activeInHierarchy, Is.True);
        }

        [Test]
        public void LatePlayingCharacterAndCharacterChangesRebindNotifications()
        {
            GameInstance.PlayingCharacter = null;
            Mount(1);
            AssertPanels(false, false);
            GameInstance.PlayingCharacter = _character;
            AssertPanels(true, false);
            PlayerCharacterEntity nextCharacter = CreateChild("Next player", _entitiesRoot.transform)
                .AddComponent<PlayerCharacterEntity>();
            nextCharacter.SetPassengingVehicle(0, _vehicle);
            GameInstance.PlayingCharacter = nextCharacter;
            AssertPanels(true, true);
            Assert.That(CountSubscriptions(_passengingVehicleEventField, _character), Is.Zero);
            Assert.That(CountSubscriptions(_passengingVehicleEventField, nextCharacter), Is.EqualTo(1));
            _character.SetPassengingVehicle(0, null);
            AssertPanels(true, true);
            nextCharacter.SetPassengingVehicle(1, _vehicle);
            AssertPanels(true, false);
            GameInstance.PlayingCharacter = new PlayerCharacterData();
            AssertPanels(false, false);
            Assert.That(CountSubscriptions(_passengingVehicleEventField, nextCharacter), Is.Zero);
        }

        [Test]
        public void DisableRestoresControlsAndEnableReadsTheCurrentSeatWithoutDuplicateListeners()
        {
            Mount(0);
            AssertPanels(true, true);
            for (int i = 0; i < 3; ++i)
            {
                SetActivatorEnabled(false);
                AssertPanels(false, false);
                Assert.That(CountSubscriptions(_playingCharacterEventField, null), Is.Zero);
                Assert.That(CountSubscriptions(_passengingVehicleEventField, _character), Is.Zero);
                Mount(1);
                AssertPanels(false, false);
                SetActivatorEnabled(true);
                AssertPanels(true, false);
                Assert.That(CountSubscriptions(_playingCharacterEventField, null), Is.EqualTo(1));
                Assert.That(CountSubscriptions(_passengingVehicleEventField, _character), Is.EqualTo(1));
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
            // Edit Mode does not dispatch runtime lifecycle callbacks automatically.
            typeof(SimpleVehicleSeatUIActivator).GetMethod(method, _privateInstance).Invoke(_activator, null);
        }

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

        private void AssertPanels(bool mounted, bool driving)
        {
            Assert.That(_normalControls.activeSelf, Is.EqualTo(!mounted));
            Assert.That(_driverControls.activeSelf, Is.EqualTo(driving));
            Assert.That(_passengerControls.activeSelf, Is.EqualTo(mounted && !driving));
            Assert.That(_sharedHud.activeSelf, Is.EqualTo(mounted));
        }
    }
}
