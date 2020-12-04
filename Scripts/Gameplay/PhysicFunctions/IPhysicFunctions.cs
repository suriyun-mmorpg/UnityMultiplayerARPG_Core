using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IPhysicFunctions
    {
        int Raycast(Vector3 origin, Vector3 direction, float distance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        int RaycastPickObjects(Camera camera, Vector3 mousePosition, int layerMask, float distance, out Vector3 raycastPosition, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        int RaycastDown(Vector3 position, int layerMask, float distance = 100f, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        bool GetRaycastIsTrigger(int index);

        Vector3 GetRaycastPoint(int index);

        Vector3 GetRaycastNormal(int index);

        float GetRaycastDistance(int index);

        Transform GetRaycastTransform(int index);

        Transform GetRaycastColliderTransform(int index);

        GameObject GetRaycastObject(int index);

        GameObject GetRaycastColliderGameObject(int index);

        int OverlapObjects(Vector3 position, float radius, int layerMask, bool sort = false, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        GameObject GetOverlapObject(int index);
    }
}
