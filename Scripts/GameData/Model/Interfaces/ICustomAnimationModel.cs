using UnityEngine;

namespace MultiplayerARPG
{
    public interface ICustomAnimationModel
    {
        void PlayCustomAnimation(int id);
        void StopCustomAnimation();
        AnimationClip GetCustomAnimationClip(int id);
    }
}
