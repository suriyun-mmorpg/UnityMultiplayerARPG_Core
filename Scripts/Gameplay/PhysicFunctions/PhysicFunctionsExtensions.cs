using UnityEngine;

namespace MultiplayerARPG
{
    public static class PhysicFunctionsExtensions
    {
        public static bool OverlapEntity<T>(this IPhysicFunctions functions, T entity, Vector3 position, float radius, int layerMask) where T : BaseGameEntity
        {
            int count = functions.OverlapObjects(position, radius, layerMask);
            GameObject obj;
            IGameEntity comp;
            for (int i = 0; i < count; ++i)
            {
                obj = functions.GetOverlapObject(i);
                comp = obj.GetComponent<IGameEntity>();
                if (comp != null && comp.Entity == entity)
                    return true;
            }
            return false;
        }
    }
}
