using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DefaultExecutionOrder(int.MaxValue - 1)]
    public class EquipmentModelBonesSetupByHumanBodyBonesUpdater : MonoBehaviour
    {
        [System.Serializable]
        public struct PredefinedBone
        {
            public HumanBodyBones boneType;
            public Transform boneTransform;
        }

        public PredefinedBone[] predefinedBones = new PredefinedBone[0];

        private Dictionary<HumanBodyBones, Transform> _predefinedBonesDict;
        public Dictionary<HumanBodyBones, Transform> PredefinedBonesDict
        {
            get
            {
                if (_predefinedBonesDict == null)
                {
                    _predefinedBonesDict = new Dictionary<HumanBodyBones, Transform>();
                    for (int i = 0; i < predefinedBones.Length; ++i)
                    {
                        _predefinedBonesDict.Add(predefinedBones[i].boneType, predefinedBones[i].boneTransform);
                    }
                }
                return _predefinedBonesDict;
            }
        }

        public void PrepareTransforms(Animator src, Animator dst)
        {
#if !UNITY_SERVER
            if (src == null || dst == null)
                return;

            if (dst.avatar == null || !dst.avatar.isHuman)
                return;

            AnimatorHandle srcAnimatorHandle = src.gameObject.GetOrAddComponent<AnimatorHandle>();
            AnimatorHandle dstAnimatorHandle = dst.gameObject.GetOrAddComponent<AnimatorHandle>();

            List<Transform> srcTransforms = new List<Transform>();
            List<Transform> dstTransforms = new List<Transform>();

            for (HumanBodyBones i = 0; i < HumanBodyBones.LastBone; ++i)
            {
                // Add all bones althrough it is null
                Transform srcTransform = src.GetBoneTransform(i);
                srcTransforms.Add(srcTransform);

                // Priority: predefined bones > bones from dst animator
                Transform dstTransform;
                if (!PredefinedBonesDict.TryGetValue(i, out dstTransform))
                    dstTransform = dst.GetBoneTransform(i);
                dstTransforms.Add(dstTransform);
            }

            EquipmentModelBonesSetupByHumanBodyBonesUpdateManager.Instance.Register(srcAnimatorHandle, srcTransforms, dstAnimatorHandle, dstTransforms);
#endif
        }
    }
}