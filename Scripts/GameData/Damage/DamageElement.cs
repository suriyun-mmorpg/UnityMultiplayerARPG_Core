using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DamageElement", menuName = "Create GameData/DamageElement")]
public class DamageElement : BaseGameData
{
    public CharacterResistance resistance;
    public float GetDamageReceiveRate(ICharacterData characterData)
    {
        if (resistance == null)
            return 1f;
        return 1f;  // TODO: Implement this
    }
}
