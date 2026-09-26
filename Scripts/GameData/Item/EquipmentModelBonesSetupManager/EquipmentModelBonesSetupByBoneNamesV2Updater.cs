using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MultiplayerARPG
{
    [DefaultExecutionOrder(int.MaxValue - 1)]
    public class EquipmentModelBonesSetupByBoneNamesV2Updater : MonoBehaviour
    {
        public bool changeRootBone = false;
        public bool useTheSameBonesWithTheFirstOne = false;
        public bool cacheTransformWithFullPath = false;
        public string topMostRootBoneName = "Root";
        private static readonly Stack<string> _lookUpNames = new Stack<string>();
        private static readonly StringBuilder _stringBuilder = new StringBuilder();
        private readonly Dictionary<string, Transform> _cachedTransforms = new Dictionary<string, Transform>();
        private bool _isCachedCharacterTopMostRootBone = false;
        private Transform _cachedCharacterTopMostRootBone = null;

        public void SetupForObject(BaseCharacterModel characterModel, GameObject instantiatedObject, EquipmentContainer equipmentContainer)
        {
            if (instantiatedObject == null)
                return;

            if (!(characterModel is IModelWithAnimator animatorSrc))
            {
                Debug.LogWarning($"[{nameof(EquipmentModelBonesSetupByBoneNamesV2Updater)}] Character model \"{characterModel}\" has no skinned mesh renderer.");
                return;
            }

            if (animatorSrc.Animator == null || animatorSrc.Animator.avatar == null || animatorSrc.Animator.avatarRoot == null)
            {
                Debug.LogWarning($"[{nameof(EquipmentModelBonesSetupByBoneNamesV2Updater)}] Character model \"{characterModel}\" has no animator or avatar.");
                return;
            }

            if (!_isCachedCharacterTopMostRootBone)
            {
                _isCachedCharacterTopMostRootBone = true;
                _cachedCharacterTopMostRootBone = FindTopMostRootBone(animatorSrc.Animator.avatarRoot);
            }
            if (_cachedCharacterTopMostRootBone == null)
            {
                Debug.LogWarning($"[{nameof(EquipmentModelBonesSetupByBoneNamesV2Updater)}] Character model \"{characterModel}\" has no top most root bone with name \"{topMostRootBoneName}\".");
                return;
            }

            Animator instantiatedAnimator = instantiatedObject.GetComponentInChildren<Animator>();
            if (instantiatedAnimator == null || instantiatedAnimator.avatar == null)
                return;

            Transform instantiatedTopMostRootBone = FindTopMostRootBone(instantiatedAnimator.avatarRoot);
            if (instantiatedTopMostRootBone == null)
            {
                Debug.LogWarning($"[{nameof(EquipmentModelBonesSetupByBoneNamesV2Updater)}] Instantiated model \"{instantiatedObject}\" has no top most root bone with name \"{topMostRootBoneName}\".");
                return;
            }

            SkinnedMeshRenderer[] instantiatedMeshes = instantiatedObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            Transform[] bones = null;
            Transform rootBone = null;
            for (int i = 0; i < instantiatedMeshes.Length; ++i)
            {
                SkinnedMeshRenderer instantiatedMesh = instantiatedMeshes[i];
                if (instantiatedMesh == null)
                    continue;
                Transform[] tempBones = instantiatedMesh.bones;
                Transform tempRootBone = instantiatedMesh.rootBone;

                if (useTheSameBonesWithTheFirstOne && bones != null)
                {
                    tempBones = bones;
                }
                else
                {
                    for (int j = 0; j < tempBones.Length; ++j)
                    {
                        Transform bone = tempBones[j];
                        if (bone == null)
                            continue;
                        Transform tempBone = FindExactStructureBone(bone);
                        if (tempBone == null)
                            continue;
                        tempBones[j] = tempBone;
                    }
                    // Set first bones if it's null, so we can use the same bones for other meshes
                    if (bones == null)
                        bones = tempBones;
                }
                if (useTheSameBonesWithTheFirstOne && rootBone != null)
                {
                    tempRootBone = rootBone;
                }
                else
                {
                    if (changeRootBone && instantiatedMesh.rootBone != null)
                    {
                        Transform tempBone = FindExactStructureBone(instantiatedMesh.rootBone);
                        if (tempBone == null)
                            continue;
                        tempRootBone = tempBone;
                    }
                    // Set first root bone if it's null, so we can use the same root bone for other meshes
                    if (rootBone == null)
                        rootBone = tempRootBone;
                }
                instantiatedMesh.bones = tempBones;
                instantiatedMesh.rootBone = tempRootBone;
            }
        }

        private Transform FindExactStructureBone(Transform srcBone)
        {
            if (string.Equals(srcBone.name, _cachedCharacterTopMostRootBone.name))
                return _cachedCharacterTopMostRootBone;

            string key = srcBone.name;
            if (!cacheTransformWithFullPath && _cachedTransforms.TryGetValue(key, out Transform result) && result != null)
                return result;

            Transform tempBone = srcBone;
            _lookUpNames.Clear();
            do
            {
                _lookUpNames.Push(tempBone.name);
                tempBone = tempBone.parent;
            } while (tempBone != null && !string.Equals(tempBone.name, _cachedCharacterTopMostRootBone.name));

            _stringBuilder.Clear();
            while (_lookUpNames.TryPop(out string name))
            {
                _stringBuilder.Append(name);
                if (_lookUpNames.Count > 0)
                    _stringBuilder.Append('/');
            }

            string path = _stringBuilder.ToString();
            if (!cacheTransformWithFullPath)
            {
                result = _cachedCharacterTopMostRootBone.Find(path);
                _cachedTransforms[key] = result;
            }
            else
            {
                key = path;
                if (!_cachedTransforms.TryGetValue(key, out result) || result == null)
                {
                    result = _cachedCharacterTopMostRootBone.Find(path);
                    _cachedTransforms[key] = result;
                }
            }
            return result;
        }

        private Transform FindTopMostRootBone(Transform root)
        {
            if (string.Equals(root.name, topMostRootBoneName))
                return root;

            for (int i = 0; i < root.childCount; ++i)
            {
                Transform found = FindTopMostRootBone(root.GetChild(i));
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
