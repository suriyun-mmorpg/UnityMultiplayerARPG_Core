using UnityEngine;

namespace MultiplayerARPG
{
    public class AutoRotate : MonoBehaviour
    {
        public Vector3 eulerAngles;
        private void Update()
        {
            transform.eulerAngles += eulerAngles * Time.deltaTime;
        }
    }
}
