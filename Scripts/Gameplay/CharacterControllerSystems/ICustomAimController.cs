using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface ICustomAimController
    {
        bool HasCustomAimControls();
        Vector3? UpdateAimControls(Vector2 aimAxes, params object[] data);
        void FinishAimControls(bool isCancel);
    }
}
