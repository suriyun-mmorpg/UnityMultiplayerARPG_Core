using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance : MonoBehaviour {
    public static GameInstance Singleton { get; protected set; }
    public CharacterClass[] characterClasses;
    public CharacterAttribute[] characterAttributes;
    public int[] expTree;
    public int increaseStatPointEachLevel = 5;
    public int increaseSkillPointEachLevel = 1;
    public int startGold = 0;
    public static readonly Dictionary<string, CharacterClass> CharacterClasses = new Dictionary<string, CharacterClass>();
    public static readonly Dictionary<string, CharacterAttribute> CharacterAttributes = new Dictionary<string, CharacterAttribute>();

    protected virtual void Awake()
    {
        if (Singleton != null)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Singleton = this;
    }

    public static void AddCharacterClasses(CharacterClass[] characterClasses)
    {
        foreach (var characterClass in characterClasses)
        {
            if (characterClass == null)
                continue;
            CharacterClasses[characterClass.Id] = characterClass;
            var attributes = new CharacterAttribute[characterClass.baseAttributes.Length];
            var i = 0;
            foreach (var baseAttribute in characterClass.baseAttributes)
            {
                if (baseAttribute == null || baseAttribute.attribute == null)
                    continue;
                attributes[i++] = baseAttribute.attribute;
            }
            AddCharacterAttributes(attributes);
        }
    }

    public static void AddCharacterAttributes(CharacterAttribute[] characterAttributes)
    {
        foreach (var characterAttribute in characterAttributes)
        {
            if (characterAttribute == null)
                continue;
            CharacterAttributes[characterAttribute.Id] = characterAttribute;
        }
    }
}
