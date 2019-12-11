using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PhysicUtils
{
    public static int SortedRaycastNonAlloc2D(Vector3 origin, Vector3 direction, RaycastHit2D[] hits, float distance, int layerMask)
    {
        int count = Physics2D.RaycastNonAlloc(origin, direction, hits, distance, layerMask);
        System.Array.Sort(hits, 0, count, new RaycastHitComparer());
        return count;
    }

    public static int SortedRaycastNonAlloc3D(Vector3 origin, Vector3 direction, RaycastHit[] hits, float distance, int layerMask)
    {
        int count = Physics.RaycastNonAlloc(origin, direction, hits, distance, layerMask);
        System.Array.Sort(hits, 0, count, new RaycastHitComparer());
        return count;
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
