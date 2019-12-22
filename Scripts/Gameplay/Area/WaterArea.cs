using UnityEngine;
using System.Collections;

namespace MultiplayerARPG
{
    public class WaterArea : MonoBehaviour
    {
        private void Awake()
        {
            // Set layer to water
            gameObject.layer = 4;
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
            BaseGameEntity gameEntity = other.GetComponent<BaseGameEntity>();
            if (gameEntity == null)
                return;

            gameEntity.IsUnderWater = true;
        }

        private void OnTriggerExit(Collider other)
        {
            TriggerEnter(other.gameObject);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TriggerEnter(other.gameObject);
        }

        private void TriggerExit(GameObject other)
        {
            BaseGameEntity gameEntity = other.GetComponent<BaseGameEntity>();
            if (gameEntity == null)
                return;

            gameEntity.IsUnderWater = false;
        }
    }
}
