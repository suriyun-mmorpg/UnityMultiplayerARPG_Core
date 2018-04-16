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

    public float autoSaveDuration = 2f;
    protected float lastSaveTime;
    public static GameStartType StartType;
    public static string ConnectingNetworkAddress;
    public static PlayerCharacterData SelectedCharacter;
    
    protected virtual void Start()
    {
        var gameInstance = GameInstance.Singleton;
        var gameInstanceExtra = gameInstance.GetExtra<LanGameInstanceExtra>();
        switch (StartType)
        {
            case GameStartType.Host:
                networkPort = gameInstanceExtra.networkPort;
                maxConnections = gameInstanceExtra.maxConnections;
                StartHost(false);
                break;
            case GameStartType.SinglePlayer:
                StartHost(true);
                break;
            case GameStartType.Client:
                networkAddress = ConnectingNetworkAddress;
                networkPort = gameInstanceExtra.networkPort;
                StartClient();
                break;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (Time.realtimeSinceStartup - lastSaveTime > autoSaveDuration)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null)
                owningCharacter.SavePersistentCharacterData();
            lastSaveTime = Time.realtimeSinceStartup;
        }
    }

    public override void SerializeClientReadyExtra(NetDataWriter writer)
    {
        writer.Put(SelectedCharacter.Id);
        writer.Put(SelectedCharacter.DatabaseId);
        writer.Put(SelectedCharacter.CharacterName);
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
        writer.Put(SelectedCharacter.Attributes.Count);
        foreach (var entry in SelectedCharacter.Attributes)
        {
            writer.Put(entry.attributeId);
            writer.Put(entry.amount);
        }
        writer.Put(SelectedCharacter.Buffs.Count);
        foreach (var entry in SelectedCharacter.Buffs)
        {
            writer.Put(entry.skillId);
            writer.Put(entry.isDebuff);
            writer.Put(entry.level);
            writer.Put(entry.buffRemainsDuration);
        }
        writer.Put(SelectedCharacter.Skills.Count);
        foreach (var entry in SelectedCharacter.Skills)
        {
            writer.Put(entry.skillId);
            writer.Put(entry.level);
            writer.Put(entry.coolDownRemainsDuration);
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
        writer.Put(SelectedCharacter.Hotkeys.Count);
        foreach (var entry in SelectedCharacter.Hotkeys)
        {
            writer.Put(entry.hotkeyId);
            writer.Put((byte)entry.type);
            writer.Put(entry.dataId);
        }
        var rightHand = SelectedCharacter.EquipWeapons.rightHand;
        writer.Put(rightHand.id);
        writer.Put(rightHand.itemId);
        writer.Put(rightHand.level);
        writer.Put(rightHand.amount);
        var leftHand = SelectedCharacter.EquipWeapons.leftHand;
        writer.Put(leftHand.id);
        writer.Put(leftHand.itemId);
        writer.Put(leftHand.level);
        writer.Put(leftHand.amount);
    }

    public override void DeserializeClientReadyExtra(LiteNetLibIdentity playerIdentity, NetDataReader reader)
    {
        var character = new PlayerCharacterData();
        character.Id = reader.GetString();
        character.DatabaseId = reader.GetString();
        character.CharacterName = reader.GetString();
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
            var entry = new CharacterAttribute();
            entry.attributeId = reader.GetString();
            entry.amount = reader.GetInt();
            character.Attributes.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterBuff();
            entry.skillId = reader.GetString();
            entry.isDebuff = reader.GetBool();
            entry.level = reader.GetInt();
            entry.buffRemainsDuration = reader.GetFloat();
            character.Buffs.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterSkill();
            entry.skillId = reader.GetString();
            entry.level = reader.GetInt();
            entry.coolDownRemainsDuration = reader.GetFloat();
            character.Skills.Add(entry);
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
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterHotkey();
            entry.hotkeyId = reader.GetString();
            entry.type = (HotkeyTypes)reader.GetByte();
            entry.dataId = reader.GetString();
            character.Hotkeys.Add(entry);
        }

        var rightWeapon = new CharacterItem();
        rightWeapon.id = reader.GetString();
        rightWeapon.itemId = reader.GetString();
        rightWeapon.level = reader.GetInt();
        rightWeapon.amount = reader.GetInt();

        var leftWeapon = new CharacterItem();
        leftWeapon.id = reader.GetString();
        leftWeapon.itemId = reader.GetString();
        leftWeapon.level = reader.GetInt();
        leftWeapon.amount = reader.GetInt();

        var equipWeapons = new EquipWeapons();
        equipWeapons.rightHand = rightWeapon;
        equipWeapons.leftHand = leftWeapon;
        character.EquipWeapons = equipWeapons;

        character.ValidateCharacterData();
        var playerCharacterEntity = playerIdentity.GetComponent<PlayerCharacterEntity>();
        character.CloneTo(playerCharacterEntity);
    }
}
