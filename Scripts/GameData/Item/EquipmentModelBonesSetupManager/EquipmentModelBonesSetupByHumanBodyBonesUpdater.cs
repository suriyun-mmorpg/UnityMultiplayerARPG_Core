using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    [DefaultExecutionOrder(int.MaxValue)]
    public class EquipmentModelBonesSetupByHumanBodyBonesUpdater : MonoBehaviour
    {
        [System.Serializable]
        public struct TransformCopyData
        {
            public Transform src;
            public Transform dst;
        }

        [System.Serializable]
        public struct PredefinedBone
        {
            public HumanBodyBones boneType;
            public Transform boneTransform;
        }

        public PredefinedBone[] predefinedBones = new PredefinedBone[0];

        private List<TransformCopyData> _copyingTransforms = new List<TransformCopyData>();

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
            if (src == null || dst == null)
                return;
            _copyingTransforms.Clear();
            for (int i = 0; i < (int)HumanBodyBones.LastBone; ++i)
            {
                Transform srcTransform = src.GetBoneTransform((HumanBodyBones)i);
                if (srcTransform == null)
                    continue;
                Transform dstTransform = null;
                try
                {
                    dstTransform = dst.GetBoneTransform((HumanBodyBones)i);
                }
                catch (System.Exception)
                {
                    // Some error occuring, skip it.
                }
                if (dstTransform != null)
                {
                    _copyingTransforms.Add(new TransformCopyData()
                    {
                        src = srcTransform,
                        dst = dstTransform,
                    });
                }
                else if (PredefinedBonesDict.TryGetValue((HumanBodyBones)i, out dstTransform))
                {
                    _copyingTransforms.Add(new TransformCopyData()
                    {
                        src = srcTransform,
                        dst = dstTransform,
                    });
                }
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < _copyingTransforms.Count; ++i)
            {
                _copyingTransforms[i].dst.position = _copyingTransforms[i].src.position;
                _copyingTransforms[i].dst.eulerAngles = _copyingTransforms[i].src.eulerAngles;
            }
        }
    }
}
