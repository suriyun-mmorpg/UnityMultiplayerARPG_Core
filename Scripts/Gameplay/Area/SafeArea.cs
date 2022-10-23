using UnityEngine;

namespace MultiplayerARPG
{
    public class SafeArea : MonoBehaviour, IUnHittable
    {
        private void Awake()
        {
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEnter(other.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerEnter(other.gameObject);
        }

        private void TriggerEnter(GameObject other)
        {
            IDamageableEntity gameEntity = other.GetComponent<IDamageableEntity>();
            if (gameEntity.IsNull())
                return;
            gameEntity.IsInSafeArea = true;
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerExit(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TriggerExit(other.gameObject);
        }

        private void TriggerExit(GameObject other)
        {
            IDamageableEntity gameEntity = other.GetComponent<IDamageableEntity>();
            if (gameEntity.IsNull())
                return;
            gameEntity.IsInSafeArea = false;
        }
    }
}
