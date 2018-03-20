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

        var playerCharacterEntity = other.GetComponent<PlayerCharacterEntity>();
        if (playerCharacterEntity == null)
            return;

        playerCharacterEntity.Warp(mapName, position);
    }
}
