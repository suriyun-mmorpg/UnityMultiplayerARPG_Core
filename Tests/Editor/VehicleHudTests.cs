using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MultiplayerARPG.Tests
{
    public class VehicleHudTestVehicle : VehicleEntity, IVehicleEntity
    {
        public int maximumHp = 200;
        public float speed;
        public override int MaxHp => maximumHp;
        public new float CurrentMoveSpeed => speed;
    }

    public class VehicleHudTests
    {
        private const BindingFlags _private = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly FieldInfo _characterEvent = typeof(GameInstance)
            .GetField("OnSetPlayingCharacterEvent", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly PropertyInfo _cachedCharacter = typeof(GameInstance)
            .GetProperty("PlayingCharacterEntity", BindingFlags.Public | BindingFlags.Static);
        private GameObject _root;
        private FuelTestManager _manager;
        private PlayerCharacterEntity _character;
        private VehicleHudTestVehicle _vehicle;
        private UIVehicleHp _hp;
        private UIVehicleSpeed _speed;
        private object _previousEvent;
        private IPlayerCharacterData _previousCharacter;
        private object _previousCachedCharacter;
        private string _previousSelectedId;

        [SetUp]
        public void SetUp()
        {
            _previousEvent = _characterEvent.GetValue(null);
            _previousCharacter = GameInstance.PlayingCharacter;
            _previousCachedCharacter = _cachedCharacter.GetValue(null);
            _previousSelectedId = GameInstance.SelectedCharacterId;
            _characterEvent.SetValue(null, null);
            GameInstance.PlayingCharacter = null;
            _root = new GameObject("Vehicle HUD tests");
            _root.SetActive(false);
            _manager = _root.AddComponent<FuelTestManager>();
            VehicleFuelTests.SetProperty(_manager, "IsServer", true);
            _character = Child("Player").AddComponent<PlayerCharacterEntity>();
            _vehicle = NewVehicle("Car", 150, 10f);
            _hp = Child("HP observer").AddComponent<UIVehicleHp>();
            _hp.controlsRoot = Child("HP controls", _hp.transform);
            _hp.hpFill = _hp.controlsRoot.AddComponent<UnityEngine.UI.Image>();
            _hp.hpFill.type = UnityEngine.UI.Image.Type.Filled;
            _speed = Child("Speed observer").AddComponent<UIVehicleSpeed>();
            _speed.controlsRoot = Child("Speed controls", _speed.transform);
            Lifecycle(_hp, "OnEnable");
            Lifecycle(_speed, "OnEnable");
            GameInstance.PlayingCharacter = _character;
        }

        [TearDown]
        public void TearDown()
        {
            Lifecycle(_hp, "OnDisable");
            Lifecycle(_speed, "OnDisable");
            Object.DestroyImmediate(_root);
            GameInstance.PlayingCharacter = _previousCharacter;
            if (_cachedCharacter.CanWrite) _cachedCharacter.SetValue(null, _previousCachedCharacter);
            GameInstance.SelectedCharacterId = _previousSelectedId;
            _characterEvent.SetValue(null, _previousEvent);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MountSeatVehicleAndCharacterChangesUpdateBothLabels(bool useTmp)
        {
            AddLabels(useTmp);
            Assert.That(_hp.controlsRoot.activeSelf, Is.False);
            Assert.That(_speed.controlsRoot.activeSelf, Is.False);
            _character.SetPassengingVehicle(0, _vehicle);
            AssertLabels("HP 150/200", "36 km/h", useTmp);
            Assert.That(_hp.hpFill.fillAmount, Is.EqualTo(0.75f));
            _character.SetPassengingVehicle(1, _vehicle);
            Assert.That(_hp.controlsRoot.activeSelf, Is.True);
            Assert.That(_speed.controlsRoot.activeSelf, Is.True);
            var other = NewVehicle("Other car", 50, 5f);
            _character.SetPassengingVehicle(1, other);
            AssertLabels("HP 50/200", "18 km/h", useTmp);
            PublishHp(_vehicle, 0);
            Assert.That(_hp.textHp.text, Is.EqualTo("HP 50/200"));
            GameInstance.PlayingCharacter = Child("Other player").AddComponent<PlayerCharacterEntity>();
            Assert.That(_hp.controlsRoot.activeSelf, Is.False);
            Assert.That(_speed.controlsRoot.activeSelf, Is.False);
            _character.SetPassengingVehicle(0, _vehicle);
            Assert.That(_hp.controlsRoot.activeSelf, Is.False);
            GameInstance.PlayingCharacter = _character;
            AssertLabels("HP 0/200", "36 km/h", useTmp);
            _character.SetPassengingVehicle(0, null);
            Assert.That(_hp.controlsRoot.activeSelf, Is.False);
            Assert.That(_speed.controlsRoot.activeSelf, Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DamageUpdatesImmediatelyAndMaximumHpChangesStayFinite(bool useTmp)
        {
            AddLabels(useTmp);
            _character.SetPassengingVehicle(0, _vehicle);
            PublishHp(_vehicle, 20);
            Assert.That(_hp.textHp.text, Is.EqualTo("HP 20/200"));
            Assert.That(_hp.hpFill.fillAmount, Is.EqualTo(0.1f).Within(0.0001f));
            PublishHp(_vehicle, -10);
            Assert.That(_hp.textHp.text, Is.EqualTo("HP 0/200"));
            Assert.That(_hp.hpFill.fillAmount, Is.Zero);
            PublishHp(_vehicle, 500);
            Assert.That(_hp.hpFill.fillAmount, Is.EqualTo(1f));
            _vehicle.maximumHp = 0;
            Lifecycle(_hp, "Update");
            Assert.That(_hp.textHp.text, Is.EqualTo("HP 0/0"));
            Assert.That(_hp.hpFill.fillAmount, Is.Zero);
            Lifecycle(_hp, "OnDisable");
            _hp.textHp.text = "disabled";
            PublishHp(_vehicle, 10);
            Assert.That(_hp.textHp.text, Is.EqualTo("disabled"));
            Assert.That(_hp.controlsRoot.activeSelf, Is.False);
            Lifecycle(_hp, "OnEnable");
            Assert.That(_hp.controlsRoot.activeSelf, Is.True);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SpeedUpdatesAtRestAndSupportsOtherUnitsAndMissingMovement(bool useTmp)
        {
            AddLabels(useTmp);
            _character.SetPassengingVehicle(0, _vehicle);
            // The movement interface supplies this value, including on remote clients.
            _vehicle.speed = 20f;
            Lifecycle(_speed, "Update");
            Assert.That(_speed.textSpeed.text, Is.EqualTo("72 km/h"));
            _vehicle.speed = 0f;
            Lifecycle(_speed, "Update");
            Assert.That(_speed.textSpeed.text, Is.EqualTo("0 km/h"));
            _vehicle.speed = 10f;
            _speed.speedMultiplier = 2.236936f;
            _speed.textFormat = "{0:0} mph";
            _speed.Refresh();
            Assert.That(_speed.textSpeed.text, Is.EqualTo("22 mph"));
            _speed.speedMultiplier = 1f;
            _speed.textFormat = "{0:0} m/s";
            _character.SetPassengingVehicle(0, Child("Vehicle without movement").AddComponent<VehicleEntity>());
            Assert.That(_speed.textSpeed.text, Is.EqualTo("0 m/s"));
        }

        [Test]
        public void DespawningAndOptionalBindingsAreSafe()
        {
            _character.SetPassengingVehicle(1, _vehicle);
            Assert.DoesNotThrow(() => { _hp.Refresh(); _speed.Refresh(); });
            Object.DestroyImmediate(_vehicle.gameObject);
            Assert.DoesNotThrow(() => { _hp.Refresh(); _speed.Refresh(); });
            Assert.That(_hp.controlsRoot.activeSelf, Is.False);
            Assert.That(_speed.controlsRoot.activeSelf, Is.False);
        }

        [Test]
        public void AControlsRootContainingTheObserverCannotDisableIt()
        {
            _hp.controlsRoot = _hp.gameObject;
            _speed.controlsRoot = _speed.gameObject;
            _hp.Refresh();
            _speed.Refresh();
            Assert.That(_hp.gameObject.activeSelf, Is.True);
            Assert.That(_speed.gameObject.activeSelf, Is.True);
        }

        private VehicleHudTestVehicle NewVehicle(string name, int hp, float speed)
        {
            var vehicle = Child(name).AddComponent<VehicleHudTestVehicle>();
            VehicleFuelTests.SetProperty(vehicle.Identity, "Manager", _manager);
            vehicle.CurrentHp = hp;
            vehicle.speed = speed;
            return vehicle;
        }

        private void AddLabels(bool useTmp)
        {
            _hp.textHp = Label(_hp.controlsRoot.transform, useTmp);
            _speed.textSpeed = Label(_speed.controlsRoot.transform, useTmp);
        }

        private TextWrapper Label(Transform parent, bool useTmp)
        {
            var label = Child("Label", parent).AddComponent<TextWrapper>();
            if (useTmp) label.textMeshText = label.gameObject.AddComponent<TMPro.TextMeshProUGUI>();
            else label.unityText = label.gameObject.AddComponent<UnityEngine.UI.Text>();
            return label;
        }

        private void AssertLabels(string hp, string speed, bool useTmp)
        {
            Assert.That(_hp.textHp.text, Is.EqualTo(hp));
            Assert.That(_speed.textSpeed.text, Is.EqualTo(speed));
            Assert.That(useTmp ? _hp.textHp.textMeshText.text : _hp.textHp.unityText.text, Is.EqualTo(hp));
            Assert.That(useTmp ? _speed.textSpeed.textMeshText.text : _speed.textSpeed.unityText.text, Is.EqualTo(speed));
        }

        private static void PublishHp(VehicleEntity vehicle, int hp)
        {
            int oldHp = vehicle.CurrentHp;
            vehicle.CurrentHp = hp;
            typeof(DamageableEntity).GetMethod("OnCurrentHpChange", _private)
                .Invoke(vehicle, new object[] { false, oldHp, hp });
        }

        private GameObject Child(string name, Transform parent = null)
        {
            var child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent != null ? parent : _root.transform, false);
            return child;
        }

        private static void Lifecycle(MonoBehaviour component, string method) => component.GetType()
            .GetMethod(method, _private).Invoke(component, null);
    }
}
