using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public class BuildingMaterial : MonoBehaviour
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
        [HideInInspector]
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
                buildingEntity.TriggerEnterEntity(other.GetComponent<RpgNetworkEntity>());
                buildingEntity.TriggerEnterBuildingMaterial(other.GetComponent<BuildingMaterial>());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (buildingEntity != null)
            {
                buildingEntity.TriggerExitEntity(other.GetComponent<RpgNetworkEntity>());
                buildingEntity.TriggerExitBuildingMaterial(other.GetComponent<BuildingMaterial>());
            }
        }
    }
}
