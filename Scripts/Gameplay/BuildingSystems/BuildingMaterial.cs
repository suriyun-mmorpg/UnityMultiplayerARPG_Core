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

        [Header("3D Settings")]
        public Material[] canBuildMaterials;
        public Material[] cannotBuildMaterials;
        public Renderer meshRenderer;

        private Material[] _defaultMaterials;
        private ShadowCastingMode _defaultShadowCastingMode;
        private bool _defaultReceiveShadows;

        [Header("2D Settings")]
        public Color canBuildColor = Color.green;
        public Color cannotBuildColor = Color.red;
        public SpriteRenderer spriteRenderer;
        public Tilemap tilemap;

        private Color _defaultColor;

        [Header("Build Mode Settings")]
        [Range(0.1f, 1f)]
        [Tooltip("It will be used to reduce collider's bounds when find other intersecting building materials")]
        public float boundsSizeRateWhilePlacing = 0.9f;

        private State _currentState;
        public State CurrentState
        {
            get { return _currentState; }
            set
            {
                if (_currentState == value)
                    return;
                _currentState = value;
                if (meshRenderer != null)
                {
                    switch (_currentState)
                    {
                        case State.Default:
                            meshRenderer.sharedMaterials = _defaultMaterials;
                            meshRenderer.shadowCastingMode = _defaultShadowCastingMode;
                            meshRenderer.receiveShadows = _defaultReceiveShadows;
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
                    switch (_currentState)
                    {
                        case State.Default:
                            spriteRenderer.color = _defaultColor;
                            spriteRenderer.shadowCastingMode = _defaultShadowCastingMode;
                            spriteRenderer.receiveShadows = _defaultReceiveShadows;
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
                    switch (_currentState)
                    {
                        case State.Default:
                            tilemap.color = _defaultColor;
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

        private BuildingMaterialBuildModeHandler _buildModeHandler;

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
                _defaultMaterials = meshRenderer.sharedMaterials;
                _defaultShadowCastingMode = meshRenderer.shadowCastingMode;
                _defaultReceiveShadows = meshRenderer.receiveShadows;
            }

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                _defaultColor = spriteRenderer.color;
                _defaultShadowCastingMode = spriteRenderer.shadowCastingMode;
                _defaultReceiveShadows = spriteRenderer.receiveShadows;
            }

            if (tilemap == null)
                tilemap = GetComponent<Tilemap>();
            if (tilemap != null)
                _defaultColor = tilemap.color;

            CurrentState = State.Unknow;
            CurrentState = State.Default;

            if (BuildingEntity.IsBuildMode)
            {
                if (CacheNavMeshObstacle != null)
                    CacheNavMeshObstacle.enabled = false;

                if (_buildModeHandler == null)
                {
                    _buildModeHandler = gameObject.AddComponent<BuildingMaterialBuildModeHandler>();
                    _buildModeHandler.Setup(this);
                }
            }
        }
    }
}
