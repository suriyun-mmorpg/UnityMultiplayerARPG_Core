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

    // TODO: Add event when hit
}
