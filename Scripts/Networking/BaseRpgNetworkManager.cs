using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using LiteNetLibHighLevel;
using LiteNetLib;
using LiteNetLib.Utils;

public abstract class BaseRpgNetworkManager : LiteNetLibGameManager
{
    protected override void Awake()
    {
        var gameInstance = GameInstance.Singleton;
        Assets.playerPrefab = gameInstance.characterEntityPrefab.Identity;
        /*
        var damages = GameInstance.Damages.Values;
        foreach (var damage in damages)
        {
            Assets.RegisterPrefab(damage)
        }
        */
        base.Awake();
    }
    
#if UNITY_EDITOR
    private void OnValidate()
    {
        clientReadyOnConnect = true;
    }
#endif

    protected override void RegisterServerMessages()
    {
        base.RegisterServerMessages();
    }

    protected override void RegisterClientMessages()
    {
        base.RegisterClientMessages();
    }

    public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        base.OnPeerDisconnected(peer, disconnectInfo);
    }

    public override void SerializeClientReadyExtra(NetDataWriter writer)
    {
        base.SerializeClientReadyExtra(writer);
    }

    public override void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, NetDataReader reader)
    {
        base.DeserializeClientReadyExtra(playerIdentity, reader);
    }
}
