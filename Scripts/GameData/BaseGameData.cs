using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGameData : ScriptableObject
{
    public string title;
    [TextArea]
    public string description;
    public Sprite icon;
}
