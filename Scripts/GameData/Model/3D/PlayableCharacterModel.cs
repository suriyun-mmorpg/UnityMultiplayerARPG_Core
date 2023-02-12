using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

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
        [ArrayElementTitle("weaponType")]
        public WieldWeaponAnimations[] leftHandWieldingWeaponAnimations = new WieldWeaponAnimations[0];
        public WieldWeaponAnimations leftHandShieldAnimations = new WieldWeaponAnimations();
        [ArrayElementTitle("skill")]
        public SkillAnimations[] skillAnimations = new SkillAnimations[0];
        [ArrayElementTitle("clip")]
        public ActionState[] customAnimations = new ActionState[0];

        public PlayableGraph Graph { get; protected set; }
        public AnimationPlayableBehaviour Template { get; protected set; }
        public AnimationPlayableBehaviour Behaviour { get; protected set; }
        public float AwakenTime { get; protected set; }

        protected WeaponType equippedWeaponType = null;
        protected Coroutine actionCoroutine = null;
        protected bool isDoingAction = false;
        protected EquipWeapons oldEquipWeapons = null;
        protected System.Action onStopAction = null;

        protected override void Awake()
        {
            base.Awake();
            AwakenTime = Time.unscaledTime;
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
            Template = new AnimationPlayableBehaviour();
            Template.Setup(this);
            CreateGraph();
        }

        protected virtual void Start()
        {
            if (!IsMainModel)
                Graph.Stop();
        }

        public override void AddingNewModel(GameObject newModel, EquipmentContainer equipmentContainer)
        {
            base.AddingNewModel(newModel, equipmentContainer);
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
            Graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            ScriptPlayable<AnimationPlayableBehaviour> playable = ScriptPlayable<AnimationPlayableBehaviour>.Create(Graph, Template, 1);
            Behaviour = playable.GetBehaviour();
            AnimationPlayableOutput output = AnimationPlayableOutput.Create(Graph, "Output", animator);
            output.SetSourcePlayable(playable);
            Graph.Play();
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

        internal override void OnSwitchedToThisModel()
        {
            if (Graph.IsValid())
                Graph.Play();
        }

        private void OnDestroy()
        {
            DestroyGraph();
        }

        public bool TryGetWeaponAnimations(int dataId, out WeaponAnimations anims)
        {
            return CacheAnimationsManager.SetAndTryGetCacheWeaponAnimations(Id, weaponAnimations, skillAnimations, dataId, out anims);
        }

        public bool TryGetSkillAnimations(int dataId, out SkillAnimations anims)
        {
            return CacheAnimationsManager.SetAndTryGetCacheSkillAnimations(Id, weaponAnimations, skillAnimations, dataId, out anims);
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
                newEquipWeapons = new EquipWeapons();
            else
                newEquipWeapons = selectableWeaponSets[equipWeaponSet];
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
            if (Time.unscaledTime - AwakenTime < 1f || isDoingAction || !newEquipWeapons.IsDiffer(oldEquipWeapons, out bool rightIsDiffer, out bool leftIsDiffer))
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

        private IEnumerator PlayEquipWeaponsAnimationRoutine(EquipWeapons newEquipWeapons, bool rightIsDiffer, bool leftIsDiffer, IList<EquipWeapons> selectableWeaponSets, byte equipWeaponSet, bool isWeaponsSheathed)
        {
            isDoingAction = true;
            // Prepare states
            IWeaponItem tempWeaponItem;
            float holsteredDurationRate = 0f;
            ActionState holsterState = new ActionState();
            if (oldEquipWeapons != null)
            {
                if (rightIsDiffer)
                {
                    tempWeaponItem = oldEquipWeapons.GetRightHandWeaponItem();
                    if (tempWeaponItem != null && TryGetWeaponAnimations(tempWeaponItem.WeaponType.DataId, out WeaponAnimations anims) && anims.rightHandHolsterAnimation.holsterState.clip != null)
                    {
                        holsterState = anims.rightHandHolsterAnimation.holsterState;
                        holsteredDurationRate = anims.rightHandHolsterAnimation.holsteredDurationRate;
                    }
                    else
                    {
                        holsterState = defaultAnimations.rightHandHolsterAnimation.holsterState;
                        holsteredDurationRate = defaultAnimations.rightHandHolsterAnimation.holsteredDurationRate;
                    }
                }
                else if (leftIsDiffer)
                {
                    tempWeaponItem = oldEquipWeapons.GetLeftHandWeaponItem();
                    if (tempWeaponItem != null && TryGetWeaponAnimations(tempWeaponItem.WeaponType.DataId, out WeaponAnimations anims) && anims.leftHandHolsterAnimation.holsterState.clip != null)
                    {
                        holsterState = anims.leftHandHolsterAnimation.holsterState;
                        holsteredDurationRate = anims.leftHandHolsterAnimation.holsteredDurationRate;
                    }
                    else
                    {
                        holsterState = defaultAnimations.leftHandHolsterAnimation.holsterState;
                        holsteredDurationRate = defaultAnimations.leftHandHolsterAnimation.holsteredDurationRate;
                    }
                }
            }

            ActionState drawState = new ActionState();
            if (newEquipWeapons != null)
            {
                if (rightIsDiffer && !newEquipWeapons.IsEmptyRightHandSlot())
                {
                    tempWeaponItem = newEquipWeapons.GetRightHandWeaponItem();
                    if (tempWeaponItem != null && TryGetWeaponAnimations(tempWeaponItem.WeaponType.DataId, out WeaponAnimations anims) && anims.rightHandHolsterAnimation.drawState.clip != null)
                        drawState = anims.rightHandHolsterAnimation.drawState;
                    else
                        drawState = defaultAnimations.rightHandHolsterAnimation.drawState;
                }
                else if (leftIsDiffer && !newEquipWeapons.IsEmptyLeftHandSlot())
                {
                    tempWeaponItem = newEquipWeapons.GetLeftHandWeaponItem();
                    if (tempWeaponItem != null && TryGetWeaponAnimations(tempWeaponItem.WeaponType.DataId, out WeaponAnimations anims) && anims.leftHandHolsterAnimation.drawState.clip != null)
                        drawState = anims.leftHandHolsterAnimation.drawState;
                    else
                        drawState = defaultAnimations.leftHandHolsterAnimation.drawState;
                }
            }

            float holsteredDelay;
            float animationDelay;
            bool hasClip;

            // Play holster state
            holsteredDelay = 0f;
            animationDelay = 0f;
            hasClip = holsterState.clip != null;
            if (hasClip)
            {
                // Setup animation playing duration
                animationDelay = Behaviour.PlayAction(holsterState, 1f);
                holsteredDelay = animationDelay * holsteredDurationRate;
            }

            if (holsteredDelay > 0f)
            {
                // Wait by holstering duration
                yield return new WaitForSecondsRealtime(holsteredDelay);
            }

            // Switch weapon items
            SetNewEquipWeapons(newEquipWeapons, selectableWeaponSets, equipWeaponSet, isWeaponsSheathed);
            onStopAction = null;

            if (animationDelay - holsteredDelay > 0f)
            {
                // Wait by animation playing duration
                yield return new WaitForSecondsRealtime(animationDelay);
            }

            // Play draw state
            animationDelay = 0f;
            hasClip = drawState.clip != null;
            if (hasClip)
            {
                // Setup animation playing duration
                animationDelay = Behaviour.PlayAction(drawState, 1f);
            }

            if (animationDelay > 0f)
            {
                // Wait by animation playing duration
                yield return new WaitForSecondsRealtime(animationDelay);
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
                Behaviour.IsFreeze = isFreezeAnimation;
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
            Behaviour.PlayAction(customAnimations[id], 1f);
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
