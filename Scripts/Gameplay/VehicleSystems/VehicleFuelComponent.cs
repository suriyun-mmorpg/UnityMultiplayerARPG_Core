using System;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    [Serializable]
    public struct VehicleFuelItemAmount
    {
        public BaseItem item;
        [Min(0f)] public float fuelAmount;
    }

    /// <summary>Optional fuel tank. Only the server consumes or transfers fuel.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(VehicleEntity))]
    public class VehicleFuelComponent : BaseNetworkedGameEntityComponent<VehicleEntity>
    {
        [SerializeField, Min(0f)] private float _capacity = 50f;
        [SerializeField, Min(0f)] private float _startingFuel = 50f;
        [Tooltip("Litres per minute while the engine is running, including when parked.")]
        [SerializeField, Min(0f)] private float _idleConsumption = 0.1f;
        [Tooltip("Additional litres per minute at full throttle.")]
        [SerializeField, Min(0f)] private float _throttleConsumption = 1.5f;
        [SerializeField, Min(0.05f)] private float _syncInterval = 0.5f;
        [SerializeField, Min(0f)] private float _refuelDistance = 4f;
        [SerializeField, Min(0f)] private float _maximumRefuelSpeed = 0.5f;
        [SerializeField] private VehicleFuelItemAmount[] _fuelItems = Array.Empty<VehicleFuelItemAmount>();
        [SerializeField] private SyncFieldFloat _syncedFuel = new SyncFieldFloat();

        private float _remainingFuel;
        private float _publishElapsed;
        private bool _initialized;
        public event Action onFuelChanged;
        public float Capacity => NonNegative(_capacity);
        public bool UsesFuel => enabled && Capacity > 0f;
        public bool IsInitialized => _initialized;
        public float RemainingFuel => Mathf.Clamp(IsServer ? _remainingFuel : _syncedFuel.Value, 0f, Capacity);
        public float FuelFraction => Capacity > 0f ? RemainingFuel / Capacity : 1f;
        public bool IsEmpty => UsesFuel && _initialized && RemainingFuel <= 0f;

        public override void OnSetup()
        {
            base.OnSetup();
            _syncedFuel.syncMode = LiteNetLibSyncFieldMode.ServerToClients;
            _syncedFuel.onChange -= OnSyncedFuelChanged;
            _syncedFuel.onChange += OnSyncedFuelChanged;
        }

        public override void OnIdentityInitialize()
        {
            base.OnIdentityInitialize();
            _initialized = true;
            _publishElapsed = 0f;
            if (IsServer)
            {
                _remainingFuel = Mathf.Min(Capacity, NonNegative(_startingFuel));
                PublishFuel();
            }
            onFuelChanged?.Invoke();
        }

        public override void OnNetworkDestroy(byte reasons)
        {
            _initialized = false;
            base.OnNetworkDestroy(reasons);
            onFuelChanged?.Invoke();
        }

        protected override void OnDestroy()
        {
            _syncedFuel.onChange -= OnSyncedFuelChanged;
            onFuelChanged = null;
            base.OnDestroy();
        }

        private void OnSyncedFuelChanged(bool initial, float oldValue, float newValue)
        {
            if (!IsServer)
                onFuelChanged?.Invoke();
        }

        /// <summary>Call from the server movement adapter using its simulated engine state.</summary>
        public void ServerConsumeFuel(bool engineRunning, float throttle, float deltaTime)
        {
            if (!IsServer || !_initialized || !UsesFuel ||
                !Finite(throttle) || !Finite(deltaTime) || deltaTime <= 0f)
                return;
            if (!engineRunning || IsEmpty)
            {
                if (_syncedFuel.Value != _remainingFuel)
                    PublishFuel();
                return;
            }
            float rate = NonNegative(_idleConsumption) + Mathf.Clamp01(throttle) * NonNegative(_throttleConsumption);
            float previous = _remainingFuel;
            _remainingFuel = Mathf.Max(0f, previous - rate * deltaTime / 60f);
            _publishElapsed += deltaTime;
            if (_remainingFuel <= 0f || _publishElapsed >= Mathf.Max(0.05f, NonNegative(_syncInterval)))
            {
                PublishFuel();
                if (previous != _remainingFuel)
                    onFuelChanged?.Invoke();
            }
        }

        /// <summary>Server API for stations. The station must validate its own payment and proximity.</summary>
        public bool ServerRefuel(float amount, out float acceptedAmount)
        {
            acceptedAmount = 0f;
            if (!IsServer || !_initialized || !UsesFuel || Entity.IsDead() || !Finite(amount) || amount <= 0f)
                return false;
            acceptedAmount = Mathf.Min(amount, Capacity - _remainingFuel);
            if (acceptedAmount <= 0f)
                return false;
            _remainingFuel += acceptedAmount;
            PublishFuel();
            onFuelChanged?.Invoke();
            return true;
        }

        /// <summary>Persistence hook. Restore saved fuel after server identity initialization.</summary>
        public bool ServerRestoreFuel(float amount)
        {
            if (!IsServer || !_initialized || !Finite(amount) || amount < 0f)
                return false;
            _remainingFuel = Mathf.Clamp(amount, 0f, Capacity);
            PublishFuel();
            onFuelChanged?.Invoke();
            return true;
        }

        public int FindRefuelItemIndex(BaseCharacterEntity character)
        {
            if (!CanRefuel(character))
                return -1;
            for (int i = 0; i < character.NonEquipItems.Count; ++i)
                if (CanRefuelFromItem(character, i, out _))
                    return i;
            return -1;
        }

        public bool CanRefuelFromItem(BaseCharacterEntity character, int itemIndex, out float amount)
        {
            amount = 0f;
            if (!CanRefuel(character) || itemIndex < 0 || itemIndex >= character.NonEquipItems.Count)
                return false;
            CharacterItem inventoryItem = character.NonEquipItems[itemIndex];
            if (inventoryItem.amount <= 0 || _fuelItems == null)
                return false;
            foreach (VehicleFuelItemAmount fuelItem in _fuelItems)
            {
                if (fuelItem.item == null || fuelItem.item.DataId != inventoryItem.dataId ||
                    !Finite(fuelItem.fuelAmount) || fuelItem.fuelAmount <= 0f)
                    continue;
                // Consume a whole can only when all of its fuel fits; never discard excess fuel.
                if (Capacity - RemainingFuel < fuelItem.fuelAmount)
                    return false;
                amount = fuelItem.fuelAmount;
                return true;
            }
            return false;
        }

        public bool TryRefuelFromItem(BaseCharacterEntity character, int itemIndex)
        {
            if (!IsServer || character == null || !character.IsServer ||
                !CanRefuelFromItem(character, itemIndex, out float amount) ||
                !character.DecreaseItemsByIndex(itemIndex, 1, false))
                return false;
            _remainingFuel = Mathf.Min(Capacity, _remainingFuel + amount);
            PublishFuel();
            onFuelChanged?.Invoke();
            return true;
        }

        private bool CanRefuel(BaseCharacterEntity character)
        {
            return _initialized && UsesFuel && character != null && !character.IsDead() && !Entity.IsDead() &&
                RemainingFuel < Capacity &&
                (character.EntityTransform.position - EntityTransform.position).sqrMagnitude <=
                    NonNegative(_refuelDistance) * NonNegative(_refuelDistance) &&
                (Entity.Movement == null || Entity.Movement.CurrentMoveSpeed <= NonNegative(_maximumRefuelSpeed));
        }

        private void PublishFuel()
        {
            _publishElapsed = 0f;
            _syncedFuel.Value = _remainingFuel;
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static float NonNegative(float value) => Finite(value) ? Mathf.Max(0f, value) : 0f;
    }
}
