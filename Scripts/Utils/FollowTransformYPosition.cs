using UnityEngine;

namespace UtilsComponents
{
    public class FollowTransformYPosition : MonoBehaviour
    {
        public Transform targetTransform;
        public float yOffsets;

        private void Update()
        {
            transform.position = new Vector3(transform.position.x, targetTransform.position.y + yOffsets, transform.position.z);
        }
    }
}
