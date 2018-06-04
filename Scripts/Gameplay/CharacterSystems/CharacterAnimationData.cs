using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimationData : BaseCharacterComponentData
{
    #region Animation System Data
    [HideInInspector, System.NonSerialized]
    public Vector3? previousPosition;
    [HideInInspector, System.NonSerialized]
    public Vector3 currentVelocity;
    [HideInInspector, System.NonSerialized]
    public float velocityCalculationDeltaTime;
    #endregion
}
