using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicUtils
{
    public static int SortedOverlapCircleNonAlloc(Vector3 position, float distance, Collider2D[] colliders, int layerMask)
    {
        int count = Physics2D.OverlapCircleNonAlloc(position, distance, colliders, layerMask);
        System.Array.Sort(colliders, 0, count, new ColliderComparer(position));
        return count;
    }

    public static int SortedOverlapSphereNonAlloc(Vector3 position, float distance, Collider[] colliders, int layerMask)
    {
        int count = Physics.OverlapSphereNonAlloc(position, distance, colliders, layerMask);
        System.Array.Sort(colliders, 0, count, new ColliderComparer(position));
        return count;
    }

    public static int SortedRaycastNonAlloc2D(Ray2D ray, RaycastHit2D[] hits, float distance, int layerMask)
    {
        return SortedRaycastNonAlloc2D(ray.origin, ray.direction, hits, distance, layerMask);
    }

    public static int SortedRaycastNonAlloc2D(Vector3 origin, Vector3 direction, RaycastHit2D[] hits, float distance, int layerMask)
    {
        int count = Physics2D.RaycastNonAlloc(origin, direction, hits, distance, layerMask);
        System.Array.Sort(hits, 0, count, new RaycastHitComparer());
        return count;
    }

    public static int SortedRaycastNonAlloc3D(Ray ray, RaycastHit[] hits, float distance, int layerMask)
    {
        return SortedRaycastNonAlloc3D(ray.origin, ray.direction, hits, distance, layerMask);
    }

    public static int SortedRaycastNonAlloc3D(Vector3 origin, Vector3 direction, RaycastHit[] hits, float distance, int layerMask)
    {
        int count = Physics.RaycastNonAlloc(origin, direction, hits, distance, layerMask);
        System.Array.Sort(hits, 0, count, new RaycastHitComparer());
        return count;
    }

    public static int SortedLinecastNonAlloc2D(Vector2 start, Vector2 end, RaycastHit2D[] hits, int layerMask)
    {
        int count = Physics2D.LinecastNonAlloc(start, end, hits, layerMask);
        System.Array.Sort(hits, 0, count, new RaycastHitComparer());
        return count;
    }

    public static Vector3 FindGroundedPosition(Vector3 origin, RaycastHit[] results, float distance, int layerMask)
    {
        // Raycast to find hit floor
        Vector3? aboveHitPoint = null;
        Vector3? underHitPoint = null;
        int hitCount = Physics.RaycastNonAlloc(origin, Vector3.up, results, distance, layerMask);
        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; ++i)
            {
                if (!aboveHitPoint.HasValue || aboveHitPoint.Value.y < results[i].point.y)
                    aboveHitPoint = results[i].point;
            }
        }
        hitCount = Physics.RaycastNonAlloc(origin, Vector3.down, results, distance, layerMask);
        if (hitCount > 0)
        {
            for (int i = 0; i < hitCount; ++i)
            {
                if (!underHitPoint.HasValue || underHitPoint.Value.y < results[i].point.y)
                    underHitPoint = results[i].point;
            }
        }
        // Set drop position to nearest hit point
        if (aboveHitPoint.HasValue && underHitPoint.HasValue)
        {
            if (Vector3.Distance(origin, aboveHitPoint.Value) < Vector3.Distance(origin, underHitPoint.Value))
                origin = aboveHitPoint.Value;
            else
                origin = underHitPoint.Value;
        }
        else if (aboveHitPoint.HasValue)
            origin = aboveHitPoint.Value;
        else if (underHitPoint.HasValue)
            origin = underHitPoint.Value;
        return origin;
    }

    public struct ColliderComparer : IComparer<Collider>, IComparer<Collider2D>
    {
        private Vector3 position;
        public ColliderComparer(Vector3 position)
        {
            this.position = position;
        }

        public int Compare(Collider x, Collider y)
        {
            return Vector3.Distance(position, x.transform.position)
                .CompareTo(Vector3.Distance(position, y.transform.position));
        }

        public int Compare(Collider2D x, Collider2D y)
        {
            return Vector3.Distance(position, x.transform.position)
                .CompareTo(Vector3.Distance(position, y.transform.position));
        }
    }

    public struct RaycastHitComparer : IComparer<RaycastHit>, IComparer<RaycastHit2D>
    {
        public int Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance.CompareTo(y.distance);
        }

        public int Compare(RaycastHit2D x, RaycastHit2D y)
        {
            return x.distance.CompareTo(y.distance);
        }
    }
}
