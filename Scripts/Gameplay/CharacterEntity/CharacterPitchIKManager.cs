using MultiplayerARPG.GameData.Model.Playables;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MultiplayerARPG
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(1)]
    public class CharacterPitchIKManager : MonoBehaviour
    {
        public Animator Animator { get; private set; }
        public BaseCharacterEntity CharacterEntity { get; private set; }
        private readonly List<CharacterPitchIK> components = new List<CharacterPitchIK>();
        private PlayableCharacterModel playableCharacterModel;
        private bool forPlayableCharacterModel;
        public bool sizeChanging;
        public bool sizeChanged;
        private bool arraysCreatedOnce;
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
                forPlayableCharacterModel = true;
        }

        private void Start()
        {
            if (forPlayableCharacterModel)
            {
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
                sizeChanging = true;
                return true;
            }
            return false;
        }

        public bool Unregister(CharacterPitchIK comp)
        {
            if (components.Remove(comp))
            {
                sizeChanging = true;
                return true;
            }
            return false;
        }

        private void Update()
        {
            for (int i = 0; i < components.Count; ++i)
            {

                if (!forPlayableCharacterModel)
                {
                    components[i].UpdatePitchRotation();
                }
                else if (arraysCreatedOnce)
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
            }
            sizeChanged = false;
            if (forPlayableCharacterModel && arraysCreatedOnce)
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

        private void LateUpdate()
        {
            if (forPlayableCharacterModel && sizeChanging)
            {
                DisposePlayableAnimArrays();
                enablings = new NativeArray<bool>(components.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                pitchBones = new NativeArray<TransformStreamHandle>(components.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                pitchRotations = new NativeArray<Quaternion>(components.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                rotationDataList = new NativeArray<UpdatingRotationData>(components.Count, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
                sizeChanging = false;
                sizeChanged = true;
                arraysCreatedOnce = true;
            }
        }

        private void OnDestroy()
        {
            DisposePlayableAnimArrays();
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

        private void DisposePlayableAnimArrays()
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
        }
    }
}
