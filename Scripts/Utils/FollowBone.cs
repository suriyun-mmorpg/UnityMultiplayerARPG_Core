using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UtilsComponents
{
    [DefaultExecutionOrder(int.MaxValue)]
    public class FollowBone : MonoBehaviour
    {
        public HumanBodyBones bone = HumanBodyBones.UpperChest;
        public bool followPosition = true;
        public bool followRotation = true;
        public Animator animator;

        private void Awake()
        {
            if (animator == null)
                animator = GetComponentInParent<Animator>();
        }

        private void LateUpdate()
        {
            Transform tempTransform = animator.GetBoneTransform(bone);
            if (followPosition)
                transform.position = tempTransform.position;
            if (followRotation)
                transform.rotation = tempTransform.rotation;
        }

        [ContextMenu("Force Update")]
        public void ForceUpdate()
        {
            if (animator == null)
                animator = GetComponentInParent<Animator>();
            LateUpdate();
        }

        [ContextMenu("Move Children To Selected Bone")]
        public void MoveChildrenToSelectedBone()
        {
            Transform tempTransform = animator.GetBoneTransform(bone);
            for (int i = transform.childCount - 1; i >= 0; --i)
            {
                Transform childTransform = transform.GetChild(i);
                Vector3 position = childTransform.position;
                Quaternion rotation = childTransform.rotation;
                childTransform.parent = tempTransform;
                childTransform.position = position;
                childTransform.rotation = rotation;
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(transform);
#endif
        }
    }
}
