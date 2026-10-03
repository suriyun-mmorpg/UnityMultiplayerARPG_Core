using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>Optional horn. The movement adapter supplies validated server input and local feedback.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(VehicleEntity))]
    public class VehicleHornComponent : BaseNetworkedGameEntityComponent<VehicleEntity>
    {
        [Tooltip("A dedicated looping AudioSource. Do not share the engine's audio source.")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField, Min(0.05f)] private float _localInputTimeout = 0.5f;
        [SerializeField] private SyncFieldBool _syncedHorn = new SyncFieldBool();

        private bool _initialized;
        private bool _localInput;
        private bool _localPresentation;
        private float _lastLocalInputTime = float.NegativeInfinity;

        public AudioSource HornAudioSource => _audioSource;
        public bool LocalInput => _localInput;
        public bool IsHornActive => _initialized && enabled && HasValidDriver && !Entity.IsDead() && _syncedHorn.Value;
        public bool CanUseLocalHorn => _initialized && enabled && IsClient && IsOwnerClient &&
            HasValidDriver && Entity.GetPassenger(0).IsOwnerClient && !Entity.IsDead();
        public bool ShouldPlayAudio => IsClient && _initialized && enabled && HasValidDriver && !Entity.IsDead() &&
            (CanUseLocalHorn ? _localPresentation && Time.unscaledTime - _lastLocalInputTime <= _localInputTimeout : _syncedHorn.Value);

        private bool HasValidDriver => Entity.HasDriver && Entity.GetPassenger(0) != null &&
            (!(Entity.GetPassenger(0) is IDamageableEntity damageable) || !damageable.IsDead());

        private void Awake()
        {
            if (_audioSource != null)
            {
                _audioSource.playOnAwake = false;
                _audioSource.loop = true;
            }
        }

        public override void OnSetup()
        {
            base.OnSetup();
            _syncedHorn.syncMode = LiteNetLibSyncFieldMode.ServerToClients;
            _syncedHorn.onChange -= OnHornChanged;
            _syncedHorn.onChange += OnHornChanged;
        }

        public override void OnIdentityInitialize()
        {
            base.OnIdentityInitialize();
            _initialized = true;
            ResetLocalInput();
            if (IsServer) _syncedHorn.Value = false;
            RefreshAudio();
        }

        public override void OnSetOwnerClient(bool isOwnerClient)
        {
            ResetLocalInput();
            if (IsServer) ServerSetHorn(false);
        }

        public override void OnNetworkDestroy(byte reasons)
        {
            _initialized = false;
            ResetLocalInput();
            base.OnNetworkDestroy(reasons);
        }

        private void OnDisable()
        {
            ResetLocalInput();
            if (IsServer) ServerSetHorn(false);
        }

        protected override void OnDestroy()
        {
            if (_audioSource != null) _audioSource.Stop();
            _syncedHorn.onChange -= OnHornChanged;
            base.OnDestroy();
        }

        private void Update()
        {
            if (_initialized && IsServer && _syncedHorn.Value && (!HasValidDriver || Entity.IsDead()))
                ServerSetHorn(false);
            RefreshAudio();
        }

        private void OnHornChanged(bool initial, bool oldValue, bool newValue) => RefreshAudio();

        /// <summary>Held UI input, read by the current driver's movement controller.</summary>
        public void SetLocalInput(bool pressed)
        {
            _localInput = pressed && CanUseLocalHorn;
            if (!_localInput)
            {
                _localPresentation = false;
                RefreshAudio();
            }
        }

        /// <summary>Call when the owning driver supplies movement input, including release.</summary>
        public void SetLocalPresentation(bool pressed)
        {
            _localPresentation = pressed && CanUseLocalHorn;
            _lastLocalInputTime = Time.unscaledTime;
            RefreshAudio();
        }

        public void ResetLocalInput()
        {
            _localInput = _localPresentation = false;
            _lastLocalInputTime = float.NegativeInfinity;
            RefreshAudio();
        }

        /// <summary>Call each simulation tick with input validated by the vehicle movement adapter.</summary>
        public void ServerSetHorn(bool pressed)
        {
            if (!_initialized || !IsServer) return;
            _syncedHorn.Value = pressed && enabled && HasValidDriver && !Entity.IsDead();
            RefreshAudio();
        }

        private void RefreshAudio()
        {
            if (_audioSource == null) return;
            if (ShouldPlayAudio && _audioSource.isActiveAndEnabled && _audioSource.clip != null)
            {
                if (!_audioSource.isPlaying) _audioSource.Play();
            }
            else if (_audioSource.isPlaying)
                _audioSource.Stop();
        }
    }
}
