using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class ProjectileEffect : MonoBehaviour
    {
        public float speed;
        public float lifeTime = 1;

        private void Start()
        {
            Destroy(gameObject, lifeTime);
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
