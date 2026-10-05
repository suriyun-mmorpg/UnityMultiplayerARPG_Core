using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerARPG.Tests
{
    public class VehicleSeatActionTestTarget : MonoBehaviour, IActivatableEntity, IHoldActivatableEntity, IPickupActivatableEntity
    {
        public bool canActivate = true, canHoldActivate = true, canPickup = true;
        public Transform EntityTransform => transform;
        public GameObject EntityGameObject => gameObject;
        public float GetActivatableDistance() => 5f;
        public bool SetAsTargetInOneClick() => true;
        public bool NotBeingSelectedOnClick() => false;
        public bool ShouldClearTargetAfterActivated() => true;
        public bool ShouldBeAttackTarget() => false;
        public bool ShouldNotActivateAfterFollowed() => false;
        public bool CanActivate() => canActivate;
        public bool CanHoldActivate() => canHoldActivate;
        public bool CanPickupActivate() => canPickup;
        public void OnActivate() { }
        public void OnHoldActivate() { }
        public void OnPickupActivate() { }
        public bool ProceedPickingUpAtServer(BaseCharacterEntity characterEntity, out UITextKeys message)
        {
            message = UITextKeys.NONE;
            return true;
        }
    }

    public partial class VehicleSeatActionTests
    {
        private static readonly FieldInfo _characterEvent = typeof(GameInstance)
            .GetField("OnSetPlayingCharacterEvent", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly PropertyInfo _cachedCharacter = typeof(GameInstance)
            .GetProperty("PlayingCharacterEntity", BindingFlags.Public | BindingFlags.Static);
        private static readonly PropertyInfo _singleton = typeof(GameInstance)
            .GetProperty("Singleton", BindingFlags.Public | BindingFlags.Static);
        private GameObject _root;
        private PlayerCharacterEntity _character;
        private VehicleEntity _vehicle;
        private VehicleControllerTestOwner _controller;
        private VehicleSeatActionTestTarget _target;
        private object _previousEvent;
        private IPlayerCharacterData _previousCharacter;
        private object _previousCachedCharacter;
        private string _previousSelectedId;
        private GameInstance _previousInstance;

        [SetUp]
        public void SetUp()
        {
            _previousEvent = _characterEvent.GetValue(null);
            _previousCharacter = GameInstance.PlayingCharacter;
            _previousCachedCharacter = _cachedCharacter.GetValue(null);
            _previousSelectedId = GameInstance.SelectedCharacterId;
            _previousInstance = GameInstance.Singleton;
            _characterEvent.SetValue(null, null);
            _root = new GameObject("Vehicle seat action tests");
            _root.SetActive(false);
            _singleton.SetValue(null, _root.AddComponent<FuelTestGameInstance>());
            _character = Child("Player").AddComponent<PlayerCharacterEntity>();
            _character.CurrentHp = 100;
            GameInstance.PlayingCharacter = _character;
            _vehicle = Child("Vehicle").AddComponent<VehicleEntity>();
            _vehicle.Seats.Add(new VehicleSeat());
            _vehicle.Seats.Add(new VehicleSeat { canActivate = true, canPickup = true });
            _controller = Child("Controller").AddComponent<VehicleControllerTestOwner>();
            _target = Child("Target").AddComponent<VehicleSeatActionTestTarget>();
            typeof(BasePlayerCharacterController).GetProperty("SelectedEntity").SetValue(_controller, _target);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_root);
            _singleton.SetValue(null, _previousInstance);
            GameInstance.PlayingCharacter = _previousCharacter;
            if (_cachedCharacter.CanWrite) _cachedCharacter.SetValue(null, _previousCachedCharacter);
            GameInstance.SelectedCharacterId = _previousSelectedId;
            _characterEvent.SetValue(null, _previousEvent);
        }

        [Test]
        public void NewSeatsDisallowActivationAndPickupByDefault()
        {
            Assert.That(new VehicleSeat().canActivate, Is.False);
            Assert.That(new VehicleSeat().canPickup, Is.False);
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void ActivationAndPickupPermissionsAreIndependent(bool canActivate, bool canPickup)
        {
            _vehicle.Seats[0].canActivate = canActivate;
            _vehicle.Seats[0].canPickup = canPickup;
            _character.SetPassengingVehicle(0, _vehicle);
            Assert.That(_controller.CanActivate(_target), Is.EqualTo(canActivate));
            Assert.That(_controller.CanHoldActivate(_target), Is.EqualTo(canActivate));
            Assert.That(_character.CanPickup(), Is.EqualTo(canPickup));
            Assert.That(_controller.CanPickupActivate(_target), Is.EqualTo(canPickup));
            Assert.That(_controller.ShouldShowActivateButtons(), Is.EqualTo(canActivate));
            Assert.That(_controller.ShouldShowHoldActivateButtons(), Is.EqualTo(canActivate));
            Assert.That(_controller.ShouldShowPickUpButtons(), Is.EqualTo(canPickup));
        }

        [Test]
        public void SeatVehicleAndExitChangesUseTheCurrentPermissions()
        {
            AssertActions(true);
            _character.SetPassengingVehicle(0, _vehicle);
            AssertActions(false);
            _character.SetPassengingVehicle(1, _vehicle);
            AssertActions(true);
            var other = Child("Other vehicle").AddComponent<VehicleEntity>();
            other.Seats.Add(new VehicleSeat());
            _character.SetPassengingVehicle(0, other);
            AssertActions(false);
            _character.SetPassengingVehicle(0, null);
            AssertActions(true);
        }

        [Test]
        public void EnablingSeatActionsStillRequiresValidTargetsAndDistance()
        {
            _character.SetPassengingVehicle(1, _vehicle);
            _target.canActivate = false;
            _target.canHoldActivate = false;
            _target.canPickup = false;
            AssertActions(false, false);
            _target.canActivate = true;
            _target.canHoldActivate = true;
            _target.canPickup = true;
            _target.transform.position = Vector3.forward * 10f;
            AssertActions(false, false);
            Assert.That(_controller.CanActivate(null), Is.False);
            Assert.That(_controller.CanHoldActivate(null), Is.False);
            Assert.That(_controller.CanPickupActivate(null), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void PickupSeatPermissionRetainsPlayerInventoryAndDeathRestrictions(bool updatingItems)
        {
            _character.SetPassengingVehicle(1, _vehicle);
            _character.IsUpdatingItems = updatingItems;
            Assert.That(_character.CanPickup(), Is.EqualTo(!updatingItems));
            Assert.That(_controller.CanPickupActivate(_target), Is.EqualTo(!updatingItems));
            _character.IsUpdatingItems = false;
            _character.CurrentHp = 0;
            Assert.That(_character.CanPickup(), Is.False);
            Assert.That(_controller.CanPickupActivate(_target), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BaseCharacterPickupAlsoRespectsItsSeat(bool canPickup)
        {
            var character = Child("Monster passenger").AddComponent<MonsterCharacterEntity>();
            Assert.That(character.CanPickup(), Is.True);
            _vehicle.Seats[0].canPickup = canPickup;
            character.SetPassengingVehicle(0, _vehicle);
            Assert.That(character.CanPickup(), Is.EqualTo(canPickup));
            character.SetPassengingVehicle(0, null);
            Assert.That(character.CanPickup(), Is.True);
        }

        [Test]
        public void BlockedSeatsRejectAllClientAndServerPickupRequests()
        {
            _character.SetPassengingVehicle(0, _vehicle);
            Assert.That(_character.CallCmdPickup(1), Is.False);
            Assert.That(_character.CallCmdPickupItemFromContainer(1, 0, 1), Is.False);
            Assert.That(_character.CallCmdPickupAllItemsFromContainer(1), Is.False);
            Assert.That(_character.CallCmdPickupNearbyItems(), Is.False);
            // No network manager is attached: a server handler must return before doing any loot work.
            Assert.DoesNotThrow(() => ServerPickup("CmdPickup", (uint)1));
            Assert.DoesNotThrow(() => ServerPickup("CmdPickupItemFromContainer", (uint)1, 0, 1));
            Assert.DoesNotThrow(() => ServerPickup("CmdPickupAllItemsFromContainer", (uint)1));
            Assert.DoesNotThrow(() => ServerPickup("CmdPickupNearbyItems"));
        }

        [Test]
        public void ADespawnedVehicleDoesNotLeaveSeatRestrictionsBehind()
        {
            _character.SetPassengingVehicle(0, _vehicle);
            Object.DestroyImmediate(_vehicle.gameObject);
            AssertActions(true);
        }

        private void AssertActions(bool allowed, bool checkCharacter = true)
        {
            Assert.That(_controller.CanActivate(_target), Is.EqualTo(allowed));
            Assert.That(_controller.CanHoldActivate(_target), Is.EqualTo(allowed));
            Assert.That(_controller.CanPickupActivate(_target), Is.EqualTo(allowed));
            if (checkCharacter) Assert.That(_character.CanPickup(), Is.EqualTo(allowed));
        }

        private void ServerPickup(string method, params object[] args) => typeof(BaseCharacterEntity)
            .GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_character, args);

        private GameObject Child(string name)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(_root.transform, false);
            return obj;
        }
    }
}
