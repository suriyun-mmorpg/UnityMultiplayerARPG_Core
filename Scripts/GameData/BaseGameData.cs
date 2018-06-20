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
    public int DataId { get { return Id.GenerateHashId(); } }
}
