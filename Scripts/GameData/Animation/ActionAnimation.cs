using UnityEngine;

[System.Serializable]
public class ActionAnimation
{
    public int actionId;
    [Range(0f, 1f)]
    public float triggerDurationRate;
    public float totalDuration;

    public float triggerDuration
    {
        get { return totalDuration * triggerDurationRate; }
    }
}
