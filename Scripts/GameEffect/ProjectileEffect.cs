using UnityEngine;

namespace MultiplayerARPG
{
    public class ProjectileEffect : PoolDescriptor
    {
        public float speed;
        public float lifeTime = 1;
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
        private bool playFxOnEnable;

        protected virtual void Awake()
        {
            CacheTransform = transform;
        }

        protected virtual void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        protected virtual void OnEnable()
        {
            if (playFxOnEnable)
                PlayFx();
        }

        public virtual void Setup(float distance, float speed)
        {
            this.speed = speed;
            lifeTime = distance / speed;
            PushBack(lifeTime);
        }

        public override void InitPrefab()
        {
            if (this == null)
            {
                Debug.LogWarning("The Projectile Effect is null, this should not happens");
                return;
            }
            FxCollection.InitPrefab();
            base.InitPrefab();
        }

        public override void OnGetInstance()
        {
            PlayFx();
            base.OnGetInstance();
        }

        protected override void OnPushBack()
        {
            StopFx();
            base.OnPushBack();
        }

        public virtual void PlayFx()
        {
            if (!gameObject.activeInHierarchy)
            {
                playFxOnEnable = true;
                return;
            }
            FxCollection.Play();
            playFxOnEnable = false;
        }

        public virtual void StopFx()
        {
            FxCollection.Stop();
        }
    }
}
