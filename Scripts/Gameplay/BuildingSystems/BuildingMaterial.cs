using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public Material[] canBuildMaterials;
        public Material[] cannotBuildMaterials;
        [HideInInspector, System.NonSerialized]
        public BuildingEntity buildingEntity;

        private Renderer meshRenderer;

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
            }
        }

        public uint ObjectId { get { return buildingEntity.ObjectId; } }
        public int CurrentHp { get { return buildingEntity.CurrentHp; } set { buildingEntity.CurrentHp = value; } }
        public BaseGameEntity Entity { get { return buildingEntity; } }
        public Transform CacheTransform { get { return buildingEntity.CacheTransform; } }
        
        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                defaultMaterials = meshRenderer.materials;
            CurrentState = State.Unknow;
            CurrentState = State.Default;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (buildingEntity != null)
            {
                buildingEntity.TriggerEnterEntity(other.GetComponent<BaseGameEntity>());
                buildingEntity.TriggerEnterBuildingMaterial(other.GetComponent<BuildingMaterial>());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (buildingEntity != null)
            {
                buildingEntity.TriggerExitEntity(other.GetComponent<BaseGameEntity>());
                buildingEntity.TriggerExitBuildingMaterial(other.GetComponent<BuildingMaterial>());
            }
        }

        public bool IsDead()
        {
            return buildingEntity.IsDead();
        }

        public void ReceiveDamage(IAttackerEntity attacker, CharacterItem weapon, Dictionary<DamageElement, MinMaxFloat> allDamageAmounts, CharacterBuff debuff, uint hitEffectsId)
        {
            buildingEntity.ReceiveDamage(attacker, weapon, allDamageAmounts, debuff, hitEffectsId);
        }

        public bool CanReceiveDamageFrom(IAttackerEntity attacker)
        {
            return buildingEntity.CanReceiveDamageFrom(attacker);
        }
    }
}
