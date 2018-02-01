using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLoadAllCharacter : MonoBehaviour {
	void Start () {
        var characters = CharacterDataExtension.LoadAllPersistentCharacterData();
        foreach (var character in characters)
        {
            var entity = Instantiate(GameInstance.Singleton.characterEntityPrefab);
            entity.gameObject.SetActive(false);
            character.CloneTo(entity);
            foreach (var attributeLevel in entity.AttributeLevels)
            {
                Debug.Log("Found attribute " + attributeLevel.attributeId + " " + attributeLevel.amount);
            }
        }
	}
}
