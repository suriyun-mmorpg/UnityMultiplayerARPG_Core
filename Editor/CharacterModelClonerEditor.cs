using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public class CharacterModelClonerEditor : EditorWindow
    {
        private BaseRemakeCharacterModel srcModel;
        private BaseRemakeCharacterModel dstModel;
        private bool cloneEffectContainers;
        private bool cloneEquipmentContainers;
        private bool cloneHitboxes;
        private bool cloneDefaultAnimations;
        private bool cloneWeaponAnimations;
        private bool cloneSkillAnimations;

        [MenuItem(EditorMenuConsts.CHARACTER_MODEL_CLONER_MENU, false, EditorMenuConsts.CHARACTER_MODEL_CLONER_ORDER)]
        public static void CreateNewEditor()
        {
            GetWindow<CharacterModelClonerEditor>();
        }

        private void OnGUI()
        {
            Vector2 wndRect = new Vector2(500, 500);
            maxSize = wndRect;
            minSize = wndRect;
            titleContent = new GUIContent("Character Model Cloner", null, "Character Model Cloner (3D)");
            GUILayout.BeginVertical("Character Model Cloner", "window");
            {
                GUILayout.BeginVertical("box");
                {
                    if (srcModel == null)
                        EditorGUILayout.HelpBox("Source character model which you want to copy from, must be `AnimationCharacterModel` or `AnimatorCharacterModel`", MessageType.Info);
                    srcModel = EditorGUILayout.ObjectField("Source", srcModel, typeof(BaseRemakeCharacterModel), true, GUILayout.ExpandWidth(true)) as BaseRemakeCharacterModel;
                    if (dstModel == null)
                        EditorGUILayout.HelpBox("Destination character model which you want to paste to, must be `AnimationCharacterModel` or `AnimatorCharacterModel`", MessageType.Info);
                    dstModel = EditorGUILayout.ObjectField("Destination", dstModel, typeof(BaseRemakeCharacterModel), true, GUILayout.ExpandWidth(true)) as BaseRemakeCharacterModel;

                    if (srcModel is AnimatorCharacterModel &&
                        dstModel is AnimatorCharacterModel &&
                        (srcModel as AnimatorCharacterModel).animator != null &&
                        (dstModel as AnimatorCharacterModel).animator != null &&
                        (srcModel as AnimatorCharacterModel).animator.isHuman &&
                        (dstModel as AnimatorCharacterModel).animator.isHuman)
                    {
                        cloneEffectContainers = EditorGUILayout.Toggle("Clone Effect Containers", cloneEffectContainers);
                        cloneEquipmentContainers = EditorGUILayout.Toggle("Clone Equipment Containers", cloneEquipmentContainers);
                        cloneHitboxes = EditorGUILayout.Toggle("Clone Hitboxes", cloneHitboxes);
                    }
                    else
                    {
                        cloneEffectContainers = false;
                        cloneEquipmentContainers = false;
                        cloneHitboxes = false;
                    }
                    cloneDefaultAnimations = EditorGUILayout.Toggle("Clone Default Animations", cloneDefaultAnimations);
                    cloneWeaponAnimations = EditorGUILayout.Toggle("Clone Weapon Animations", cloneWeaponAnimations);
                    cloneSkillAnimations = EditorGUILayout.Toggle("Clone Skill Animations", cloneSkillAnimations);
                }
                GUILayout.EndVertical();

                if (srcModel != null && dstModel != null)
                {
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Clone", GUILayout.ExpandWidth(true), GUILayout.Height(40)))
                        Clone();
                    GUILayout.EndHorizontal();
                }
            }
            GUILayout.EndVertical();
        }

        private void Clone()
        {
            if (cloneEffectContainers)
            {
                List<EffectContainer> containers = new List<EffectContainer>();
                for (int i = 0; i < srcModel.EffectContainers.Length; ++i)
                {
                    GameObject newObj = null;
                    bool isRootTransform;
                    HumanBodyBones bone;
                    Transform cloneSrc;
                    List<int> childIndexes;
                    FindBoneAndRootContainer(srcModel.transform.root, (srcModel as AnimatorCharacterModel).animator, srcModel.EquipmentContainers[i].transform, out isRootTransform, out bone, out cloneSrc, out childIndexes);
                    if (cloneSrc != null)
                    {
                        if (isRootTransform)
                        {
                            newObj = Instantiate(cloneSrc.gameObject, dstModel.transform.root);
                        }
                        else
                        {
                            newObj = Instantiate(cloneSrc.gameObject, (dstModel as AnimatorCharacterModel).animator.GetBoneTransform(bone));
                        }
                        newObj.transform.localPosition = cloneSrc.localPosition;
                        newObj.transform.localEulerAngles = cloneSrc.localEulerAngles;
                        newObj.transform.localRotation = cloneSrc.localRotation;
                        for (int j = childIndexes.Count - 1; j >= 0; --j)
                        {
                            newObj = newObj.transform.GetChild(childIndexes[j]).gameObject;
                        }
                    }
                    containers.Add(new EffectContainer()
                    {
                        effectSocket = srcModel.EffectContainers[i].effectSocket,
                        transform = newObj == null ? null : newObj.transform,
                    });
                }
                dstModel.EffectContainers = containers.ToArray();
            }
            if (cloneEquipmentContainers)
            {
                List<EquipmentContainer> containers = new List<EquipmentContainer>();
                for (int i = 0; i < srcModel.EquipmentContainers.Length; ++i)
                {
                    GameObject newObj = null;
                    bool isRootTransform;
                    HumanBodyBones bone;
                    Transform cloneSrc;
                    List<int> childIndexes;
                    FindBoneAndRootContainer(srcModel.transform.root, (srcModel as AnimatorCharacterModel).animator, srcModel.EquipmentContainers[i].transform, out isRootTransform, out bone, out cloneSrc, out childIndexes);
                    if (cloneSrc != null)
                    {
                        if (isRootTransform)
                        {
                            newObj = Instantiate(cloneSrc.gameObject, dstModel.transform.root);
                        }
                        else
                        {
                            newObj = Instantiate(cloneSrc.gameObject, (dstModel as AnimatorCharacterModel).animator.GetBoneTransform(bone));
                        }
                        newObj.transform.localPosition = cloneSrc.localPosition;
                        newObj.transform.localEulerAngles = cloneSrc.localEulerAngles;
                        newObj.transform.localRotation = cloneSrc.localRotation;
                        for (int j = childIndexes.Count - 1; j >= 0; --j)
                        {
                            newObj = newObj.transform.GetChild(childIndexes[j]).gameObject;
                        }
                    }
                    containers.Add(new EquipmentContainer()
                    {
                        equipSocket = srcModel.EquipmentContainers[i].equipSocket,
                        transform = newObj == null ? null : newObj.transform,
                    });
                }
                dstModel.EquipmentContainers = containers.ToArray();
            }
            if (cloneHitboxes)
            {
                DamageableHitBox[] hitBoxes = srcModel.transform.root.GetComponentsInChildren<DamageableHitBox>(true);
                for (int i = 0; i < hitBoxes.Length; ++i)
                {
                    GameObject newObj;
                    bool isRootTransform;
                    HumanBodyBones bone;
                    Transform cloneSrc;
                    FindBoneAndRootContainer(srcModel.transform.root, (srcModel as AnimatorCharacterModel).animator, hitBoxes[i].transform, out isRootTransform, out bone, out cloneSrc, out _);
                    if (cloneSrc != null)
                    {
                        if (isRootTransform)
                        {
                            newObj = Instantiate(cloneSrc.gameObject, dstModel.transform.root);
                        }
                        else
                        {
                            newObj = Instantiate(cloneSrc.gameObject, (dstModel as AnimatorCharacterModel).animator.GetBoneTransform(bone));
                        }
                        newObj.transform.localPosition = cloneSrc.localPosition;
                        newObj.transform.localEulerAngles = cloneSrc.localEulerAngles;
                        newObj.transform.localRotation = cloneSrc.localRotation;
                    }
                }
            }
            if (cloneDefaultAnimations)
            {
                dstModel.defaultAnimations = srcModel.defaultAnimations;
            }
            if (cloneWeaponAnimations)
            {
                dstModel.weaponAnimations = new List<WeaponAnimations>(srcModel.weaponAnimations).ToArray();
            }
            if (cloneSkillAnimations)
            {
                dstModel.skillAnimations = new List<SkillAnimations>(srcModel.skillAnimations).ToArray();
            }
            EditorUtility.SetDirty(dstModel.gameObject);
        }

        private void FindBoneAndRootContainer(Transform rootTransform, Animator srcAnimator, Transform srcTransform, out bool isRootTransform, out HumanBodyBones bone, out Transform cloneSrc, out List<int> childIndexes)
        {
            isRootTransform = false;
            bone = HumanBodyBones.Hips;
            cloneSrc = null;
            childIndexes = new List<int>();
            if (srcTransform == null)
                return;
            cloneSrc = srcTransform;
            do
            {
                for (int i = 0; i < (int)HumanBodyBones.LastBone; ++i)
                {
                    if (srcAnimator.GetBoneTransform((HumanBodyBones)i) == srcTransform.parent)
                    {
                        bone = (HumanBodyBones)i;
                        return;
                    }
                }
                childIndexes.Add(srcTransform.GetSiblingIndex());
                srcTransform = srcTransform.parent;
                cloneSrc = srcTransform;
            } while (srcTransform.parent != rootTransform);
            isRootTransform = true;
        }
    }
}
