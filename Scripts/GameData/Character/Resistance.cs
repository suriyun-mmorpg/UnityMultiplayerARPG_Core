using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ResistanceAmount
{
    public DamageElement damageElement;
    public float amount;
}

[System.Serializable]
public struct ResistanceIncremental
{
    public DamageElement damageElement;
    public IncrementalFloat amount;
}
