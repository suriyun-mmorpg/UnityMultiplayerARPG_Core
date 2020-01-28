using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MultiplayerARPG
{
    public class BuildingMaterial : DamageableHitBox<BuildingEntity>
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

        [Header("Materials settings (for 3D)")]
        public Material[] canBuildMaterials;
        public Material[] cannotBuildMaterials;

        [Header("Color Settings (for 2D)")]
        public Color canBuildColor = Color.green;
        public Color cannotBuildColor = Color.red;

        private Renderer meshRenderer;
        private SpriteRenderer spriteRenderer;
        private Tilemap tilemap;

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
                            meshRenderer.materials = defaultMaterials;
                            break;
                        case State.CanBuild:
                            meshRenderer.materials = canBuildMaterials;
                            break;
                        case State.CannotBuild:
                            meshRenderer.materials = cannotBuildMaterials;
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

        public Transform CacheTransform { get { return entity.CacheTransform; } }

        private void Awake()
        {
            gameObject.tag = GameInstance.Singleton.buildingTag;
            gameObject.layer = GameInstance.Singleton.buildingLayer;

            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                defaultMaterials = meshRenderer.materials;

            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                defaultColor = spriteRenderer.color;

            tilemap = GetComponent<Tilemap>();
            if (tilemap != null)
                defaultColor = tilemap.color;

            CurrentState = State.Unknow;
            CurrentState = State.Default;
        }

        protected override void Start()
        {
            base.Start();
            entity.RegisterMaterial(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!ValidateTriggerLayer(other.gameObject))
                return;

            if (entity != null && entity.IsBuildMode)
            {
                entity.TriggerEnterEntity(other.GetComponent<BaseGameEntity>());
                entity.TriggerEnterBuildingMaterial(other.GetComponent<BuildingMaterial>());
                entity.TriggerEnterTilemap(other.GetComponent<TilemapCollider2D>());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (entity != null && entity.IsBuildMode)
            {
                entity.TriggerExitEntity(other.GetComponent<BaseGameEntity>());
                entity.TriggerExitBuildingMaterial(other.GetComponent<BuildingMaterial>());
                entity.TriggerExitTilemap(other.GetComponent<TilemapCollider2D>());
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!ValidateTriggerLayer(other.gameObject))
                return;

            if (entity != null && entity.IsBuildMode)
            {
                entity.TriggerEnterEntity(other.GetComponent<BaseGameEntity>());
                entity.TriggerEnterBuildingMaterial(other.GetComponent<BuildingMaterial>());
                entity.TriggerEnterTilemap(other.GetComponent<TilemapCollider2D>());
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (entity != null && entity.IsBuildMode)
            {
                entity.TriggerExitEntity(other.GetComponent<BaseGameEntity>());
                entity.TriggerExitBuildingMaterial(other.GetComponent<BuildingMaterial>());
                entity.TriggerExitTilemap(other.GetComponent<TilemapCollider2D>());
            }
        }

        public bool ValidateTriggerLayer(GameObject gameObject)
        {
            return !(gameObject.layer == PhysicLayers.TransparentFX || gameObject.layer == PhysicLayers.IgnoreRaycast);
        }
    }
}
