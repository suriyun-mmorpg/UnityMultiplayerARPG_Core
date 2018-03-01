using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibHighLevel;
using LiteNetLib;
using LiteNetLib.Utils;

public class LanRpgNetworkManager : BaseRpgNetworkManager
{
    public enum GameStartType
    {
        Client,
        Host,
        SinglePlayer,
    }

    public static GameStartType StartType;
    public static string ConnectingNetworkAddress;
    public static int ConnectingNetworkPort;
    public static CharacterData SelectedCharacter;
    
    protected virtual void Start()
    {
        switch (StartType)
        {
            case GameStartType.Host:
                StartHost();
                break;
            case GameStartType.SinglePlayer:
                maxConnections = 1;
                StartHost();
                break;
            case GameStartType.Client:
                networkAddress = ConnectingNetworkAddress;
                networkPort = ConnectingNetworkPort;
                StartClient();
                break;
        }
    }

    public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        base.OnClientDisconnected(peer, disconnectInfo);
        UISceneLoading.Singleton.LoadScene(GameInstance.Singleton.homeSceneName);
    }

    public override void SerializeClientReadyExtra(NetDataWriter writer)
    {
        writer.Put(SelectedCharacter.Id);
        writer.Put(SelectedCharacter.CharacterName);
        writer.Put(SelectedCharacter.PrototypeId);
        writer.Put(SelectedCharacter.Level);
        writer.Put(SelectedCharacter.Exp);
        writer.Put(SelectedCharacter.CurrentHp);
        writer.Put(SelectedCharacter.CurrentMp);
        writer.Put(SelectedCharacter.StatPoint);
        writer.Put(SelectedCharacter.SkillPoint);
        writer.Put(SelectedCharacter.Gold);
        writer.Put(SelectedCharacter.CurrentMapName);
        writer.Put(SelectedCharacter.CurrentPosition.x);
        writer.Put(SelectedCharacter.CurrentPosition.y);
        writer.Put(SelectedCharacter.CurrentPosition.z);
        writer.Put(SelectedCharacter.RespawnMapName);
        writer.Put(SelectedCharacter.RespawnPosition.x);
        writer.Put(SelectedCharacter.RespawnPosition.y);
        writer.Put(SelectedCharacter.RespawnPosition.z);
        writer.Put(SelectedCharacter.LastUpdate);
        writer.Put(SelectedCharacter.AttributeLevels.Count);
        foreach (var entry in SelectedCharacter.AttributeLevels)
        {
            writer.Put(entry.attributeId);
            writer.Put(entry.amount);
        }
        writer.Put(SelectedCharacter.SkillLevels.Count);
        foreach (var entry in SelectedCharacter.SkillLevels)
        {
            writer.Put(entry.skillId);
            writer.Put(entry.level);
        }
        writer.Put(SelectedCharacter.EquipItems.Count);
        foreach (var entry in SelectedCharacter.EquipItems)
        {
            writer.Put(entry.id);
            writer.Put(entry.itemId);
            writer.Put(entry.level);
            writer.Put(entry.amount);
        }
        writer.Put(SelectedCharacter.NonEquipItems.Count);
        foreach (var entry in SelectedCharacter.NonEquipItems)
        {
            writer.Put(entry.id);
            writer.Put(entry.itemId);
            writer.Put(entry.level);
            writer.Put(entry.amount);
        }
    }

    public override void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, NetDataReader reader)
    {
        var character = new CharacterData();
        character.Id = reader.GetString();
        character.CharacterName = reader.GetString();
        character.PrototypeId = reader.GetString();
        character.Level = reader.GetInt();
        character.Exp = reader.GetInt();
        character.CurrentHp = reader.GetInt();
        character.CurrentMp = reader.GetInt();
        character.StatPoint = reader.GetInt();
        character.SkillPoint = reader.GetInt();
        character.Gold = reader.GetInt();
        character.CurrentMapName = reader.GetString();
        character.CurrentPosition = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        character.RespawnMapName = reader.GetString();
        character.RespawnPosition = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        character.LastUpdate = reader.GetInt();
        var count = 0;
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterAttributeLevel();
            entry.attributeId = reader.GetString();
            entry.amount = reader.GetInt();
            character.AttributeLevels.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterSkillLevel();
            entry.skillId = reader.GetString();
            entry.level = reader.GetInt();
            character.SkillLevels.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterItem();
            entry.id = reader.GetString();
            entry.itemId = reader.GetString();
            entry.level = reader.GetInt();
            entry.amount = reader.GetInt();
            character.EquipItems.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterItem();
            entry.id = reader.GetString();
            entry.itemId = reader.GetString();
            entry.level = reader.GetInt();
            entry.amount = reader.GetInt();
            character.NonEquipItems.Add(entry);
        }
        var characterEntity = playerIdentity.GetComponent<CharacterEntity>();
        character.CloneTo(characterEntity);
    }
}
