using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Animation 2D", menuName = "Animation 2D")]
public class AnimationClip2D : ScriptableObject
{
    public Sprite[] frames;
    public float framesPerSec = 5;
    public bool loop = true;

    public float duration
    {
        get { return frames.Length * framesPerSec; }
        set { framesPerSec = value / frames.Length; }
    }
}
