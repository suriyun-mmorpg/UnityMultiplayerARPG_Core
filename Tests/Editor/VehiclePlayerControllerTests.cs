using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace MultiplayerARPG.Tests
{
    public class VehiclePlayerControllerTests
    {
        private GameObject _root;
        private VehicleControllerTestOwner _owner;
        private VehicleControllerTestControls _controls;
        private VehicleType _type;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Vehicle controller test");
            _owner = _root.AddComponent<VehicleControllerTestOwner>();
            _controls = _root.AddComponent<VehicleControllerTestControls>();
            _type = ScriptableObject.CreateInstance<VehicleType>();
            _type.name = "TestVehicle";
            _owner.Configure(_type, _controls, null);
            SetField("exitVehicleButton", string.Empty);
            SetField("cameraRotateButton", string.Empty);
        }

        [TearDown]
        public void TearDown()
        {
            Invoke("Deactivate");
            Object.DestroyImmediate(_root);
            Object.DestroyImmediate(_type);
        }

        [Test]
        public void SelectionUsesSeatAndFallsBackForMissingConfiguration()
        {
            Assert.That(_owner.GetVehicleController(_type, 0), Is.SameAs(_controls));
            Assert.That(_owner.GetVehicleController(_type, 1), Is.Null);
            Assert.That(_owner.GetVehicleController(_type, 255), Is.Null);
            Assert.That(_owner.GetVehicleController(null, 0), Is.Null);
            _owner.Configure(_type, null);
            Assert.That(_owner.GetVehicleController(_type, 0), Is.Null);
        }

        [Test]
        public void SelectionRejectsDisabledAndForeignComponents()
        {
            _controls.enabled = false;
            Assert.That(_owner.GetVehicleController(_type, 0), Is.Null);
            _controls.enabled = true;
            var foreign = new GameObject("Other player");
            try
            {
                _owner.Configure(_type, foreign.AddComponent<VehicleControllerTestControls>());
                Assert.That(_owner.GetVehicleController(_type, 0), Is.Null);
            }
            finally { Object.DestroyImmediate(foreign); }
        }

        [Test]
        public void ActivationBindsSeatAndBlockingReleasesInputInBothPhases()
        {
            Invoke("Activate", _owner, null, null, (byte)2);
            Assert.That(_controls.PlayerController, Is.SameAs(_owner));
            Assert.That(_controls.SeatIndex, Is.EqualTo(2));
            Assert.That(_controls.Activations, Is.EqualTo(1));
            Invoke("TickUpdate", 0.1f);
            Invoke("TickLateUpdate", 0.1f);
            Assert.That(_controls.Updates, Is.EqualTo(1));
            Assert.That(_controls.LateUpdates, Is.EqualTo(1));
            _owner.Blocked = true;
            Invoke("TickUpdate", 0.1f);
            Invoke("TickLateUpdate", 0.1f);
            Assert.That(_controls.Updates, Is.EqualTo(1));
            Assert.That(_controls.LateUpdates, Is.EqualTo(1));
            Assert.That(_controls.Resets, Is.EqualTo(2));
        }

        [Test]
        public void DeactivationRestoresPanelAndClearsContextOnce()
        {
            var panel = new GameObject("Controls panel");
            panel.transform.SetParent(_root.transform);
            panel.SetActive(false);
            SetField("controlsPanel", panel);
            Invoke("Activate", _owner, null, null, (byte)0);
            Assert.That(panel.activeSelf, Is.True);
            Invoke("Deactivate");
            Invoke("Deactivate");
            Assert.That(panel.activeSelf, Is.False);
            Assert.That(_controls.IsActive, Is.False);
            Assert.That(_controls.PlayerController, Is.Null);
            Assert.That(_controls.Resets, Is.EqualTo(1));
            Assert.That(_controls.Deactivations, Is.EqualTo(1));
        }

        [Test]
        public void InputResetClearsHoldHistory()
        {
            var input = new InputStateManager("Activate");
            var flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(InputStateManager).GetField("_holdTime", flags).SetValue(input, 10f);
            typeof(InputStateManager).GetField("_isHolded", flags).SetValue(input, true);
            typeof(InputStateManager).GetField("_isHolding", flags).SetValue(input, true);
            input.Reset();
            input.OnLateUpdate();
            Assert.That(typeof(InputStateManager).GetField("_holdTime", flags).GetValue(input), Is.EqualTo(0f));
            Assert.That(typeof(InputStateManager).GetField("_isHolded", flags).GetValue(input), Is.False);
            Assert.That(input.IsPress || input.IsRelease || input.IsPressed || input.IsHold, Is.False);
        }

        private void SetField(string name, object value) => typeof(BaseVehiclePlayerController)
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(_controls, value);

        private void Invoke(string name, params object[] args) => typeof(BaseVehiclePlayerController)
            .GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(_controls, args);
    }

    public class VehicleControllerTestControls : BaseVehiclePlayerController
    {
        public int Activations, Deactivations, Updates, LateUpdates, Resets;
        protected override void OnActivated() => ++Activations;
        protected override void OnDeactivated() => ++Deactivations;
        protected override void UpdateControls(float deltaTime) => ++Updates;
        protected override void LateUpdateControls(float deltaTime) => ++LateUpdates;
        protected override void ResetInput() => ++Resets;
    }

    public class VehicleControllerTestOwner : BasePlayerCharacterController
    {
        public bool Blocked;
        public void Configure(VehicleType type, params BaseVehiclePlayerController[] controls) =>
            vehicleControllers = new[] { new VehiclePlayerController { vehicleType = type, controllersForEachSeats = controls } };
        public override bool IsVehicleInputBlocked() => Blocked;
        protected override void Awake() { }
        protected override void OnEnable() { }
        protected override void OnDisable() { }
        protected override void OnDestroy() { }
        protected override void UpdateController() { }
        protected override void LateUpdateController() { }
        public override Camera MainCamera => null;
        public override Transform MainCameraTransform => null;
        public override Vector3 AssignedCameraTargetOffset { get; set; }
        public override float AssignedCameraZoomDistance { get; set; }
        public override float AssignedCameraFov { get; set; }
        public override float AssignedCameraNearClipPlane { get; set; }
        public override float AssignedCameraFarClipPlane { get; set; }
        public override float AssignedCameraRotationSpeedScale { get; set; }
        public override bool AssignedEnableWallHitSpring { get; set; }
        public override bool UseHotkey(HotkeyType type, string relateId, AimPosition aimPosition) => false;
    }
}
