using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibHighLevel;

public class MonsterSpawnArea : MonoBehaviour
{
    public MonsterCharacterDatabase database;
    public int level = 1;
    public int amount = 1;
    public float randomRadius = 5f;

    public void RandomSpawn(LiteNetLibGameManager manager)
    {
        if (database == null)
        {
            Debug.LogWarning("Have to set monster database to spawn monster");
            return;
        }
        var gameInstance = GameInstance.Singleton;
        var databaseId = database.Id;
        MonsterCharacterDatabase foundDatabase;
        if (!GameInstance.MonsterCharacterDatabases.TryGetValue(databaseId, out foundDatabase))
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
            entity.Id = System.Guid.NewGuid().ToString();
            entity.DatabaseId = databaseId;
            entity.Level = level;
            var attributes = database.attributes;
            foreach (var attribute in attributes)
            {
                var characterAttribute = new CharacterAttribute();
                characterAttribute.attributeId = attribute.attribute.Id;
                characterAttribute.amount = attribute.GetAmount(level);
                entity.Attributes.Add(characterAttribute);
            }
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
