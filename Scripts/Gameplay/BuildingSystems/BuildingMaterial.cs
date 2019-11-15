using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace MultiplayerARPG
{
    public class BuildingMaterial : MonoBehaviour, IDamageableEntity
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

        [HideInInspector, System.NonSerialized]
        public BuildingEntity buildingEntity;

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

        public uint ObjectId { get { return buildingEntity.ObjectId; } }
        public int CurrentHp { get { return buildingEntity.CurrentHp; } set { buildingEntity.CurrentHp = value; } }
        public Transform OpponentAimTransform { get { return buildingEntity.OpponentAimTransform; } }
        public BaseGameEntity Entity { get { return buildingEntity; } }
        public Transform CacheTransform { get { return buildingEntity.CacheTransform; } }
        
        private void Awake()
        {
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

        private void OnTriggerEnter(Collider other)
        {
            if (!ValidateTriggerLayer(other.gameObject))
                return;

            if (buildingEntity != null)
            {
                buildingEntity.TriggerEnterEntity(other.GetComponent<BaseGameEntity>());
                buildingEntity.TriggerEnterBuildingMaterial(other.GetComponent<BuildingMaterial>());
                buildingEntity.TriggerEnterTilemap(other.GetComponent<TilemapCollider2D>());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (buildingEntity != null)
            {
                buildingEntity.TriggerExitEntity(other.GetComponent<BaseGameEntity>());
                buildingEntity.TriggerExitBuildingMaterial(other.GetComponent<BuildingMaterial>());
                buildingEntity.TriggerExitTilemap(other.GetComponent<TilemapCollider2D>());
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!ValidateTriggerLayer(other.gameObject))
                return;

            if (buildingEntity != null)
            {
                buildingEntity.TriggerEnterEntity(other.GetComponent<BaseGameEntity>());
                buildingEntity.TriggerEnterBuildingMaterial(other.GetComponent<BuildingMaterial>());
                buildingEntity.TriggerEnterTilemap(other.GetComponent<TilemapCollider2D>());
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (buildingEntity != null)
            {
                buildingEntity.TriggerExitEntity(other.GetComponent<BaseGameEntity>());
                buildingEntity.TriggerExitBuildingMaterial(other.GetComponent<BuildingMaterial>());
                buildingEntity.TriggerExitTilemap(other.GetComponent<TilemapCollider2D>());
            }
        }

        public bool ValidateTriggerLayer(GameObject gameObject)
        {
            return !(gameObject.layer == 1 /* TransparentFX */ ||
                gameObject.layer == 2 /* IgnoreRaycast */ ||
                gameObject.layer == 4 /* Water */);
        }

        public bool IsDead()
        {
            return buildingEntity.IsDead();
        }

        public void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> damageAmounts, BaseSkill skill, short skillLevel)
        {
            buildingEntity.ReceiveDamage(attacker, weapon, damageAmounts, skill, skillLevel);
        }

        public bool CanReceiveDamageFrom(IAttackerEntity attacker)
        {
            return buildingEntity.CanReceiveDamageFrom(attacker);
        }

        public void PlayHitEffects(IEnumerable<DamageElement> damageElements, BaseSkill skill)
        {
            buildingEntity.PlayHitEffects(damageElements, skill);
        }
    }
}
