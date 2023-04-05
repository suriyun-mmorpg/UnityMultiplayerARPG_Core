using Playables = MultiplayerARPG.GameData.Model.Playables;
using UnityEditor;
using UnityEngine;

namespace MultiplayerARPG
{
    public class PlayableCharacterModelTestWindow : EditorWindow
    {
        protected GameObject go;
        protected AnimationClip animationClip;
        protected float time = 0.0f;
        protected bool lockSelection = false;
        protected bool animationMode = false;
        protected bool isGrounded = false;
        protected bool jump = false;
        protected bool landed = false;
        protected bool moveForward = false;
        protected bool moveBackward = false;
        protected bool moveRight = false;
        protected bool moveLeft = false;
        protected bool sprint = false;
        protected bool walk = false;
        protected bool crouch = false;
        protected bool crawl = false;
        protected bool swim = false;
        protected bool hurt = false;
        protected bool dead = false;
        protected bool pickup = false;
        protected bool attackRight = false;
        protected bool attackLeft = false;
        protected int attackIndex = 0;
        protected WeaponType weaponType;
        protected Playables.PlayableCharacterModel model;

        [MenuItem("MMORPG KIT/Test Playable Character Model Animation", false, 10100)]
        public static void CreateNewEditor()
        {
            PlayableCharacterModelTestWindow window = GetWindowWithRect<PlayableCharacterModelTestWindow>(new Rect(0, 0, 300, 500));
            window.Show();
        }

        public void OnSelectionChange()
        {
            if (!lockSelection)
            {
                go = null;
                GameObject activeGameObject = Selection.activeGameObject;
                if (activeGameObject != null && activeGameObject.TryGetComponent(out model) && model != null && model.animator != null)
                {
                    go = model.animator.gameObject;
                    animationClip = model.defaultAnimations.idleState.clip;
                }
                Repaint();
            }
        }

        // Main editor window
        public void OnGUI()
        {
            // Wait for user to select a GameObject
            if (go == null)
            {
                EditorGUILayout.HelpBox("Please select a entity", MessageType.Info);
                return;
            }

            // Animate and Lock buttons.  Check if Animate has changed
            GUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            GUILayout.Toggle(AnimationMode.InAnimationMode(), "Animate");
            if (EditorGUI.EndChangeCheck())
                ToggleAnimationMode();

            GUILayout.FlexibleSpace();
            lockSelection = GUILayout.Toggle(lockSelection, "Lock");
            GUILayout.EndHorizontal();

            // Slider to use when Animate has been ticked
            EditorGUILayout.BeginVertical();

            weaponType = EditorGUILayout.ObjectField(weaponType, typeof(WeaponType), false) as WeaponType;

            GUILayout.BeginHorizontal();
            isGrounded = GUILayout.Toggle(isGrounded, "Is Grounded");
            if (isGrounded)
            {
                jump = false;
                landed = false;
            }
            jump = GUILayout.Toggle(jump, "Jump");
            if (jump)
            {
                isGrounded = false;
                landed = false;
            }
            landed = GUILayout.Toggle(landed, "Landed");
            if (landed)
            {
                isGrounded = false;
                jump = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            moveForward = GUILayout.Toggle(moveForward, "Move Forward");
            if (moveForward)
                moveBackward = false;
            moveBackward = GUILayout.Toggle(moveBackward, "Move Backward");
            if (moveBackward)
                moveForward = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            moveRight = GUILayout.Toggle(moveRight, "Move Right");
            if (moveRight)
                moveLeft = false;
            moveLeft = GUILayout.Toggle(moveLeft, "Move Left");
            if (moveLeft)
                moveRight = false;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            sprint = GUILayout.Toggle(sprint, "Sprint");
            if (sprint)
            {
                walk = false;
                crouch = false;
                crawl = false;
                swim = false;
            }
            walk = GUILayout.Toggle(walk, "Walk");
            if (walk)
            {
                sprint = false;
                crouch = false;
                crawl = false;
                swim = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            crouch = GUILayout.Toggle(crouch, "Crouch");
            if (crouch)
            {
                sprint = false;
                walk = false;
                crawl = false;
                swim = false;
            }
            crawl = GUILayout.Toggle(crawl, "Crawl");
            if (crawl)
            {
                sprint = false;
                walk = false;
                crouch = false;
                swim = false;
            }
            swim = GUILayout.Toggle(swim, "Swim");
            if (swim)
            {
                sprint = false;
                walk = false;
                crouch = false;
                crawl = false;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            hurt = GUILayout.Toggle(hurt, "Hurt");
            if (hurt)
            {
                dead = false;
                pickup = false;
                attackLeft = false;
                attackRight = false;
            }
            dead = GUILayout.Toggle(dead, "Dead");
            if (dead)
            {
                hurt = false;
                pickup = false;
                attackLeft = false;
                attackRight = false;
            }
            pickup = GUILayout.Toggle(pickup, "Pickup");
            if (pickup)
            {
                hurt = false;
                dead = false;
                attackLeft = false;
                attackRight = false;
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            attackLeft = GUILayout.Toggle(attackLeft, "Attack Left");
            if (attackLeft)
            {
                hurt = false;
                dead = false;
                pickup = false;
                attackRight = false;
            }
            attackRight = GUILayout.Toggle(attackRight, "Attack Right");
            if (attackRight)
            {
                hurt = false;
                dead = false;
                pickup = false;
                attackLeft = false;
            }
            GUILayout.EndHorizontal();
            attackIndex = EditorGUILayout.IntField("Attack Index", attackIndex);

            if (isGrounded)
            {
                if (swim)
                {
                    if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                        SetMovementClip(anims.swimIdleState, anims.swimMoveStates);
                    else
                        SetMovementClip(model.defaultAnimations.swimIdleState, model.defaultAnimations.swimMoveStates);
                }
                else if (crawl)
                {
                    if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                        SetMovementClip(anims.crawlIdleState, anims.crawlMoveStates);
                    else
                        SetMovementClip(model.defaultAnimations.crawlIdleState, model.defaultAnimations.crawlMoveStates);
                }
                else if (crouch)
                {
                    if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                        SetMovementClip(anims.crouchIdleState, anims.crouchMoveStates);
                    else
                        SetMovementClip(model.defaultAnimations.crouchIdleState, model.defaultAnimations.crouchMoveStates);
                }
                else if (walk)
                {
                    if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                        SetMovementClip(anims.idleState, anims.walkStates);
                    else
                        SetMovementClip(model.defaultAnimations.idleState, model.defaultAnimations.walkStates);
                }
                else if (sprint)
                {
                    if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                        SetMovementClip(anims.idleState, anims.sprintStates);
                    else
                        SetMovementClip(model.defaultAnimations.idleState, model.defaultAnimations.sprintStates);
                }
                else
                {
                    if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                        SetMovementClip(anims.idleState, anims.moveStates);
                    else
                        SetMovementClip(model.defaultAnimations.idleState, model.defaultAnimations.moveStates);
                }
            }
            else if (jump)
            {
                if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                    animationClip = anims.jumpState.clip;
                else
                    animationClip = model.defaultAnimations.jumpState.clip;
            }
            else if (landed)
            {
                if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                    animationClip = anims.landedState.clip;
                else
                    animationClip = model.defaultAnimations.landedState.clip;
            }
            else if (hurt)
            {
                if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                    animationClip = anims.hurtState.clip;
                else
                    animationClip = model.defaultAnimations.hurtState.clip;
            }
            else if (dead)
            {
                if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                    animationClip = anims.deadState.clip;
                else
                    animationClip = model.defaultAnimations.deadState.clip;
            }
            else if (pickup)
            {
                if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                    animationClip = anims.pickupState.clip;
                else
                    animationClip = model.defaultAnimations.pickupState.clip;
            }
            else if (attackRight)
            {
                if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims) && anims.rightHandAttackAnimations != null && attackIndex < anims.rightHandAttackAnimations.Length)
                    animationClip = anims.rightHandAttackAnimations[attackIndex].state.clip;
                else if (attackIndex < model.defaultAnimations.rightHandAttackAnimations.Length)
                    animationClip = model.defaultAnimations.rightHandAttackAnimations[attackIndex].state.clip;
            }
            else if (attackLeft)
            {
                if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims) && anims.leftHandAttackAnimations != null && attackIndex < anims.leftHandAttackAnimations.Length)
                    animationClip = anims.leftHandAttackAnimations[attackIndex].state.clip;
                else if (attackIndex < model.defaultAnimations.leftHandAttackAnimations.Length)
                    animationClip = model.defaultAnimations.leftHandAttackAnimations[attackIndex].state.clip;
            }
            else
            {
                if (TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims))
                    animationClip = anims.fallState.clip;
                else
                    animationClip = model.defaultAnimations.fallState.clip;
            }
            animationClip = EditorGUILayout.ObjectField(animationClip, typeof(AnimationClip), false) as AnimationClip;
            if (animationClip != null)
            {
                float startTime = 0.0f;
                float stopTime = animationClip.length;
                time = EditorGUILayout.Slider(time, startTime, stopTime);
            }
            else if (AnimationMode.InAnimationMode())
            {
                AnimationMode.StopAnimationMode();
            }
            EditorGUILayout.EndVertical();
        }

        void SetMovementClip(Playables.AnimState idleState, Playables.MoveStates moveStates)
        {
            if (moveForward)
            {
                if (moveRight)
                {
                    animationClip = moveStates.forwardRightState.clip;
                }
                else if (moveLeft)
                {
                    animationClip = moveStates.forwardLeftState.clip;
                }
                else
                {
                    animationClip = moveStates.forwardState.clip;
                }
            }
            else if (moveBackward)
            {
                if (moveRight)
                {
                    animationClip = moveStates.backwardRightState.clip;
                }
                else if (moveLeft)
                {
                    animationClip = moveStates.backwardLeftState.clip;
                }
                else
                {
                    animationClip = moveStates.backwardState.clip;
                }
            }
            else if (moveRight)
            {
                animationClip = moveStates.rightState.clip;
            }
            else if (moveLeft)
            {
                animationClip = moveStates.leftState.clip;
            }
            else
            {
                animationClip = idleState.clip;
            }
        }

        bool TryGetWeaponTypeBasedStates(out Playables.WeaponAnimations anims)
        {
            anims = default;
            if (weaponType == null)
                return false;
            for (int i = 0; i < model.weaponAnimations.Length; ++i)
            {
                if (model.weaponAnimations[i].weaponType == weaponType)
                    anims = model.weaponAnimations[i];
                return true;
            }
            return false;
        }

        void Update()
        {
            if (go == null)
                return;

            if (animationClip == null)
                return;

            // Animate the GameObject
            if (!EditorApplication.isPlaying && AnimationMode.InAnimationMode())
            {
                time += Time.deltaTime;
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(go, animationClip, time);
                AnimationMode.EndSampling();
                if (time > animationClip.length)
                    time = 0f;
                Repaint();
            }
        }

        void ToggleAnimationMode()
        {
            if (AnimationMode.InAnimationMode())
                AnimationMode.StopAnimationMode();
            else
                AnimationMode.StartAnimationMode();
        }
    }
}
