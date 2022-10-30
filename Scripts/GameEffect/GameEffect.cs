using UnityEngine;

namespace MultiplayerARPG
{
    public class GameEffect : PoolDescriptor
    {
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
        private bool intendToFollowingTarget;
        private Transform followingTarget;
        public Transform FollowingTarget
        {
            get { return followingTarget; }
            set
            {
                if (value == null)
                    return;
                followingTarget = value;
                intendToFollowingTarget = true;
            }
        }

        public Transform CacheTransform { get; private set; }
        private FxCollection fxCollection;
        public FxCollection FxCollection
        {
            get
            {
                if (fxCollection == null)
                    fxCollection = new FxCollection(gameObject);
                return fxCollection;
            }
        }

        public AudioClip[] randomSoundEffects = new AudioClip[0];
        private float destroyTime;

        private void Awake()
        {
            CacheTransform = transform;
        }

        protected override void PushBack()
        {
            OnPushBack();
            if (ObjectPrefab != null)
                PoolSystem.PushBack(this);
            else if (gameObject.activeSelf)
                gameObject.SetActive(false);
        }

        private void Update()
        {
            if (destroyTime >= 0 && destroyTime - Time.time <= 0)
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
            else if (intendToFollowingTarget)
            {
                // Following target destroyed, don't push back immediately, destroy it after some delay
                DestroyEffect();
            }
        }

        public void DestroyEffect()
        {
            FxCollection.SetLoop(false);
            destroyTime = Time.time + lifeTime;
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
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);
            // Prepare destroy time
            destroyTime = isLoop ? -1 : Time.time + lifeTime;
            if (!Application.isBatchMode && !AudioListener.pause)
            {
                // Play random audio
                if (randomSoundEffects.Length > 0)
                    AudioSource.PlayClipAtPoint(randomSoundEffects[Random.Range(0, randomSoundEffects.Length)], CacheTransform.position, AudioManager.Singleton.GetVolumeLevel(SettingId));
            }
            FxCollection.Play();
        }
    }
}
