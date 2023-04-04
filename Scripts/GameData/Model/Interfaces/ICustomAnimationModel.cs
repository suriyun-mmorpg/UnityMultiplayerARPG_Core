using UnityEngine;

namespace MultiplayerARPG
{
    public interface ICustomAnimationModel
    {
        void PlayCustomAnimation(int id);
        AnimationClip GetCustomAnimationClip(int id);
    }
}
