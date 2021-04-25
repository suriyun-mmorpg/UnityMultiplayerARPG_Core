using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
                            break;
                        case State.CanBuild:
                            meshRenderer.sharedMaterials = canBuildMaterials;
                            break;
                        case State.CannotBuild:
                            meshRenderer.sharedMaterials = cannotBuildMaterials;
                            break;
                    }
                }

                if (spriteRenderer != null)
                {
                    switch (currentState)
                    {
                        case State.Default:
                            spriteRenderer.color = defaultColor;
                            break;
                        case State.CanBuild:
                            spriteRenderer.color = canBuildColor;
                            break;
                        case State.CannotBuild:
                            spriteRenderer.color = cannotBuildColor;
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
        public Collider CacheCollider { get; private set; }
        public Collider2D CacheCollider2D { get; private set; }

        public override void Setup(DamageableEntity entity, int index)
        {
            base.Setup(entity, index);
            BuildingEntity = entity as BuildingEntity;
            BuildingEntity.RegisterMaterial(this);
            CacheCollider = GetComponent<Collider>();
            CacheCollider2D = GetComponent<Collider2D>();

            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                defaultMaterials = meshRenderer.sharedMaterials;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                defaultColor = spriteRenderer.color;

            if (tilemap == null)
                tilemap = GetComponent<Tilemap>();
            if (tilemap != null)
                defaultColor = tilemap.color;

            CurrentState = State.Unknow;
            CurrentState = State.Default;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!ValidateTriggerLayer(other.gameObject))
                return;

            BuildingMaterial material = other.GetComponent<BuildingMaterial>();
            if (material != null && CacheCollider.bounds.BoundsContainedRate(other.bounds) <= 0.025f)
            {
                BuildingEntity.TriggerExitBuildingMaterial(material);
                return;
            }

            if (BuildingEntity.IsBuildMode)
            {
                if (BuildingEntity.BuildingArea != null &&
                    BuildingEntity.BuildingArea.transform.root == other.transform.root)
                    return;
                BuildingEntity.TriggerEnterBuildingMaterial(material);
                BuildingEntity.TriggerEnterEntity(other.GetComponent<BaseGameEntity>());
                BuildingEntity.TriggerEnterNoConstructionArea(other.GetComponent<NoConstructionArea>());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (BuildingEntity.IsBuildMode)
            {
                BuildingEntity.TriggerExitEntity(other.GetComponent<BaseGameEntity>());
                BuildingEntity.TriggerExitBuildingMaterial(other.GetComponent<BuildingMaterial>());
                BuildingEntity.TriggerExitNoConstructionArea(other.GetComponent<NoConstructionArea>());
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (!ValidateTriggerLayer(other.gameObject))
                return;

            BuildingMaterial material = other.GetComponent<BuildingMaterial>();
            if (material != null && CacheCollider2D.bounds.BoundsContainedRate(other.bounds) <= 0.025f)
            {
                BuildingEntity.TriggerExitBuildingMaterial(material);
                return;
            }

            if (BuildingEntity.IsBuildMode)
            {
                if (BuildingEntity.BuildingArea != null &&
                    BuildingEntity.BuildingArea.transform.root == other.transform.root)
                    return;
                BuildingEntity.TriggerEnterBuildingMaterial(material);
                BuildingEntity.TriggerEnterEntity(other.GetComponent<BaseGameEntity>());
                BuildingEntity.TriggerEnterTilemap(other.GetComponent<TilemapCollider2D>());
                BuildingEntity.TriggerEnterNoConstructionArea(other.GetComponent<NoConstructionArea>());
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (BuildingEntity.IsBuildMode)
            {
                BuildingEntity.TriggerExitEntity(other.GetComponent<BaseGameEntity>());
                BuildingEntity.TriggerExitBuildingMaterial(other.GetComponent<BuildingMaterial>());
                BuildingEntity.TriggerExitTilemap(other.GetComponent<TilemapCollider2D>());
                BuildingEntity.TriggerExitNoConstructionArea(other.GetComponent<NoConstructionArea>());
            }
        }

        public bool ValidateTriggerLayer(GameObject gameObject)
        {
            return !(gameObject.layer == PhysicLayers.TransparentFX || gameObject.layer == PhysicLayers.IgnoreRaycast);
        }
    }
}
