using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MultiplayerARPG.Tests
{
    public class VehicleSeatUIActivatorTests
    {
        private GameObject _root;
        private VehicleSeatUIActivator _activator;
        private GameObject _normalControls;
        private GameObject _vehicleHud;
        private PlayerCharacterEntity _character;
        private VehicleEntity _vehicle;
        private VehicleType _carType;
        private VehicleType _otherType;
        private object _previousPlayingCharacter;
        private static readonly BindingFlags _privateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo _playingCharacterField = typeof(GameInstance)
            .GetField("s_playingCharacter", BindingFlags.Static | BindingFlags.NonPublic);

        [SetUp]
        public void SetUp()
        {
            _previousPlayingCharacter = _playingCharacterField.GetValue(null);
            _playingCharacterField.SetValue(null, null);
            _root = new GameObject("Vehicle UI test");
            _root.SetActive(false);
            _activator = CreateChild("UI observer").AddComponent<VehicleSeatUIActivator>();
            _normalControls = CreateChild("Normal controls");
            _vehicleHud = CreateChild("Car HUD");
            _character = CreateChild("Player").AddComponent<PlayerCharacterEntity>();
            _vehicle = CreateChild("Vehicle").AddComponent<VehicleEntity>();
            _vehicle.Seats.Add(new VehicleSeat());
            _vehicle.Seats.Add(new VehicleSeat());
            _carType = ScriptableObject.CreateInstance<VehicleType>();
            _carType.name = "Vehicle UI test car";
            _otherType = ScriptableObject.CreateInstance<VehicleType>();
            _otherType.name = "Vehicle UI test other";
            SetVehicleType(_carType);
            _playingCharacterField.SetValue(null, _character);
            _activator.activateObjects = new[] { _vehicleHud, null };
            _activator.deactivateObjects = new[] { _normalControls, null };
            _activator.conditions = new[] { new VehicleSeatUIActivator.VehicleSeatCondition { vehicleType = _carType, seatIndex = 0 } };
        }

        [TearDown]
        public void TearDown()
        {
            _playingCharacterField.SetValue(null, _previousPlayingCharacter);
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_carType);
            Object.DestroyImmediate(_otherType);
        }

        [Test]
        public void PanelsFollowEnteringChangingSeatsAndExiting()
        {
            AssertPanels(false);
            Mount(0);
            AssertPanels(true);
            Mount(1);
            AssertPanels(false);
            Mount(0);
            AssertPanels(true);
            SetVehicleType(_otherType);
            AssertPanels(false);
            SetVehicleType(_carType);
            Mount(2);
            AssertPanels(false);
            typeof(BaseGameEntity).GetField("_passengingVehicleEntity", _privateInstance).SetValue(_character, null);
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
            AssertPanels(true);
            Mount(0);
            AssertPanels(false);
            _activator.conditions = new[] { new VehicleSeatUIActivator.VehicleSeatCondition { seatIndex = 0 } };
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

        [Test]
        public void EmptyRulesMatchAnyVehicleAndMissingContextRestoresNormalControls()
        {
            _activator.conditions = null;
            SetVehicleType(_otherType);
            Mount(1);
            AssertPanels(true);
            _playingCharacterField.SetValue(null, null);
            AssertPanels(false);
            _playingCharacterField.SetValue(null, _character);
            AssertPanels(true);
            Object.DestroyImmediate(_vehicle.gameObject);
            AssertPanels(false);
            _activator.activateObjects = null;
            _activator.deactivateObjects = null;
            Assert.DoesNotThrow(() => _activator.Refresh());
        }

        [Test]
        public void TargetingTheObserverOrItsAncestorsCannotStopRefresh()
        {
            _activator.deactivateObjects = new[] { _normalControls, _activator.gameObject, _root };
            Mount(0);
            AssertPanels(true);
            Assert.That(_activator.gameObject.activeSelf, Is.True);
            Assert.That(_root.activeSelf, Is.False);
            Mount(1);
            AssertPanels(false);
            Assert.That(_activator.gameObject.activeSelf, Is.True);
            Assert.That(_root.activeSelf, Is.False);
        }

        private GameObject CreateChild(string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(_root.transform);
            return obj;
        }

        private void Mount(byte seatIndex)
        {
            typeof(BaseGameEntity).GetField("_passengingVehicleEntity", _privateInstance).SetValue(_character, _vehicle);
            typeof(BaseGameEntity).GetProperty("PassengingVehicleSeatIndex").SetValue(_character, seatIndex);
        }

        private void SetVehicleType(VehicleType type) => typeof(VehicleEntity)
            .GetField("vehicleType", _privateInstance).SetValue(_vehicle, type);

        private void AssertPanels(bool driving)
        {
            // Exercise the same polling phase used by the live UI.
            typeof(VehicleSeatUIActivator).GetMethod("LateUpdate", _privateInstance).Invoke(_activator, null);
            Assert.That(_vehicleHud.activeSelf, Is.EqualTo(driving));
            Assert.That(_normalControls.activeSelf, Is.EqualTo(!driving));
        }
    }
}
