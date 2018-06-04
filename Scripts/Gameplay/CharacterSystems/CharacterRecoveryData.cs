using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterRecoveryData : BaseCharacterComponentData
{
    #region Recovery System Data
    [HideInInspector, System.NonSerialized]
    public float recoveryingHp;
    [HideInInspector, System.NonSerialized]
    public float recoveryingMp;
    [HideInInspector, System.NonSerialized]
    public float recoveryingStamina;
    [HideInInspector, System.NonSerialized]
    public float recoveryingFood;
    [HideInInspector, System.NonSerialized]
    public float recoveryingWater;
    [HideInInspector, System.NonSerialized]
    public float decreasingHp;
    [HideInInspector, System.NonSerialized]
    public float decreasingMp;
    [HideInInspector, System.NonSerialized]
    public float decreasingStamina;
    [HideInInspector, System.NonSerialized]
    public float decreasingFood;
    [HideInInspector, System.NonSerialized]
    public float decreasingWater;
    [HideInInspector, System.NonSerialized]
    public float recoveryUpdateDeltaTime;
    #endregion
}
