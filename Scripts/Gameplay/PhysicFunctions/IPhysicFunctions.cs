using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IPhysicFunctions
    {
        int Raycast(Vector3 origin, Vector3 direction, float distance, int layerMask);

        int RaycastPickObjects(Camera camera, Vector3 mousePosition, int layerMask, float distance, out Vector3 worldPosition2D);

        int RaycastDown(Vector3 position, int layerMask, float distance = 100f);

        bool GetRaycastIsTrigger(int index);

        Vector3 GetRaycastPoint(int index);

        Vector3 GetRaycastNormal(int index);

        float GetRaycastDistance(int index);

        Transform GetRaycastTransform(int index);

        GameObject GetRaycastObject(int index);

        int OverlapObjects(Vector3 position, float distance, int layerMask, bool sort = false);

        GameObject GetOverlapObject(int index);
    }
}
