using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public interface IPhysicFunctions
    {
        bool SingleRaycast(Vector3 start, Vector3 end, out PhysicRaycastResult result, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        bool SingleRaycast(Vector3 origin, Vector3 direction, out PhysicRaycastResult result, float distance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        int Raycast(Vector3 start, Vector3 end, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        int Raycast(Vector3 origin, Vector3 direction, float distance, int layerMask, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        int RaycastPickObjects(Camera camera, Vector3 mousePosition, int layerMask, float distance, out Vector3 raycastPosition, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        int RaycastDown(Vector3 position, int layerMask, float distance = 100f, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        bool GetRaycastIsTrigger(int index);

        Vector3 GetRaycastPoint(int index);

        Vector3 GetRaycastNormal(int index);

        Bounds GetRaycastColliderBounds(int index);

        float GetRaycastDistance(int index);

        Transform GetRaycastTransform(int index);

        GameObject GetRaycastObject(int index);

        Vector3 GetRaycastColliderClosestPoint(int index, Vector3 position);

        int OverlapObjects(Vector3 position, float radius, int layerMask, bool sort = false, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.UseGlobal);

        GameObject GetOverlapObject(int index);

        Vector3 GetOverlapColliderClosestPoint(int index, Vector3 position);

        bool GetOverlapColliderRaycast(int index, Vector3 origin, Vector3 direction, out Vector3 point, out Vector3 normal, out float distance, out Transform transform, float maxDistance);
    }
}
