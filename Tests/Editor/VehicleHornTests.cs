using System;
using System.Collections.Generic;
using System.Reflection;
using LiteNetLib.Utils;
using LiteNetLibManager;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;

namespace MultiplayerARPG.Tests
{
    public class HornTestManager : LiteNetLibGameManager { protected override void OnDestroy() { } }

    public class VehicleHornTests
    {
        private const BindingFlags _private = BindingFlags.Instance | BindingFlags.NonPublic;
        private GameObject _root;
        private HornTestManager _manager;
        private VehicleEntity _vehicle;
        private PlayerCharacterEntity _driver;
        private VehicleHornComponent _horn;
        private Dictionary<byte, BaseGameEntity> _passengers;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Horn tests");
            _root.SetActive(false);
            _manager = _root.AddComponent<HornTestManager>();
            VehicleFuelTests.SetProperty(_manager, "IsServer", true);
            VehicleFuelTests.SetProperty(_manager, "IsClient", true);
            VehicleFuelTests.SetProperty(_manager, "ClientConnectionId", 10L);
            _vehicle = Child("Car").AddComponent<VehicleEntity>();
            _driver = Child("Driver").AddComponent<PlayerCharacterEntity>();
            SetIdentity(_vehicle, 1);
            SetIdentity(_driver, 2);
            _vehicle.CurrentHp = _driver.CurrentHp = 100;
            _vehicle.Seats.Add(new VehicleSeat());
            _vehicle.Seats.Add(new VehicleSeat());
            _passengers = (Dictionary<byte, BaseGameEntity>)typeof(VehicleEntity).GetField("_passengers", _private).GetValue(_vehicle);
            _passengers[0] = _driver;
            _driver.SetPassengingVehicle(0, _vehicle);
            _horn = _vehicle.gameObject.AddComponent<VehicleHornComponent>();
            _horn.OnSetup();
            _horn.OnIdentityInitialize();
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_root);

        [Test]
        public void ServerHornFollowsPressAndReleaseWithoutAnEngineOrFuelTank()
        {
            Assert.That(_horn.IsHornActive, Is.False);
            _horn.ServerSetHorn(true);
            Assert.That(_horn.IsHornActive, Is.True);
            _horn.ServerSetHorn(false);
            Assert.That(_horn.IsHornActive, Is.False);
        }

        [Test]
        public void ClientCannotChangeAuthoritativeState()
        {
            VehicleFuelTests.SetProperty(_manager, "IsServer", false);
            _horn.ServerSetHorn(true);
            Assert.That(_horn.IsHornActive, Is.False);
        }

        [Test]
        public void NoDriverDeadVehicleDeadDriverAndDisabledComponentCannotSound()
        {
            _passengers.Clear();
            _horn.ServerSetHorn(true);
            Assert.That(_horn.IsHornActive, Is.False);
            _passengers[0] = _driver;
            _driver.CurrentHp = 0;
            _horn.ServerSetHorn(true);
            Assert.That(_horn.IsHornActive, Is.False);
            _driver.CurrentHp = 100;
            _vehicle.CurrentHp = 0;
            _horn.ServerSetHorn(true);
            Assert.That(_horn.IsHornActive, Is.False);
            _vehicle.CurrentHp = 100;
            _horn.enabled = false;
            _horn.ServerSetHorn(true);
            Assert.That(_horn.IsHornActive, Is.False);
        }

        [Test]
        public void LocalFeedbackStartsAndReleasesWithoutWaitingForServerState()
        {
            _horn.SetLocalInput(true);
            _horn.SetLocalPresentation(true);
            Assert.That(_horn.LocalInput, Is.True);
            Assert.That(_horn.ShouldPlayAudio, Is.True);
            Assert.That(_horn.IsHornActive, Is.False);
            _horn.ServerSetHorn(true);
            _horn.SetLocalInput(false);
            Assert.That(_horn.ShouldPlayAudio, Is.False); // The old server press cannot restart local audio.
            Assert.That(_horn.IsHornActive, Is.True);
        }

        [Test]
        public void LocalFeedbackExpiresAndOwnershipLossClearsHeldInput()
        {
            _horn.SetLocalInput(true);
            _horn.SetLocalPresentation(true);
            typeof(VehicleHornComponent).GetField("_lastLocalInputTime", _private).SetValue(_horn, float.NegativeInfinity);
            Assert.That(_horn.ShouldPlayAudio, Is.False);
            _horn.ServerSetHorn(true);
            _horn.OnSetOwnerClient(false);
            Assert.That(_horn.LocalInput, Is.False);
            Assert.That(_horn.IsHornActive, Is.False);
            _horn.SetLocalInput(true);
            _horn.OnNetworkDestroy(0);
            Assert.That(_horn.LocalInput || _horn.IsHornActive || _horn.ShouldPlayAudio, Is.False);
        }

        [Test]
        public void NonOwnerCannotSupplyLocalInputAndDedicatedServerDoesNotPlayAudio()
        {
            VehicleFuelTests.SetProperty(_driver.Identity, "ConnectionId", 20L);
            _horn.SetLocalInput(true);
            Assert.That(_horn.LocalInput, Is.False);
            VehicleFuelTests.SetProperty(_manager, "IsClient", false);
            _horn.ServerSetHorn(true);
            Assert.That(_horn.IsHornActive, Is.True);
            Assert.That(_horn.ShouldPlayAudio, Is.False);
        }

        [Test]
        public void ObserversReceivePressAndReleaseAndIgnoreOlderSyncPackets()
        {
            _horn.ServerSetHorn(true);
            var field = (SyncFieldBool)typeof(VehicleHornComponent).GetField("_syncedHorn", _private).GetValue(_horn);
            var writer = new NetDataWriter();
            typeof(SyncFieldBool).GetMethod("WriteSyncData", _private).Invoke(field, new object[] { 10u, false, writer });
            VehicleFuelTests.SetProperty(_manager, "IsServer", false);
            VehicleFuelTests.SetProperty(_vehicle.Identity, "ConnectionId", 20L);
            field.Value = false;
            var read = typeof(SyncFieldBool).GetMethod("ReadSyncData", _private);
            read.Invoke(field, new object[] { 10u, false, new NetDataReader(writer.CopyData()) });
            Assert.That(_horn.ShouldPlayAudio, Is.True);
            var released = new NetDataWriter();
            released.Put(false);
            read.Invoke(field, new object[] { 11u, false, new NetDataReader(released.CopyData()) });
            Assert.That(_horn.ShouldPlayAudio, Is.False);
            read.Invoke(field, new object[] { 10u, false, new NetDataReader(writer.CopyData()) });
            Assert.That(_horn.ShouldPlayAudio, Is.False);
        }

        [Test]
        public void HudHidesForPassengersAndReleasesOnSeatChangeOrExit()
        {
            var ui = CreateUI();
            var bind = typeof(UIVehicleHorn).GetMethod("OnPlayingCharacterChanged", _private);
            try
            {
                bind.Invoke(ui, new object[] { _driver });
                Assert.That(ui.controlsRoot.activeSelf, Is.True);
                ui.SetPressed(true);
                Assert.That(_horn.LocalInput, Is.True);
                _driver.SetPassengingVehicle(1, _vehicle);
                Assert.That(ui.controlsRoot.activeSelf, Is.False);
                Assert.That(_horn.LocalInput, Is.False);
                _driver.SetPassengingVehicle(0, _vehicle);
                Assert.That(ui.controlsRoot.activeSelf, Is.True);
                ui.SetPressed(true);
                _driver.SetPassengingVehicle(0, null);
                Assert.That(ui.controlsRoot.activeSelf || _horn.LocalInput, Is.False);
            }
            finally { bind.Invoke(ui, new object[] { null }); }
        }

        [Test]
        public void HoldButtonIgnoresOtherFingersAndReleasesWhenPointerLeaves()
        {
            var ui = CreateUI();
            var bind = typeof(UIVehicleHorn).GetMethod("OnPlayingCharacterChanged", _private);
            var eventSystem = Child("Events").AddComponent<EventSystem>();
            var handler = ui.buttonHorn.gameObject.AddComponent<UIVehicleHornPressHandler>();
            handler.ui = ui;
            // Enable only the UI. Runtime entity lifecycle is outside this fixture.
            ui.transform.SetParent(null);
            try
            {
                bind.Invoke(ui, new object[] { _driver });
                var first = new PointerEventData(eventSystem) { pointerId = 1, button = PointerEventData.InputButton.Left };
                var second = new PointerEventData(eventSystem) { pointerId = 2, button = PointerEventData.InputButton.Left };
                var right = new PointerEventData(eventSystem) { pointerId = 3, button = PointerEventData.InputButton.Right };
                handler.OnPointerDown(right);
                Assert.That(_horn.LocalInput, Is.False);
                handler.OnPointerDown(first);
                Assert.That(_horn.LocalInput, Is.True);
                handler.OnPointerDown(second);
                handler.OnPointerUp(second);
                Assert.That(_horn.LocalInput, Is.True);
                handler.OnPointerExit(first);
                Assert.That(_horn.LocalInput, Is.False);
                handler.OnPointerDown(first);
                handler.OnPointerUp(first);
                Assert.That(_horn.LocalInput, Is.False);
            }
            finally
            {
                bind.Invoke(ui, new object[] { null });
                Object.DestroyImmediate(ui.gameObject);
            }
        }

        private UIVehicleHorn CreateUI()
        {
            var root = Child("Horn HUD");
            var ui = root.AddComponent<UIVehicleHorn>();
            var controls = Child("Controls");
            controls.transform.SetParent(root.transform, false);
            ui.controlsRoot = controls;
            ui.buttonHorn = controls.AddComponent<UnityEngine.UI.Button>();
            return ui;
        }

        private GameObject Child(string name)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(_root.transform, false);
            return child;
        }

        private void SetIdentity(BaseGameEntity entity, uint id)
        {
            VehicleFuelTests.SetProperty(entity.Identity, "Manager", _manager);
            VehicleFuelTests.SetProperty(entity.Identity, "ConnectionId", 10L);
            VehicleFuelTests.SetProperty(entity.Identity, "ObjectId", id);
        }
    }
}
