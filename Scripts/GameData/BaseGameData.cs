using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameData : ScriptableObject
{
    public string title;
    [TextArea]
    public string description;
    public Sprite icon;

    public string Id { get { return name; } }
    public int HashId { get { return GenerateHashId(Id); } }

    public static int GenerateHashId(string id)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < id.Length && id[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ id[i];
                if (i == id.Length - 1 || id[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ id[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}
