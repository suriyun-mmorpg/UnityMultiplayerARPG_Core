using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;
using LiteNetLib;
using LiteNetLib.Utils;

[RequireComponent(typeof(RpgGameManager))]
public class LanRpgNetworkManager : LiteNetLibGameManager
{
    public static LanRpgNetworkManager Singleton { get; protected set; }
    public enum GameStartType
    {
        Client,
        Host,
        SinglePlayer,
    }

    public float autoSaveDuration = 2f;
    public GameStartType startType;
    public PlayerCharacterData selectedCharacter;
    protected float lastSaveTime;

    private RpgGameManager cacheGameManager;
    public RpgGameManager CacheGameManager
    {
        get
        {
            if (cacheGameManager == null)
                cacheGameManager = GetComponent<RpgGameManager>();
            return cacheGameManager;
        }
    }

    protected override void Awake()
    {
        Singleton = this;
        doNotDestroyOnSceneChanges = true;
        base.Awake();
    }

    public void StartGame()
    {
        CacheGameManager.Init(this);
        var gameInstance = GameInstance.Singleton;
        var gameServiceConnection = gameInstance.NetworkSetting;
        switch (startType)
        {
            case GameStartType.Host:
                networkPort = gameServiceConnection.networkPort;
                maxConnections = gameServiceConnection.maxConnections;
                StartHost(false);
                break;
            case GameStartType.SinglePlayer:
                StartHost(true);
                break;
            case GameStartType.Client:
                networkPort = gameServiceConnection.networkPort;
                StartClient();
                break;
        }
    }

    protected override void Update()
    {
        base.Update();
        if (Time.unscaledTime - lastSaveTime > autoSaveDuration)
        {
            var owningCharacter = BasePlayerCharacterController.OwningCharacter;
            if (owningCharacter != null && IsNetworkActive)
                owningCharacter.SavePersistentCharacterData();
            lastSaveTime = Time.unscaledTime;
        }
    }

    public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        base.OnClientDisconnected(peer, disconnectInfo);
        CacheGameManager.OnClientDisconnected(peer, disconnectInfo);
    }

    public override void OnServerOnlineSceneLoaded()
    {
        base.OnServerOnlineSceneLoaded();
        CacheGameManager.OnServerOnlineSceneLoaded();
    }

    public override void SerializeClientReadyExtra(NetDataWriter writer)
    {
        writer.Put(selectedCharacter.Id);
        writer.Put(selectedCharacter.DatabaseId);
        writer.Put(selectedCharacter.CharacterName);
        writer.Put(selectedCharacter.Level);
        writer.Put(selectedCharacter.Exp);
        writer.Put(selectedCharacter.CurrentHp);
        writer.Put(selectedCharacter.CurrentMp);
        writer.Put(selectedCharacter.CurrentStamina);
        writer.Put(selectedCharacter.CurrentFood);
        writer.Put(selectedCharacter.CurrentWater);
        writer.Put(selectedCharacter.StatPoint);
        writer.Put(selectedCharacter.SkillPoint);
        writer.Put(selectedCharacter.Gold);
        writer.Put(selectedCharacter.CurrentMapName);
        writer.Put(selectedCharacter.CurrentPosition.x);
        writer.Put(selectedCharacter.CurrentPosition.y);
        writer.Put(selectedCharacter.CurrentPosition.z);
        writer.Put(selectedCharacter.RespawnMapName);
        writer.Put(selectedCharacter.RespawnPosition.x);
        writer.Put(selectedCharacter.RespawnPosition.y);
        writer.Put(selectedCharacter.RespawnPosition.z);
        writer.Put(selectedCharacter.LastUpdate);
        writer.Put(selectedCharacter.Attributes.Count);
        foreach (var entry in selectedCharacter.Attributes)
        {
            writer.Put(entry.attributeId);
            writer.Put(entry.amount);
        }
        writer.Put(selectedCharacter.Buffs.Count);
        foreach (var entry in selectedCharacter.Buffs)
        {
            writer.Put(entry.id);
            writer.Put(entry.characterId);
            writer.Put(entry.dataId);
            writer.Put((byte)entry.type);
            writer.Put(entry.level);
            writer.Put(entry.buffRemainsDuration);
        }
        writer.Put(selectedCharacter.Skills.Count);
        foreach (var entry in selectedCharacter.Skills)
        {
            writer.Put(entry.skillId);
            writer.Put(entry.level);
            writer.Put(entry.coolDownRemainsDuration);
        }
        writer.Put(selectedCharacter.EquipItems.Count);
        foreach (var entry in selectedCharacter.EquipItems)
        {
            writer.Put(entry.id);
            writer.Put(entry.itemId);
            writer.Put(entry.level);
            writer.Put(entry.amount);
        }
        writer.Put(selectedCharacter.NonEquipItems.Count);
        foreach (var entry in selectedCharacter.NonEquipItems)
        {
            writer.Put(entry.id);
            writer.Put(entry.itemId);
            writer.Put(entry.level);
            writer.Put(entry.amount);
        }
        writer.Put(selectedCharacter.Hotkeys.Count);
        foreach (var entry in selectedCharacter.Hotkeys)
        {
            writer.Put(entry.hotkeyId);
            writer.Put((byte)entry.type);
            writer.Put(entry.dataId);
        }
        writer.Put(selectedCharacter.Quests.Count);
        foreach (var entry in selectedCharacter.Quests)
        {
            writer.Put(entry.questId);
            writer.Put(entry.isComplete);
            var killedMonsters = entry.killedMonsters;
            var killMonsterCount = killedMonsters == null ? 0 : killedMonsters.Count;
            writer.Put(killMonsterCount);
            if (killMonsterCount > 0)
            {
                foreach (var killedMonster in killedMonsters)
                {
                    writer.Put(killedMonster.Key);
                    writer.Put(killedMonster.Value);
                }
            }
        }
        var rightHand = selectedCharacter.EquipWeapons.rightHand;
        writer.Put(rightHand.id);
        writer.Put(rightHand.itemId);
        writer.Put(rightHand.level);
        writer.Put(rightHand.amount);
        var leftHand = selectedCharacter.EquipWeapons.leftHand;
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
        character.CurrentStamina = reader.GetInt();
        character.CurrentFood = reader.GetInt();
        character.CurrentWater = reader.GetInt();
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
            entry.id = reader.GetString();
            entry.characterId = reader.GetString();
            entry.dataId = reader.GetString();
            entry.type = (BuffType)reader.GetByte();
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
            entry.type = (HotkeyType)reader.GetByte();
            entry.dataId = reader.GetString();
            character.Hotkeys.Add(entry);
        }
        count = reader.GetInt();
        for (var i = 0; i < count; ++i)
        {
            var entry = new CharacterQuest();
            entry.questId = reader.GetString();
            entry.isComplete = reader.GetBool();
            var killMonsterCount = reader.GetInt();
            entry.killedMonsters = new Dictionary<string, int>();
            for (var j = 0; j < killMonsterCount; ++j)
            {
                entry.killedMonsters.Add(reader.GetString(), reader.GetInt());
            }
            character.Quests.Add(entry);
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
