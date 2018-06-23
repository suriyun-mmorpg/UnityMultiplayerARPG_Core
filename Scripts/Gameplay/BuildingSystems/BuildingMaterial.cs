using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public BuildingObject buildingObject;

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
        if (buildingObject != null)
        {
            buildingObject.AddTriggerEntity(other.GetComponent<RpgNetworkEntity>());
            buildingObject.AddTriggerBuildingMaterial(other.GetComponent<BuildingMaterial>());
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (buildingObject != null)
        {
            buildingObject.AddTriggerEntity(other.GetComponent<RpgNetworkEntity>());
            buildingObject.AddTriggerBuildingMaterial(other.GetComponent<BuildingMaterial>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (buildingObject != null)
        {
            buildingObject.RemoveTriggerEntity(other.GetComponent<RpgNetworkEntity>());
            buildingObject.RemoveTriggerBuildingMaterial(other.GetComponent<BuildingMaterial>());
        }
    }

    // TODO: Add event when hit
}
