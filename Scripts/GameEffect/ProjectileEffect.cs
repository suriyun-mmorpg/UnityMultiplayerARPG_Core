using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ProjectileEffect : MonoBehaviour, IPoolDescriptor
    {
        public float speed;
        public float lifeTime = 1;
        [SerializeField]
        private int poolSize = 30;
        public int PoolSize { get { return poolSize; } set { poolSize = value; } }
        public IPoolDescriptor ObjectPrefab { get; set; }

        private void Start()
        {
            PushBack(lifeTime);
        }

        protected virtual void PushBack(float delay)
        {
            Invoke("PushBack", delay);
        }

        protected virtual void PushBack()
        {
            PoolSystem.PushBack(this);
        }

        private void Update()
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        public void Setup(float distance, float speed)
        {
            this.speed = speed;
            lifeTime = distance / speed;
        }
    }
}
