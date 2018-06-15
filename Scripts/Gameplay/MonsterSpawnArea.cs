using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

public class MonsterSpawnArea : MonoBehaviour
{
    public MonsterCharacter database;
    public short level = 1;
    public short amount = 1;
    public float randomRadius = 5f;

    public void RandomSpawn(LiteNetLibGameManager manager)
    {
        if (database == null)
        {
            Debug.LogWarning("Have to set monster database to spawn monster");
            return;
        }
        var gameInstance = GameInstance.Singleton;
        var dataId = database.HashId;
        MonsterCharacter foundDatabase;
        if (!GameInstance.MonsterCharacters.TryGetValue(dataId, out foundDatabase))
        {
            Debug.LogWarning("The monster database have to be added to game instance");
            return;
        }
        for (var i = 0; i < amount; ++i)
        {
            var randomedPosition = Random.insideUnitSphere * randomRadius;
            randomedPosition = transform.position + new Vector3(randomedPosition.x, 0, randomedPosition.z);
            var randomedRotation = Vector3.up * Random.Range(0, 360);
            var identity = manager.Assets.NetworkSpawn(gameInstance.monsterCharacterEntityPrefab.gameObject, randomedPosition, Quaternion.Euler(randomedRotation));
            var entity = identity.GetComponent<MonsterCharacterEntity>();
            entity.Id = GenericUtils.GetUniqueId();
            entity.DataId = dataId;
            entity.Level = level;
            var stats = entity.GetStats();
            entity.CurrentHp = (int)stats.hp;
            entity.CurrentMp = (int)stats.mp;
            entity.CurrentStamina = (int)stats.stamina;
            entity.CurrentFood = (int)stats.food;
            entity.CurrentWater = (int)stats.water;
            entity.respawnPosition = randomedPosition;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, randomRadius);
    }
}
