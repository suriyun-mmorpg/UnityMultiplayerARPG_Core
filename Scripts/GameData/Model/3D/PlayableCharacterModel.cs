using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace MultiplayerARPG.GameData.Model.Playables
{
    public partial class PlayableCharacterModel : BaseCharacterModel, ICustomAnimationModel
    {
        [Header("Relates Components")]
        [Tooltip("It will find `Animator` component on automatically if this is NULL")]
        public Animator animator;

        [Header("Renderer")]
        [Tooltip("This will be used to apply bone weights when equip an equipments")]
        public SkinnedMeshRenderer skinnedMeshRenderer;

        [Header("Animations")]
        [Tooltip("If `avatarMask` in action state settings is `null`, it will use this value")]
        public AvatarMask actionAvatarMask;
        [Tooltip("If `transitionDuration` in state settings is <= 0, it will use this value")]
        public float transitionDuration = 0.1f;
        public DefaultAnimations defaultAnimations;
        [Tooltip("Default animations will be overrided by these animations while wielding weapon with the same type")]
        [ArrayElementTitle("weaponType")]
        public WeaponAnimations[] weaponAnimations = new WeaponAnimations[0];
        [Tooltip("Weapon animations will be overrided by these animations while wielding weapon with the same type at left-hand")]
        [FormerlySerializedAs("leftHandWieldingWeaponAnimations")]
        [ArrayElementTitle("weaponType")]
        public WieldWeaponAnimations[] leftHandWeaponAnimations = new WieldWeaponAnimations[0];
        [Tooltip("Don't have to set `weaponType` data, this is for shield")]
        public WieldWeaponAnimations leftHandShieldAnimations = new WieldWeaponAnimations();
        [ArrayElementTitle("skill")]
        public SkillAnimations[] skillAnimations = new SkillAnimations[0];
        [ArrayElementTitle("clip")]
        public ActionState[] customAnimations = new ActionState[0];

        public PlayableGraph Graph { get; protected set; }
        public AnimationPlayableBehaviour Template { get; protected set; }
        public AnimationPlayableBehaviour Behaviour { get; protected set; }
        public float SwitchedTime { get; protected set; }

        protected WeaponType equippedWeaponType = null;
        protected Coroutine actionCoroutine = null;
        protected bool isDoingAction = false;
        protected EquipWeapons oldEquipWeapons = null;
        protected System.Action onStopAction = null;

        protected override void Awake()
        {
            base.Awake();
            SwitchedTime = Time.unscaledTime;
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            Template = new AnimationPlayableBehaviour();
            Template.Setup(this);
            CreateGraph();
        }

        protected virtual void Start()
        {
            if (IsActiveModel)
            {
                Graph.Play();
                Graph.Evaluate(Time.deltaTime);
            }
            CacheEntity.onIsUpdateEntityComponentsChanged += CacheEntity_onUpdateEntityComponentsChanged;
        }

        private void CacheEntity_onUpdateEntityComponentsChanged(bool isUpdate)
        {
            if (!isUpdate)
            {
                if (Graph.IsPlaying())
                    Graph.Stop();
            }
            else
            {
                if (IsActiveModel && !Graph.IsPlaying())
                    Graph.Play();
            }
        }

        public override void UpdateAnimation(float deltaTime)
        {
            if (Graph.IsValid())
                Graph.Evaluate(deltaTime);
        }

        public override void AddingNewModel(EquipmentModel data, GameObject newModel, EquipmentContainer equipmentContainer)
        {
            base.AddingNewModel(data, newModel, equipmentContainer);
            if (data.doNotUseEntityBones)
                return;
            SkinnedMeshRenderer skinnedMesh = newModel.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skinnedMesh != null && skinnedMeshRenderer != null)
            {
                skinnedMesh.bones = skinnedMeshRenderer.bones;
                skinnedMesh.rootBone = skinnedMeshRenderer.rootBone;
                if (equipmentContainer.defaultModel != null)
                {
                    SkinnedMeshRenderer defaultSkinnedMesh = equipmentContainer.defaultModel.GetComponentInChildren<SkinnedMeshRenderer>();
                    if (defaultSkinnedMesh != null)
                    {
                        skinnedMesh.bones = defaultSkinnedMesh.bones;
                        skinnedMesh.rootBone = defaultSkinnedMesh.rootBone;
                    }
                }
            }
        }

        protected void CreateGraph()
        {
            Graph = PlayableGraph.Create($"{name}.PlayableCharacterModel");
            Graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            ScriptPlayable<AnimationPlayableBehaviour> playable = ScriptPlayable<AnimationPlayableBehaviour>.Create(Graph, Template, 1);
            Behaviour = playable.GetBehaviour();
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(Graph, "Output", animator);
            output.SetSourcePlayable(playable);
        }

        protected void DestroyGraph()
        {
            if (Graph.IsValid())
                Graph.Destroy();
        }

        internal override void OnSwitchingToAnotherModel()
        {
            if (Graph.IsValid())
                Graph.Stop();
        }

        internal override void OnSwitchingToThisModel()
        {
            SwitchedTime = Time.unscaledTime;
        }

        internal override void OnSwitchedToThisModel()
        {
            if (Graph.IsValid())
                Graph.Play();
        }

        private void OnDestroy()
        {
            CacheEntity.onIsUpdateEntityComponentsChanged -= CacheEntity_onUpdateEntityComponentsChanged;
            DestroyGraph();
        }

        public bool TryGetWeaponAnimations(int dataId, out WeaponAnimations anims)
        {
            if (CacheAnimationsManager.SetAndTryGetCacheWeaponAnimations(Id, weaponAnimations, skillAnimations, dataId, out anims))
            {
                return true;
            }
            if (GameInstance.WeaponTypes.TryGetValue(dataId, out WeaponType weaponType) && weaponType.PlayableCharacterModelSettings.applyWeaponAnimations)
            {
                anims = weaponType.PlayableCharacterModelSettings.weaponAnimations;
                anims.weaponType = weaponType;
                return true;
            }
            return false;
        }

        public bool TryGetSkillAnimations(int dataId, out SkillAnimations anims)
        {
            if (CacheAnimationsManager.SetAndTryGetCacheSkillAnimations(Id, weaponAnimations, skillAnimations, dataId, out anims))
            {
                return true;
            }
            if (GameInstance.Skills.TryGetValue(dataId, out BaseSkill skill) && skill.PlayableCharacterModelSettings.applySkillAnimations)
            {
                anims = skill.PlayableCharacterModelSettings.skillAnimations;
                anims.skill = skill;
                return true;
            }
            return false;
        }

        public ActionAnimation GetActionAnimation(AnimActionType animActionType, int dataId, int index)
        {
            ActionAnimation tempActionAnimation = default;
            switch (animActionType)
            {
                case AnimActionType.AttackRightHand:
                    ActionAnimation[] rightHandAnims = GetRightHandAttackAnimations(dataId);
                    if (index >= rightHandAnims.Length)
                        index = 0;
                    if (index < rightHandAnims.Length)
                        tempActionAnimation = rightHandAnims[index];
                    break;
                case AnimActionType.AttackLeftHand:
                    ActionAnimation[] leftHandAnims = GetLeftHandAttackAnimations(dataId);
                    if (index >= leftHandAnims.Length)
                        index = 0;
                    if (index < leftHandAnims.Length)
                        tempActionAnimation = leftHandAnims[index];
                    break;
                case AnimActionType.SkillRightHand:
                case AnimActionType.SkillLeftHand:
                    tempActionAnimation = GetSkillActivateAnimation(dataId);
                    break;
                case AnimActionType.ReloadRightHand:
                    tempActionAnimation = GetRightHandReloadAnimation(dataId);
                    break;
                case AnimActionType.ReloadLeftHand:
                    tempActionAnimation = GetLeftHandReloadAnimation(dataId);
                    break;
            }
            return tempActionAnimation;
        }

        public override void SetEquipWeapons(IList<EquipWeapons> selectableWeaponSets, byte equipWeaponSet, bool isWeaponsSheathed)
        {
            EquipWeapons newEquipWeapons;
            if (isWeaponsSheathed || selectableWeaponSets == null || selectableWeaponSets.Count == 0)
            {
                newEquipWeapons = new EquipWeapons();
            }
            else
            {
                if (equipWeaponSet >= selectableWeaponSets.Count)
                {
                    // Issues occuring, so try to simulate data
                    // Create a new list to make sure that changes won't be applied to the source list (the source list must be readonly)
                    selectableWeaponSets = new List<EquipWeapons>(selectableWeaponSets);
                    while (equipWeaponSet >= selectableWeaponSets.Count)
                    {
                        selectableWeaponSets.Add(new EquipWeapons());
                    }
                }
                newEquipWeapons = selectableWeaponSets[equipWeaponSet];
            }
            // Get one equipped weapon from right-hand or left-hand
            IWeaponItem rightWeaponItem = newEquipWeapons.GetRightHandWeaponItem();
            IWeaponItem leftWeaponItem = newEquipWeapons.GetLeftHandWeaponItem();
            if (rightWeaponItem == null)
                rightWeaponItem = leftWeaponItem;
            // Set equipped weapon type, it will be used to get animations by id
            equippedWeaponType = null;
            if (rightWeaponItem != null)
                equippedWeaponType = rightWeaponItem.WeaponType;
            if (Behaviour != null)
                Behaviour.SetEquipWeapons(rightWeaponItem, leftWeaponItem, newEquipWeapons.GetLeftHandShieldItem());
            // Player draw/holster animation
            if (oldEquipWeapons == null)
                oldEquipWeapons = newEquipWeapons;
            if (Time.unscaledTime - SwitchedTime < 1f || !newEquipWeapons.IsDiffer(oldEquipWeapons, out bool rightIsDiffer, out bool leftIsDiffer))
            {
                SetNewEquipWeapons(newEquipWeapons, selectableWeaponSets, equipWeaponSet, isWeaponsSheathed);
                return;
            }
            StartActionCoroutine(PlayEquipWeaponsAnimationRoutine(newEquipWeapons, rightIsDiffer, leftIsDiffer, selectableWeaponSets, equipWeaponSet, isWeaponsSheathed), () => SetNewEquipWeapons(newEquipWeapons, selectableWeaponSets, equipWeaponSet, isWeaponsSheathed));
        }

        private void SetNewEquipWeapons(EquipWeapons newEquipWeapons, IList<EquipWeapons> selectableWeaponSets, byte equipWeaponSet, bool isWeaponsSheathed)
        {
            if (newEquipWeapons != null)
                oldEquipWeapons = newEquipWeapons.Clone();
            base.SetEquipWeapons(selectableWeaponSets, equipWeaponSet, isWeaponsSheathed);
        }

        public void GetRightHandSheathActionState(EquipWeapons oldEquipWeapons, out ActionState actionState, out float triggeredDurationRate)
        {
            IWeaponItem tempWeaponItem = oldEquipWeapons.GetRightHandWeaponItem();
            if (tempWeaponItem != null && TryGetWeaponAnimations(tempWeaponItem.WeaponType.DataId, out WeaponAnimations anims) && anims.rightHandWeaponSheathingAnimation.sheathState.clip != null)
            {
                actionState = anims.rightHandWeaponSheathingAnimation.sheathState;
                triggeredDurationRate = anims.rightHandWeaponSheathingAnimation.sheathedDurationRate;
            }
            else
            {
                actionState = defaultAnimations.rightHandWeaponSheathingAnimation.sheathState;
                triggeredDurationRate = defaultAnimations.rightHandWeaponSheathingAnimation.sheathedDurationRate;
            }
        }

        public void GetLeftHandSheathActionState(EquipWeapons oldEquipWeapons, out ActionState actionState, out float triggeredDurationRate)
        {
            IWeaponItem tempWeaponItem = oldEquipWeapons.GetLeftHandWeaponItem();
            IShieldItem tempShieldItem = oldEquipWeapons.GetLeftHandShieldItem();
            if (tempWeaponItem != null && TryGetWeaponAnimations(tempWeaponItem.WeaponType.DataId, out WeaponAnimations anims) && anims.leftHandWeaponSheathingAnimation.sheathState.clip != null)
            {
                actionState = anims.leftHandWeaponSheathingAnimation.sheathState;
                triggeredDurationRate = anims.leftHandWeaponSheathingAnimation.sheathedDurationRate;
            }
            else if (tempShieldItem != null)
            {
                actionState = defaultAnimations.leftHandShieldSheathingAnimation.sheathState;
                triggeredDurationRate = defaultAnimations.leftHandShieldSheathingAnimation.sheathedDurationRate;
            }
            else
            {
                actionState = defaultAnimations.leftHandWeaponSheathingAnimation.sheathState;
                triggeredDurationRate = defaultAnimations.leftHandWeaponSheathingAnimation.sheathedDurationRate;
            }
        }

        public void GetRightHandUnsheathActionState(EquipWeapons newEquipWeapons, out ActionState actionState, out float triggeredDurationRate)
        {
            IWeaponItem tempWeaponItem = newEquipWeapons.GetRightHandWeaponItem();
            if (tempWeaponItem != null && TryGetWeaponAnimations(tempWeaponItem.WeaponType.DataId, out WeaponAnimations anims) && anims.rightHandWeaponSheathingAnimation.unsheathState.clip != null)
            {
                actionState = anims.rightHandWeaponSheathingAnimation.unsheathState;
                triggeredDurationRate = anims.rightHandWeaponSheathingAnimation.unsheathedDurationRate;
            }
            else
            {
                actionState = defaultAnimations.rightHandWeaponSheathingAnimation.unsheathState;
                triggeredDurationRate = defaultAnimations.rightHandWeaponSheathingAnimation.unsheathedDurationRate;
            }
        }

        public void GetLeftHandUnsheathActionState(EquipWeapons newEquipWeapons, out ActionState actionState, out float triggeredDurationRate)
        {
            IWeaponItem tempWeaponItem = oldEquipWeapons.GetLeftHandWeaponItem();
            IShieldItem tempShieldItem = oldEquipWeapons.GetLeftHandShieldItem();
            if (tempWeaponItem != null && TryGetWeaponAnimations(tempWeaponItem.WeaponType.DataId, out WeaponAnimations anims) && anims.leftHandWeaponSheathingAnimation.sheathState.clip != null)
            {
                actionState = anims.leftHandWeaponSheathingAnimation.unsheathState;
                triggeredDurationRate = anims.leftHandWeaponSheathingAnimation.unsheathedDurationRate;
            }
            else if (tempShieldItem != null)
            {
                actionState = defaultAnimations.leftHandShieldSheathingAnimation.unsheathState;
                triggeredDurationRate = defaultAnimations.leftHandShieldSheathingAnimation.unsheathedDurationRate;
            }
            else
            {
                actionState = defaultAnimations.leftHandWeaponSheathingAnimation.unsheathState;
                triggeredDurationRate = defaultAnimations.leftHandWeaponSheathingAnimation.unsheathedDurationRate;
            }
        }

        private IEnumerator PlayEquipWeaponsAnimationRoutine(EquipWeapons newEquipWeapons, bool rightIsDiffer, bool leftIsDiffer, IList<EquipWeapons> selectableWeaponSets, byte equipWeaponSet, bool isWeaponsSheathed)
        {
            isDoingAction = true;

            ActionState actionState = new ActionState();
            float triggeredDurationRate = 0f;
            if (isWeaponsSheathed)
            {
                if (oldEquipWeapons != null)
                {
                    if (rightIsDiffer)
                    {
                        GetRightHandSheathActionState(oldEquipWeapons, out actionState, out triggeredDurationRate);
                    }
                    else if (leftIsDiffer)
                    {
                        GetLeftHandSheathActionState(oldEquipWeapons, out actionState, out triggeredDurationRate);
                    }
                }
            }
            else
            {
                if (newEquipWeapons != null)
                {
                    if (rightIsDiffer)
                    {
                        if (newEquipWeapons.IsEmptyRightHandSlot())
                            GetRightHandSheathActionState(oldEquipWeapons, out actionState, out triggeredDurationRate);
                        else
                            GetRightHandUnsheathActionState(newEquipWeapons, out actionState, out triggeredDurationRate);
                    }
                    else if (leftIsDiffer)
                    {
                        if (newEquipWeapons.IsEmptyLeftHandSlot())
                            GetLeftHandSheathActionState(oldEquipWeapons, out actionState, out triggeredDurationRate);
                        else
                            GetLeftHandUnsheathActionState(newEquipWeapons, out actionState, out triggeredDurationRate);
                    }
                }
            }

            float animationDelay = 0f;
            float triggeredDelay = 0f;
            if (actionState.clip != null)
            {
                // Setup animation playing duration
                animationDelay = Behaviour.PlayAction(actionState, 1f);
                triggeredDelay = animationDelay * triggeredDurationRate;
            }

            if (triggeredDelay > 0f)
                yield return new WaitForSecondsRealtime(triggeredDelay);

            SetNewEquipWeapons(newEquipWeapons, selectableWeaponSets, equipWeaponSet, isWeaponsSheathed);
            onStopAction = null;

            if (animationDelay - triggeredDelay > 0f)
            {
                // Wait by remaining animation playing duration
                yield return new WaitForSecondsRealtime(animationDelay - triggeredDelay);
            }

            isDoingAction = false;
        }


        #region Right-hand animations
        public ActionAnimation[] GetRightHandAttackAnimations(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.rightHandAttackAnimations != null)
                return anims.rightHandAttackAnimations;
            return defaultAnimations.rightHandAttackAnimations;
        }

        public ActionAnimation GetRightHandReloadAnimation(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.rightHandReloadAnimation.state.clip != null)
                return anims.rightHandReloadAnimation;
            return defaultAnimations.rightHandReloadAnimation;
        }

        public override bool GetRightHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetRightHandAttackAnimations(dataId);
            animSpeedRate = 1f;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0 || animationIndex >= tempActionAnimations.Length) return false;
            animSpeedRate = tempActionAnimations[animationIndex].GetAnimSpeedRate();
            triggerDurations = tempActionAnimations[animationIndex].GetTriggerDurations();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetRightHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetRightHandReloadAnimation(dataId);
            animSpeedRate = tempActionAnimation.GetAnimSpeedRate();
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetRandomRightHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animationIndex = GenericUtils.RandomInt(randomSeed, 0, GetRightHandAttackAnimations(dataId).Length);
            return GetRightHandAttackAnimation(dataId, animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
        }

        public override int GetRightHandAttackRandomMax(int dataId)
        {
            return GetRightHandAttackAnimations(dataId).Length;
        }
        #endregion

        #region Left-hand animations
        public ActionAnimation[] GetLeftHandAttackAnimations(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.leftHandAttackAnimations != null)
                return anims.leftHandAttackAnimations;
            return defaultAnimations.leftHandAttackAnimations;
        }

        public ActionAnimation GetLeftHandReloadAnimation(int dataId)
        {
            WeaponAnimations anims;
            if (TryGetWeaponAnimations(dataId, out anims) && anims.leftHandReloadAnimation.state.clip != null)
                return anims.leftHandReloadAnimation;
            return defaultAnimations.leftHandReloadAnimation;
        }

        public override bool GetLeftHandAttackAnimation(int dataId, int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation[] tempActionAnimations = GetLeftHandAttackAnimations(dataId);
            animSpeedRate = 1f;
            triggerDurations = new float[] { 0f };
            totalDuration = 0f;
            if (tempActionAnimations.Length == 0 || animationIndex >= tempActionAnimations.Length) return false;
            animSpeedRate = tempActionAnimations[animationIndex].GetAnimSpeedRate();
            triggerDurations = tempActionAnimations[animationIndex].GetTriggerDurations();
            totalDuration = tempActionAnimations[animationIndex].GetTotalDuration();
            return true;
        }

        public override bool GetLeftHandReloadAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetLeftHandReloadAnimation(dataId);
            animSpeedRate = tempActionAnimation.GetAnimSpeedRate();
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override bool GetRandomLeftHandAttackAnimation(int dataId, int randomSeed, out int animationIndex, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            animationIndex = GenericUtils.RandomInt(randomSeed, 0, GetLeftHandAttackAnimations(dataId).Length);
            return GetLeftHandAttackAnimation(dataId, animationIndex, out animSpeedRate, out triggerDurations, out totalDuration);
        }

        public override int GetLeftHandAttackRandomMax(int dataId)
        {
            return GetLeftHandAttackAnimations(dataId).Length;
        }
        #endregion

        #region Skill animations
        public ActionAnimation GetSkillActivateAnimation(int dataId)
        {
            SkillAnimations anims;
            if (TryGetSkillAnimations(dataId, out anims) && anims.activateAnimation.state.clip != null)
                return anims.activateAnimation;
            return defaultAnimations.skillActivateAnimation;
        }

        public ActionState GetSkillCastState(int dataId)
        {
            SkillAnimations anims;
            if (TryGetSkillAnimations(dataId, out anims) && anims.castState.clip != null)
                return anims.castState;
            return defaultAnimations.skillCastState;
        }

        public override bool GetSkillActivateAnimation(int dataId, out float animSpeedRate, out float[] triggerDurations, out float totalDuration)
        {
            ActionAnimation tempActionAnimation = GetSkillActivateAnimation(dataId);
            animSpeedRate = tempActionAnimation.GetAnimSpeedRate();
            triggerDurations = tempActionAnimation.GetTriggerDurations();
            totalDuration = tempActionAnimation.GetTotalDuration();
            return true;
        }

        public override SkillActivateAnimationType GetSkillActivateAnimationType(int dataId)
        {
            SkillAnimations anims;
            if (!TryGetSkillAnimations(dataId, out anims))
                return SkillActivateAnimationType.UseActivateAnimation;
            return anims.activateAnimationType;
        }

        public override void PlaySkillCastClip(int dataId, float duration)
        {
            StartActionCoroutine(PlaySkillCastClipRoutine(GetSkillCastState(dataId), duration));
        }

        private IEnumerator PlaySkillCastClipRoutine(ActionState castState, float duration)
        {
            isDoingAction = true;
            // Waits by skill cast duration
            yield return new WaitForSecondsRealtime(Behaviour.PlayAction(castState, 1f, duration));
            isDoingAction = false;
        }

        public override void StopSkillCastAnimation()
        {
            Behaviour.StopAction();
            isDoingAction = false;
        }
        #endregion

        #region Action animations
        public override void PlayActionAnimation(AnimActionType animActionType, int dataId, int index, float playSpeedMultiplier = 1)
        {
            StartActionCoroutine(PlayActionAnimationRoutine(GetActionAnimation(animActionType, dataId, index), playSpeedMultiplier));
        }

        private IEnumerator PlayActionAnimationRoutine(ActionAnimation actionAnimation, float playSpeedMultiplier)
        {
            isDoingAction = true;
            AudioManager.PlaySfxClipAtAudioSource(actionAnimation.GetRandomAudioClip(), GenericAudioSource);
            // Wait by animation playing duration
            yield return new WaitForSecondsRealtime(Behaviour.PlayAction(actionAnimation.state, playSpeedMultiplier));
            // Waits by current transition + extra duration before end playing animation state
            yield return new WaitForSecondsRealtime(actionAnimation.GetExtendDuration() / playSpeedMultiplier);
            isDoingAction = false;
        }

        public override void StopActionAnimation()
        {
            Behaviour.StopAction();
            isDoingAction = false;
        }
        #endregion

        #region Weapon charge animations
        public override void PlayWeaponChargeClip(int dataId, bool isLeftHand)
        {
            isDoingAction = true;
            WeaponAnimations weaponAnimations;
            if (TryGetWeaponAnimations(dataId, out weaponAnimations))
            {
                if (isLeftHand && weaponAnimations.leftHandChargeState.clip != null)
                {
                    Behaviour.PlayAction(weaponAnimations.leftHandChargeState, 1f, loop: true);
                    return;
                }
                if (!isLeftHand && weaponAnimations.rightHandChargeState.clip != null)
                {
                    Behaviour.PlayAction(weaponAnimations.rightHandChargeState, 1f, loop: true);
                    return;
                }
            }
            if (isLeftHand)
                Behaviour.PlayAction(defaultAnimations.leftHandChargeState, 1f, loop: true);
            else
                Behaviour.PlayAction(defaultAnimations.rightHandChargeState, 1f, loop: true);
        }

        public override void StopWeaponChargeAnimation()
        {
            Behaviour.StopAction();
            isDoingAction = false;
        }
        #endregion

        #region Other animations
        public override void PlayMoveAnimation()
        {
            // Do nothing, animation playable behaviour will do it
            if (Behaviour != null)
                Behaviour.IsFreeze = IsFreezeAnimation;
        }

        public override void PlayHitAnimation()
        {
            if (isDoingAction)
                return;
            WeaponAnimations weaponAnimations;
            if (equippedWeaponType != null && TryGetWeaponAnimations(equippedWeaponType.DataId, out weaponAnimations) && weaponAnimations.hurtState.clip != null)
            {
                Behaviour.PlayAction(weaponAnimations.hurtState, 1f);
                return;
            }
            if (defaultAnimations.hurtState.clip != null)
                Behaviour.PlayAction(defaultAnimations.hurtState, 1f);
        }

        public override void PlayJumpAnimation()
        {
            Behaviour.PlayJump();
        }

        public override void PlayPickupAnimation()
        {
            if (isDoingAction)
                return;
            WeaponAnimations weaponAnimations;
            if (equippedWeaponType != null && TryGetWeaponAnimations(equippedWeaponType.DataId, out weaponAnimations) && weaponAnimations.pickupState.clip != null)
            {
                Behaviour.PlayAction(weaponAnimations.pickupState, 1f);
                return;
            }
            if (defaultAnimations.pickupState.clip != null)
                Behaviour.PlayAction(defaultAnimations.pickupState, 1f);
        }

        public void PlayCustomAnimation(int id)
        {
            if (id < 0 || id >= customAnimations.Length)
                return;
            if (isDoingAction)
                return;
            if (customAnimations[id].clip != null)
                Behaviour.PlayAction(customAnimations[id], 1f);
        }

        public AnimationClip GetCustomAnimationClip(int id)
        {
            if (id < 0 || id >= customAnimations.Length)
                return null;
            if (customAnimations[id].clip == null)
                return null;
            return customAnimations[id].clip;
        }
        #endregion

        protected Coroutine StartActionCoroutine(IEnumerator routine, System.Action onStopAction = null)
        {
            StopActionCoroutine();
            isDoingAction = true;
            actionCoroutine = StartCoroutine(routine);
            this.onStopAction = onStopAction;
            return actionCoroutine;
        }

        protected void StopActionCoroutine()
        {
            if (isDoingAction)
            {
                if (onStopAction != null)
                    onStopAction.Invoke();
            }
            if (actionCoroutine != null)
                StopCoroutine(actionCoroutine);
            actionCoroutine = null;
            isDoingAction = false;
            onStopAction = null;
        }
    }
}
