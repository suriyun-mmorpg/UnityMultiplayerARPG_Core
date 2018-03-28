using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibHighLevel;

public class MonsterSpawnArea : MonoBehaviour
{
    public MonsterCharacterDatabase database;
    public int amount;
    public float randomRadius;

    public void RandomSpawn(LiteNetLibGameManager manager)
    {
        if (database == null)
        {
            Debug.LogWarning("Have to set monster database to spawn monster");
            return;
        }
        var gameInstance = GameInstance.Singleton;
        var databaseId = database.Id;
        BaseCharacterDatabase foundDatabase;
        if (!GameInstance.CharacterDatabases.TryGetValue(databaseId, out foundDatabase))
        {
            Debug.LogWarning("The monster database have to be added to game instance");
            return;
        }
        if (!(foundDatabase is MonsterCharacterDatabase))
        {
            Debug.LogWarning("This is not monster database");
            return;
        }
        for (var i = 0; i < amount; ++i)
        {
            var randomedPosition = Random.insideUnitSphere * randomRadius;
            randomedPosition = transform.position + new Vector3(randomedPosition.x, 0, randomedPosition.z);
            var identity = manager.Assets.NetworkSpawn(gameInstance.monsterCharacterEntityPrefab.gameObject, randomedPosition);
            var entity = identity.GetComponent<MonsterCharacterEntity>();
            entity.DatabaseId = databaseId;
            entity.CurrentHp = entity.GetMaxHp();
            entity.CurrentMp = entity.GetMaxMp();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, randomRadius);
    }
}
