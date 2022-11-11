using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

namespace MultiplayerARPG
{
    public class BuildingMaterial : DamageableHitBox
    {
        public enum State
        {
            Unknow,
            Default,
            CanBuild,
            CannotBuild,
        }
        private Material[] defaultMaterials;
        private ShadowCastingMode defaultShadowCastingMode;
        private bool defaultReceiveShadows;
        private Color defaultColor;

        [Header("Materials Settings (for 3D)")]
        public Material[] canBuildMaterials;
        public Material[] cannotBuildMaterials;

        [Header("Color Settings (for 2D)")]
        public Color canBuildColor = Color.green;
        public Color cannotBuildColor = Color.red;

        [Header("Renderer Components")]
        public Renderer meshRenderer;
        public SpriteRenderer spriteRenderer;
        public Tilemap tilemap;

        [Header("Build Mode Settings")]
        [Range(0.1f, 1f)]
        [Tooltip("It will be used to reduce collider's bounds when find other intersecting building materials")]
        public float boundsSizeRateWhilePlacing = 0.9f;

        private State currentState;
        public State CurrentState
        {
            get { return currentState; }
            set
            {
                if (currentState == value)
                    return;
                currentState = value;
                if (meshRenderer != null)
                {
                    switch (currentState)
                    {
                        case State.Default:
                            meshRenderer.sharedMaterials = defaultMaterials;
                            meshRenderer.shadowCastingMode = defaultShadowCastingMode;
                            meshRenderer.receiveShadows = defaultReceiveShadows;
                            break;
                        case State.CanBuild:
                            meshRenderer.sharedMaterials = canBuildMaterials;
                            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                            meshRenderer.receiveShadows = false;
                            break;
                        case State.CannotBuild:
                            meshRenderer.sharedMaterials = cannotBuildMaterials;
                            meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                            meshRenderer.receiveShadows = false;
                            break;
                    }
                }

                if (spriteRenderer != null)
                {
                    switch (currentState)
                    {
                        case State.Default:
                            spriteRenderer.color = defaultColor;
                            spriteRenderer.shadowCastingMode = defaultShadowCastingMode;
                            spriteRenderer.receiveShadows = defaultReceiveShadows;
                            break;
                        case State.CanBuild:
                            spriteRenderer.color = canBuildColor;
                            spriteRenderer.shadowCastingMode = ShadowCastingMode.Off;
                            spriteRenderer.receiveShadows = false;
                            break;
                        case State.CannotBuild:
                            spriteRenderer.color = cannotBuildColor;
                            spriteRenderer.shadowCastingMode = ShadowCastingMode.Off;
                            spriteRenderer.receiveShadows = false;
                            break;
                    }
                }

                if (tilemap != null)
                {
                    switch (currentState)
                    {
                        case State.Default:
                            tilemap.color = defaultColor;
                            break;
                        case State.CanBuild:
                            tilemap.color = canBuildColor;
                            break;
                        case State.CannotBuild:
                            tilemap.color = cannotBuildColor;
                            break;
                    }
                }
            }
        }

        public BuildingEntity BuildingEntity { get; private set; }
        public NavMeshObstacle CacheNavMeshObstacle { get; private set; }

        private BuildingMaterialBuildModeHandler buildModeHandler;

        public override void Setup(byte index)
        {
            base.Setup(index);
            BuildingEntity = DamageableEntity as BuildingEntity;
            BuildingEntity.RegisterMaterial(this);
            CacheNavMeshObstacle = GetComponent<NavMeshObstacle>();

            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                defaultMaterials = meshRenderer.sharedMaterials;
                defaultShadowCastingMode = meshRenderer.shadowCastingMode;
                defaultReceiveShadows = meshRenderer.receiveShadows;
            }

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                defaultColor = spriteRenderer.color;
                defaultShadowCastingMode = spriteRenderer.shadowCastingMode;
                defaultReceiveShadows = spriteRenderer.receiveShadows;
            }

            if (tilemap == null)
                tilemap = GetComponent<Tilemap>();
            if (tilemap != null)
                defaultColor = tilemap.color;

            CurrentState = State.Unknow;
            CurrentState = State.Default;

            if (BuildingEntity.IsBuildMode)
            {
                if (CacheNavMeshObstacle != null)
                    CacheNavMeshObstacle.enabled = false;

                if (buildModeHandler == null)
                {
                    buildModeHandler = gameObject.AddComponent<BuildingMaterialBuildModeHandler>();
                    buildModeHandler.Setup(this);
                }
            }
        }
    }
}
