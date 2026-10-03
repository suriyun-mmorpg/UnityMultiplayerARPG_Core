using System;
using System.Reflection;
using LiteNetLibManager;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerARPG.Tests
{
    public class FuelTestManager : LiteNetLibGameManager { protected override void OnDestroy() { } }

    public class FuelTestGameInstance : GameInstance { protected override void OnDestroy() { } }

    public class VehicleFuelTests
    {
        private GameObject _root;
        private FuelTestManager _manager;
        private VehicleEntity _vehicle;
        private VehicleFuelComponent _fuel;
        private PlayerCharacterEntity _character;
        private JunkItem _can;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("Fuel tests");
            _root.SetActive(false);
            _manager = _root.AddComponent<FuelTestManager>();
            SetProperty(_manager, "IsServer", true);
            var car = new GameObject("Vehicle");
            car.transform.SetParent(_root.transform, false);
            _vehicle = car.AddComponent<VehicleEntity>();
            SetProperty(_vehicle.Identity, "Manager", _manager);
            _vehicle.CurrentHp = 100;
            _fuel = car.AddComponent<VehicleFuelComponent>();
            SetField(_fuel, "_capacity", 10f);
            SetField(_fuel, "_startingFuel", 5f);
            SetField(_fuel, "_idleConsumption", 1f);
            SetField(_fuel, "_throttleConsumption", 3f);
            SetField(_fuel, "_syncInterval", 0.5f);
            _fuel.OnIdentityInitialize();
            var player = new GameObject("Player");
            player.transform.SetParent(_root.transform, false);
            _character = player.AddComponent<PlayerCharacterEntity>();
            SetProperty(_character.Identity, "Manager", _manager);
            _character.CurrentHp = 100;
            _can = ScriptableObject.CreateInstance<JunkItem>();
            _can.Id = "fuel_test_can";
            SetField(_fuel, "_fuelItems", new[] { new VehicleFuelItemAmount { item = _can, fuelAmount = 2f } });
            _character.NonEquipItems.Add(new CharacterItem { dataId = _can.DataId, amount = 3, level = 1 });
        }

        [TearDown]
        public void TearDown() { Object.DestroyImmediate(_root); Object.DestroyImmediate(_can); }

        [TestCase(0f, 4f)]
        [TestCase(0.5f, 2.5f)]
        [TestCase(1f, 1f)]
        [TestCase(2f, 1f)]
        public void ConsumptionUsesMinutesAndClampedThrottle(float throttle, float expected)
        {
            _fuel.ServerConsumeFuel(true, throttle, 60f);
            Assert.That(_fuel.RemainingFuel, Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void EmptyTankClampsAtZeroAndRefillingPublishesImmediately()
        {
            _fuel.ServerConsumeFuel(true, 1f, 600f);
            Assert.That(_fuel.IsEmpty, Is.True);
            Assert.That(_fuel.RemainingFuel, Is.Zero);
            Assert.That(_fuel.ServerRefuel(20f, out float accepted), Is.True);
            Assert.That(accepted, Is.EqualTo(10f));
            Assert.That(_fuel.RemainingFuel, Is.EqualTo(10f));
            Assert.That(_fuel.IsEmpty, Is.False);
            Assert.That(SyncedFuel(), Is.EqualTo(10f));
        }

        [Test]
        public void EngineOffAndDisabledFuelDoNotConsume()
        {
            _fuel.ServerConsumeFuel(false, 1f, 60f);
            Assert.That(_fuel.RemainingFuel, Is.EqualTo(5f));
            _fuel.enabled = false;
            _fuel.ServerConsumeFuel(true, 1f, 60f);
            Assert.That(_fuel.RemainingFuel, Is.EqualTo(5f));
            Assert.That(_fuel.UsesFuel, Is.False);
        }

        [Test]
        public void FuelUpdatesAreBatchedButEmptyAndEngineOffFlushImmediately()
        {
            _fuel.ServerConsumeFuel(true, 1f, 0.1f);
            Assert.That(SyncedFuel(), Is.EqualTo(5f));
            _fuel.ServerConsumeFuel(false, 0f, 0.1f);
            Assert.That(SyncedFuel(), Is.EqualTo(_fuel.RemainingFuel));
            _fuel.ServerConsumeFuel(true, 1f, 0.5f);
            Assert.That(SyncedFuel(), Is.EqualTo(_fuel.RemainingFuel));
            _fuel.ServerConsumeFuel(true, 1f, 600f);
            Assert.That(SyncedFuel(), Is.Zero);
        }

        [Test]
        public void ClientsCannotConsumeRefillRestoreOrSpendInventory()
        {
            SetProperty(_manager, "IsServer", false);
            _fuel.ServerConsumeFuel(true, 1f, 60f);
            Assert.That(_fuel.ServerRefuel(2f, out _), Is.False);
            Assert.That(_fuel.ServerRestoreFuel(0f), Is.False);
            Assert.That(_fuel.TryRefuelFromItem(_character, 0), Is.False);
            Assert.That(_fuel.RemainingFuel, Is.EqualTo(5f));
            Assert.That(_character.NonEquipItems[0].amount, Is.EqualTo(3));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(-1f)]
        [TestCase(0f)]
        public void InvalidRefillsLeaveFuelUnchanged(float amount)
        {
            Assert.That(_fuel.ServerRefuel(amount, out float accepted), Is.False);
            Assert.That(accepted, Is.Zero);
            Assert.That(_fuel.RemainingFuel, Is.EqualTo(5f));
        }

        [Test]
        public void SavedFuelRestoresAndSeatOrOwnerChangesDoNotResetIt()
        {
            Assert.That(_fuel.ServerRestoreFuel(2f), Is.True);
            _character.SetPassengingVehicle(0, _vehicle);
            _character.SetPassengingVehicle(1, _vehicle);
            _fuel.OnSetOwnerClient(true);
            Assert.That(_fuel.RemainingFuel, Is.EqualTo(2f));
            Assert.That(_fuel.ServerRestoreFuel(50f), Is.True);
            Assert.That(_fuel.RemainingFuel, Is.EqualTo(10f));
        }

        [Test]
        public void FuelCansRequireCompatibilityDistanceAndSpaceForTheWholeCan()
        {
            Assert.That(_fuel.FindRefuelItemIndex(_character), Is.Zero);
            Assert.That(_fuel.CanRefuelFromItem(_character, 0, out float amount), Is.True);
            Assert.That(amount, Is.EqualTo(2f));
            _character.transform.position = Vector3.right * 10f;
            Assert.That(_fuel.TryRefuelFromItem(_character, 0), Is.False);
            _character.transform.position = Vector3.zero;
            _fuel.ServerRestoreFuel(9f);
            Assert.That(_fuel.TryRefuelFromItem(_character, 0), Is.False);
            _fuel.ServerRestoreFuel(5f);
            _character.NonEquipItems[0] = new CharacterItem { dataId = _can.DataId + 1, amount = 3 };
            Assert.That(_fuel.TryRefuelFromItem(_character, 0), Is.False);
            Assert.That(_character.NonEquipItems[0].amount, Is.EqualTo(3));
            Assert.That(_fuel.RemainingFuel, Is.EqualTo(5f));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RefillingConsumesExactlyOneCanAndHandlesTheLastInventorySlot(bool limitedSlots)
        {
            var previousInstance = GameInstance.Singleton;
            bool hadItem = GameInstance.Items.TryGetValue(_can.DataId, out BaseItem previousItem);
            var holder = new GameObject("Inventory services");
            holder.transform.SetParent(_root.transform, false);
            var instance = holder.AddComponent<FuelTestGameInstance>();
            var inventory = ScriptableObject.CreateInstance<DefaultInventoryManager>();
            var singleton = typeof(GameInstance).GetProperty("Singleton", BindingFlags.Public | BindingFlags.Static);
            try
            {
                typeof(GameInstance).GetField("inventoryManager", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(instance, inventory);
                instance.inventorySystem = limitedSlots ? InventorySystem.LimitSlots : InventorySystem.Simple;
                singleton.SetValue(null, instance);
                GameInstance.Items[_can.DataId] = _can;
                _character.NonEquipItems[0] = new CharacterItem { id = Guid.NewGuid().ToString("N"), dataId = _can.DataId, amount = 3, level = 1 };
                Assert.That(_fuel.TryRefuelFromItem(_character, 0), Is.True);
                Assert.That(_character.NonEquipItems[0].amount, Is.EqualTo(2));
                Assert.That(_fuel.RemainingFuel, Is.EqualTo(7f));
                Assert.That(SyncedFuel(), Is.EqualTo(7f));
                Assert.That(_fuel.TryRefuelFromItem(_character, 0), Is.True);
                Assert.That(_character.NonEquipItems[0].amount, Is.EqualTo(1));
                Assert.That(_fuel.RemainingFuel, Is.EqualTo(9f));
                Assert.That(_fuel.TryRefuelFromItem(_character, 0), Is.False);
                Assert.That(_character.NonEquipItems[0].amount, Is.EqualTo(1));
                _fuel.ServerRestoreFuel(0f);
                Assert.That(_fuel.TryRefuelFromItem(_character, 0), Is.True);
                Assert.That(_fuel.RemainingFuel, Is.EqualTo(2f));
                Assert.That(_character.NonEquipItems.Count, Is.EqualTo(limitedSlots ? 1 : 0));
                if (limitedSlots) Assert.That(_character.NonEquipItems[0].amount, Is.Zero);
            }
            finally
            {
                singleton.SetValue(null, previousInstance);
                if (hadItem) GameInstance.Items[_can.DataId] = previousItem;
                else GameInstance.Items.Remove(_can.DataId);
                Object.DestroyImmediate(inventory);
            }
        }

        [Test]
        public void MountedHudTracksFuelSeatChangesRefillingAndExiting()
        {
            var observer = new GameObject("Fuel HUD", typeof(RectTransform));
            observer.transform.SetParent(_root.transform, false);
            var ui = observer.AddComponent<UIVehicleFuel>();
            var controls = new GameObject("Controls", typeof(RectTransform));
            controls.transform.SetParent(observer.transform, false);
            ui.controlsRoot = controls;
            ui.textFuel = controls.AddComponent<TMPro.TextMeshProUGUI>();
            var bar = new GameObject("Fill", typeof(RectTransform));
            bar.transform.SetParent(controls.transform, false);
            ui.fuelFill = bar.AddComponent<UnityEngine.UI.Image>();
            ui.fuelFill.type = UnityEngine.UI.Image.Type.Filled;
            ui.buttonRefuel = bar.AddComponent<UnityEngine.UI.Button>();
            var bind = typeof(UIVehicleFuel).GetMethod("OnPlayingCharacterChanged", BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                bind.Invoke(ui, new object[] { _character });
                Assert.That(controls.activeSelf, Is.False);
                _character.SetPassengingVehicle(0, _vehicle);
                Assert.That(controls.activeSelf, Is.True);
                Assert.That(ui.fuelFill.fillAmount, Is.EqualTo(0.5f));
                Assert.That(ui.buttonRefuel.interactable, Is.True);
                _character.SetPassengingVehicle(1, _vehicle);
                Assert.That(controls.activeSelf, Is.True);
                _fuel.ServerRestoreFuel(0f);
                Assert.That(ui.textFuel.text, Is.EqualTo("FUEL EMPTY"));
                Assert.That(ui.fuelFill.color, Is.EqualTo(ui.emptyColor));
                _fuel.ServerRefuel(10f, out _);
                Assert.That(ui.fuelFill.fillAmount, Is.EqualTo(1f));
                Assert.That(ui.buttonRefuel.interactable, Is.False);
                _character.SetPassengingVehicle(0, null);
                Assert.That(controls.activeSelf, Is.False);
            }
            finally { bind.Invoke(ui, new object[] { null }); }
        }
        private float SyncedFuel() => ((SyncFieldFloat)typeof(VehicleFuelComponent)
            .GetField("_syncedFuel", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(_fuel)).Value;
        private static void SetField(object target, string name, object value) => target.GetType()
            .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        internal static void SetProperty(object target, string name, object value)
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
    }
}
