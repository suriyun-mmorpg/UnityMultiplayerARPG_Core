using MultiplayerARPG.GameData.Model.Playables;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    public class CharacterPitchIKManager : MonoBehaviour
    {
        public Animator Animator { get; private set; }
        public BaseCharacterEntity CharacterEntity { get; private set; }
        private readonly List<CharacterPitchIK> components = new List<CharacterPitchIK>();
        private PlayableCharacterModel playableCharacterModel;
        private bool forPlayableCharacterModel;
        private bool sizeChanged;
        private NativeArray<bool> enablings;
        private NativeArray<TransformStreamHandle> pitchBones;
        private NativeArray<Quaternion> pitchRotations;
        private NativeArray<UpdatingRotationData> rotationDataList;
        private PlayableAnimJob playableAnimJob;
        private AnimationScriptPlayable pitchUpdatePlayable;

        private void Awake()
        {
            Animator = GetComponentInParent<Animator>();
            CharacterEntity = GetComponentInParent<BaseCharacterEntity>();
            if (Animator == null)
                Animator = GetComponentInChildren<Animator>();
            playableCharacterModel = GetComponentInParent<PlayableCharacterModel>();
            if (playableCharacterModel != null)
            {
                forPlayableCharacterModel = true;
                // Insert animation job as output before default output
                playableAnimJob = new PlayableAnimJob();
                pitchUpdatePlayable = AnimationScriptPlayable.Create(playableCharacterModel.Graph, playableAnimJob, 1);
                PlayableOutput output = playableCharacterModel.Graph.GetOutput(0);
                playableCharacterModel.Graph.Connect(output.GetSourcePlayable(), 0, pitchUpdatePlayable, 0);
                pitchUpdatePlayable.SetInputWeight(0, 1);
                output.SetSourcePlayable(pitchUpdatePlayable);
            }
        }

        public bool Register(CharacterPitchIK comp)
        {
            if (!components.Contains(comp))
            {
                components.Add(comp);
                sizeChanged = true;
                return true;
            }
            return false;
        }

        public bool Unregister(CharacterPitchIK comp)
        {
            if (components.Remove(comp))
            {
                sizeChanged = true;
                return true;
            }
            return false;
        }

        private void Update()
        {
            if (forPlayableCharacterModel && sizeChanged)
            {
                playableAnimJob.DisposeArrays();
                enablings = new NativeArray<bool>(components.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                pitchBones = new NativeArray<TransformStreamHandle>(components.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                pitchRotations = new NativeArray<Quaternion>(components.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                rotationDataList = new NativeArray<UpdatingRotationData>(components.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            }
            for (int i = 0; i < components.Count; ++i)
            {
                if (forPlayableCharacterModel)
                {
                    enablings[i] = components[i].Enabling;
                    if (sizeChanged)
                        pitchBones[i] = Animator.BindStreamTransform(Animator.GetBoneTransform(components[i].pitchBone));
                    if (enablings[i])
                    {
                        // Change updating data
                        rotationDataList[i] = new UpdatingRotationData()
                        {
                            axis = components[i].axis,
                            rotateOffset = components[i].rotateOffset,
                            inversePitch = components[i].inversePitch,
                            lerpDamping = components[i].lerpDamping,
                            maxAngle = components[i].maxAngle,
                        };
                    }
                }
                else
                {
                    components[i].UpdatePitchRotation();
                }
            }
            sizeChanged = false;
            if (forPlayableCharacterModel)
            {
                playableAnimJob.characterPitch = CharacterEntity.Pitch;
                playableAnimJob.deltaTime = Time.deltaTime;
                playableAnimJob.enablings = enablings;
                playableAnimJob.pitchBones = pitchBones;
                playableAnimJob.pitchRotations = pitchRotations;
                playableAnimJob.rotationDataList = rotationDataList;
                pitchUpdatePlayable.SetJobData(playableAnimJob);
            }
        }

        private void OnDestroy()
        {
            playableAnimJob.DisposeArrays();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (forPlayableCharacterModel)
                return;
            for (int i = 0; i < components.Count; ++i)
            {
                if (!components[i].Enabling) continue;
                Animator.SetBoneLocalRotation(components[i].pitchBone, components[i].PitchRotation);
            }
        }

        public struct UpdatingRotationData
        {
            public CharacterPitchIK.Axis axis;
            public Vector3 rotateOffset;
            public bool inversePitch;
            public float lerpDamping;
            public float maxAngle;
        }

        public struct PlayableAnimJob : IAnimationJob
        {
            public float characterPitch;
            public float deltaTime;
            public NativeArray<bool> enablings;
            public NativeArray<TransformStreamHandle> pitchBones;
            public NativeArray<Quaternion> pitchRotations;
            public NativeArray<UpdatingRotationData> rotationDataList;

            public void ProcessAnimation(AnimationStream stream)
            {
                if (!enablings.IsCreated ||
                    !pitchBones.IsCreated ||
                    !pitchRotations.IsCreated ||
                    !rotationDataList.IsCreated)
                    return;
                for (int i = pitchBones.Length - 1; i >= 0; --i)
                {
                    if (!enablings[i]) continue;
                    pitchRotations[i] = CharacterPitchIK.CalculatePitchRotation(
                        characterPitch, deltaTime, pitchRotations[i],
                        rotationDataList[i].axis, rotationDataList[i].rotateOffset,
                        rotationDataList[i].inversePitch, rotationDataList[i].lerpDamping,
                        rotationDataList[i].maxAngle);
                    pitchBones[i].SetLocalRotation(stream, pitchRotations[i]);
                }
            }

            public void ProcessRootMotion(AnimationStream stream)
            {
            }

            public void DisposeArrays()
            {
                if (enablings.IsCreated)
                    enablings.Dispose();
                if (pitchBones.IsCreated)
                    pitchBones.Dispose();
                if (pitchRotations.IsCreated)
                    pitchRotations.Dispose();
                if (rotationDataList.IsCreated)
                    rotationDataList.Dispose();
            }
        }
    }
}
