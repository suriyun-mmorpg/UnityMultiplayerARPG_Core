using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawnArea : MonoBehaviour
{
    public MonsterCharacter database;
    public short level = 1;
    public short amount = 1;
    public float randomRadius = 5f;
    [HideInInspector]
    public BaseGameNetworkManager manager;
    
    private GameInstance gameInstance { get { return GameInstance.Singleton; } }
    private int dataId { get { return database == null ? 0 : database.DataId; } }

    public void RandomSpawn()
    {
        if (database == null)
        {
            Debug.LogWarning("Have to set monster database to spawn monster");
            return;
        }
        MonsterCharacter foundDatabase;
        if (!GameInstance.MonsterCharacters.TryGetValue(dataId, out foundDatabase))
        {
            Debug.LogWarning("The monster database have to be added to game instance");
            return;
        }
        for (var i = 0; i < amount; ++i)
        {
            Spawn(0);
        }
    }

    public void Spawn(float delay)
    {
        StartCoroutine(SpawnRoutine(delay));
    }

    IEnumerator SpawnRoutine(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        var spawnPosition = GetRandomPosition();
        var spawnRotation = GetRandomRotation();
        var identity = manager.Assets.NetworkSpawn(gameInstance.monsterCharacterEntityPrefab.gameObject, spawnPosition, spawnRotation);
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
        entity.spawnArea = this;
        entity.spawnPosition = spawnPosition;
    }

    public Vector3 GetRandomPosition()
    {
        var randomedPosition = Random.insideUnitSphere * randomRadius;
        randomedPosition = transform.position + new Vector3(randomedPosition.x, 0, randomedPosition.z);
        return randomedPosition;
    }

    public Quaternion GetRandomRotation()
    {
        var randomedRotation = Vector3.up * Random.Range(0, 360);
        return Quaternion.Euler(randomedRotation);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, randomRadius);
    }
}
