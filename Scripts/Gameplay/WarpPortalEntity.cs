using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibHighLevel;

public class WarpPortalEntity : RpgNetworkEntity
{
    public string mapName;
    public Vector3 position;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer)
            return;

        var characterEntity = other.GetComponent<CharacterEntity>();
        if (characterEntity == null)
            return;

        characterEntity.Warp(mapName, position);
    }
}
