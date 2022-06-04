using UnityEngine;

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
    }
}
