using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSkillAndBuffData : BaseCharacterComponentData
{
    #region Buff System Data
    [HideInInspector, System.NonSerialized]
    public float skillBuffUpdateDeltaTime;
    #endregion
}
