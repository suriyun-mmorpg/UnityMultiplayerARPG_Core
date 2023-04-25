using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IBuiltInEntityMovement3D
    {
        bool WaterCheck(Collider waterCollider);
        bool GroundCheck();
        void Move(Vector3 motion);
        Bounds GetBounds();
    }
}
