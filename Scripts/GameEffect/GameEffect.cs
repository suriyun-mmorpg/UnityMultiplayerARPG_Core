using UnityEngine;

namespace MultiplayerARPG
{
    public class GameEffect : PoolDescriptor
    {
        public enum PlayMode
        {
            PlayClipAtPoint,
            PlayClipAtAudioSource
        }
        public PlayMode playMode = PlayMode.PlayClipAtPoint;

        private AudioSource _cacheAudioSource;
        public AudioComponentSettingType settingType = AudioComponentSettingType.Sfx;
        public string otherSettingId;
        public string SettingId
        {
            get
            {
                switch (settingType)
                {
                    case AudioComponentSettingType.Master:
                        return AudioManager.Singleton.masterVolumeSetting.id;
                    case AudioComponentSettingType.Bgm:
                        return AudioManager.Singleton.bgmVolumeSetting.id;
                    case AudioComponentSettingType.Sfx:
                        return AudioManager.Singleton.sfxVolumeSetting.id;
                    case AudioComponentSettingType.Ambient:
                        return AudioManager.Singleton.ambientVolumeSetting.id;
                }
                return otherSettingId;
            }
        }

        public string effectSocket;
        public bool isLoop;
        public float lifeTime;
        public AudioClip[] randomSoundEffects = new AudioClip[0];

        private bool _intendToFollowTarget;
        private Transform _followingTarget;
        public Transform FollowingTarget
        {
            get { return _followingTarget; }
            set
            {
                if (value == null)
                    return;
                _followingTarget = value;
                _intendToFollowTarget = true;
            }
        }

        public Transform CacheTransform { get; private set; }

        private FxCollection _fxCollection;
        public FxCollection FxCollection
        {
            get
            {
                if (_fxCollection == null)
                    _fxCollection = new FxCollection(gameObject);
                return _fxCollection;
            }
        }

        protected float _destroyTime;

        protected virtual void Awake()
        {
            CacheTransform = transform;

            if (playMode == PlayMode.PlayClipAtAudioSource)
            {
                _cacheAudioSource = GetComponent<AudioSource>();
                if (_cacheAudioSource == null)
                {
                    _cacheAudioSource = gameObject.AddComponent<AudioSource>();
                    _cacheAudioSource.spatialBlend = 1f;
                    _cacheAudioSource.playOnAwake = false;
                }
            }
        }

        protected override void PushBack()
        {
            OnPushBack();
            if (ObjectPrefab != null)
                PoolSystem.PushBack(this);
            else if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        protected virtual void Update()
        {
            if (_destroyTime >= 0 && _destroyTime - Time.time <= 0)
            {
                PushBack();
                return;
            }
            if (FollowingTarget != null)
            {
                // Following target is not destroyed, follow its position
                CacheTransform.position = FollowingTarget.position;
                CacheTransform.rotation = FollowingTarget.rotation;
            }
            else if (_intendToFollowTarget)
            {
                // Following target destroyed, don't push back immediately, destroy it after some delay
                DestroyEffect();
            }
        }

        public virtual void DestroyEffect()
        {
            FxCollection.SetLoop(false);
            _destroyTime = Time.time + lifeTime;
        }

        public override void InitPrefab()
        {
            if (this == null)
            {
                Debug.LogWarning("The Game Effect is null, this should not happens");
                return;
            }
            FxCollection.InitPrefab();
            base.InitPrefab();
        }

        public override void OnGetInstance()
        {
            Play();
            base.OnGetInstance();
        }

        /// <summary>
        /// Play particle effects and an audio
        /// </summary>
        public virtual void Play()
        {
            if (!BaseGameNetworkManager.Singleton.IsClientConnected)
                return;

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            // Prepare destroy time
            _destroyTime = isLoop ? -1 : Time.time + lifeTime;

            switch (playMode)
            {
                case PlayMode.PlayClipAtAudioSource:
                    if (!Application.isBatchMode && !AudioListener.pause && randomSoundEffects.Length > 0)
                    {
                        _cacheAudioSource.clip = randomSoundEffects[Random.Range(0, randomSoundEffects.Length)];
                        _cacheAudioSource.volume = AudioManager.Singleton.GetVolumeLevel(SettingId);
                        _cacheAudioSource.loop = isLoop;
                        _cacheAudioSource.Play();
                    }
                    break;
                case PlayMode.PlayClipAtPoint:
                    if (!Application.isBatchMode && !AudioListener.pause && randomSoundEffects.Length > 0)
                    {
                        AudioSource.PlayClipAtPoint(randomSoundEffects[Random.Range(0, randomSoundEffects.Length)], CacheTransform.position, AudioManager.Singleton.GetVolumeLevel(SettingId));
                    }
                    break;
            }

            FxCollection.Play();
        }
    }
}
