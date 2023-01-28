using UnityEngine;

namespace MultiplayerARPG
{
    public interface ISkillAimController
    {
        AimPosition UpdateAimControls(Vector2 aimAxes, params object[] data);
        void FinishAimControls(bool isCancel);
    }
}
