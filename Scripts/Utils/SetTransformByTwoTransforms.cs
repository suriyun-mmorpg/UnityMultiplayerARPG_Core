using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UtilsComponents
{
    public class SetTransformByTwoTransforms : MonoBehaviour
    {
        public Transform transform1;
        public Transform transform2;
        public Vector3 upVector3 = Vector3.forward;
        public Vector3 rotateAngles = Vector3.zero;

        [ContextMenu("Set")]
        public void Set()
        {
            Vector3 dir = (transform2.position - transform1.position).normalized;
            float dist = Vector3.Distance(transform1.position, transform2.position);
            transform.position = transform1.position + (dir * dist * 0.5f);
            transform.rotation = Quaternion.LookRotation(dir, upVector3);
            transform.localEulerAngles += rotateAngles;
#if UNITY_EDITOR
            EditorUtility.SetDirty(gameObject);
#endif
        }
    }
}