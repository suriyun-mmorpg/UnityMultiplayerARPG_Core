using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ProjectileEffect : PoolDescriptor
    {
        public float speed;
        public float lifeTime = 1;
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
        protected bool _playFxOnEnable;
        protected ImpactEffects _impactEffects;
        protected Vector3 _launchOrigin;
        protected readonly List<ImpactEffectPlayingData> _impacts = new List<ImpactEffectPlayingData>();

        protected virtual void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
            UpdateImpactEffects(false);
        }

        protected virtual void UpdateImpactEffects(bool pushingBack)
        {
            if (_impacts.Count > 0)
                return;
            GameEffect tempGameEffect;
            if (pushingBack)
            {
                for (int i = 0; i < _impacts.Count; ++i)
                {
                    if (_impactEffects.TryGetEffect(_impacts[i].tag, out tempGameEffect))
                        PoolSystem.GetInstance(tempGameEffect, _impacts[i].point, Quaternion.LookRotation(Vector3.up, _impacts[i].normal));
                }
                _impacts.Clear();
                return;
            }
            while (_impacts.Count > 0 && (transform.position - _launchOrigin).sqrMagnitude > (_impacts[0].point - _launchOrigin).sqrMagnitude)
            {
                if (_impactEffects.TryGetEffect(_impacts[0].tag, out tempGameEffect))
                    PoolSystem.GetInstance(tempGameEffect, _impacts[0].point, Quaternion.LookRotation(Vector3.up, _impacts[0].normal));
                _impacts.RemoveAt(0);
            }
        }

        protected virtual void OnEnable()
        {
            if (_playFxOnEnable)
                PlayFx();
        }

        public virtual void Setup(float distance, float speed, ImpactEffects impactEffects, Vector3 launchOrigin, List<ImpactEffectPlayingData> impacts)
        {
            this.speed = speed;
            lifeTime = distance / speed;
            if (lifeTime <= 0f)
            {
                PushBack();
                return;
            }
            PushBack(lifeTime);
            _impactEffects = impactEffects;
            _launchOrigin = launchOrigin;
            _impacts.Clear();
            _impacts.AddRange(impacts);
            _impacts.Sort((a, b) => (a.point - launchOrigin).sqrMagnitude.CompareTo((b.point - launchOrigin).sqrMagnitude));
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
            _impactEffects = null;
            _impacts.Clear();
            base.OnGetInstance();
        }

        protected override void OnPushBack()
        {
            StopFx();
            UpdateImpactEffects(true);
            base.OnPushBack();
        }

        public virtual void PlayFx()
        {
            if (!gameObject.activeInHierarchy)
            {
                _playFxOnEnable = true;
                return;
            }
            FxCollection.Play();
            _playFxOnEnable = false;
        }

        public virtual void StopFx()
        {
            FxCollection.Stop();
        }
    }
}
