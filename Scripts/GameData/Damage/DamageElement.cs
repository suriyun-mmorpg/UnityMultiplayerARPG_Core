using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageElement", menuName = "Create GameData/DamageElement")]
public class DamageElement : BaseGameData
{
    // TODO: Resistance
    public float GetDamageReceiveRate(ICharacterData characterData)
    {
        return 1f;
    }
}
