using UnityEngine;
using UnityEngine.AI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG
{
    public enum GameAreaType
    {
        Radius,
        Square,
    }

    public enum GameAreaGroundFindingType
    {
        NavMesh,
        Raycast,
    }

    public class GameArea : MonoBehaviour
    {
        protected static readonly RaycastHit[] s_findGroundRaycastHits = new RaycastHit[10];
        public float groundDetectionOffsets = 100f;
        public Color gizmosColor = Color.magenta;
        public GameAreaType type;
        [Header("Radius Area")]
        public float randomRadius = 5f;
        [Header("Square Area")]
        public float squareSizeX = 10f;
        public float squareSizeZ = 10f;
        public GameAreaGroundFindingType groundFindingType = GameAreaGroundFindingType.NavMesh;
        public float findGroundUpOffsetsRate = 1f;
        public bool stillUseRandomedPositionIfGroundNotFound = false;

        protected GameInstance CurrentGameInstance { get { return GameInstance.Singleton; } }

        protected IPhysicFunctions _physicFunctions;

        public virtual bool GetRandomPosition(out Vector3 randomedPosition)
        {
            Vector3 randomingPosition = transform.position;
            randomedPosition = randomingPosition;

            switch (GameInstance.Singleton.DimensionType)
            {
                case DimensionType.Dimension3D:
                    switch (type)
                    {
                        case GameAreaType.Radius:
                            randomingPosition += new Vector3(Random.Range(-1f, 1f) * randomRadius, 0f, Random.Range(-1f, 1f) * randomRadius);
                            break;
                        case GameAreaType.Square:
                            randomingPosition += new Vector3(Random.Range(-0.5f, 0.5f) * squareSizeX, 0f, Random.Range(-0.5f, 0.5f) * squareSizeZ);
                            break;
                    }
                    if (FindGroundedPosition(randomingPosition, groundDetectionOffsets, out randomedPosition))
                        return true;
                    if (!stillUseRandomedPositionIfGroundNotFound)
                        return false;
                    randomedPosition = randomingPosition;
                    return true;
                case DimensionType.Dimension2D:
                    switch (type)
                    {
                        case GameAreaType.Radius:
                            randomingPosition += new Vector3(Random.Range(-1f, 1f) * randomRadius, Random.Range(-1f, 1f) * randomRadius);
                            break;
                        case GameAreaType.Square:
                            randomingPosition += new Vector3(Random.Range(-0.5f, 0.5f) * squareSizeX, Random.Range(-0.5f, 0.5f) * squareSizeZ);
                            break;
                    }
                    randomedPosition = randomingPosition;
                    return true;
            }
            return false;
        }

        public virtual Quaternion GetRandomRotation()
        {
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
                return Quaternion.Euler(Vector3.up * Random.Range(0, 360));
            return Quaternion.identity;
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            Color handleCol = Handles.color;
            Color gizmosCol = Gizmos.color;
            Handles.color = gizmosColor;
            Gizmos.color = gizmosColor;
            switch (type)
            {
                case GameAreaType.Radius:
                    Vector3 upOrigin = Vector3.down * (1f - findGroundUpOffsetsRate) * groundDetectionOffsets;
                    Vector3 upDestination = Vector3.up * findGroundUpOffsetsRate * groundDetectionOffsets;
                    Handles.DrawWireDisc(transform.position + upOrigin, Vector3.up, randomRadius);
                    Handles.DrawWireDisc(transform.position + upDestination, Vector3.up, randomRadius);
                    Gizmos.DrawLine(transform.position + (Vector3.left * randomRadius) + upOrigin, transform.position + (Vector3.left * randomRadius) + upDestination);
                    Gizmos.DrawLine(transform.position + (Vector3.right * randomRadius) + upOrigin, transform.position + (Vector3.right * randomRadius) + upDestination);
                    Gizmos.DrawLine(transform.position + (Vector3.forward * randomRadius) + upOrigin, transform.position + (Vector3.forward * randomRadius) + upDestination);
                    Gizmos.DrawLine(transform.position + (Vector3.back * randomRadius) + upOrigin, transform.position + (Vector3.back * randomRadius) + upDestination);
                    break;
                case GameAreaType.Square:
                    Gizmos.DrawWireCube(transform.position + (Vector3.up * findGroundUpOffsetsRate * groundDetectionOffsets * 0.5f), new Vector3(squareSizeX, groundDetectionOffsets, squareSizeZ));
                    break;
            }
            Handles.color = handleCol;
            Gizmos.color = gizmosCol;
        }
#endif

        public bool FindGroundedPosition(Vector3 fromPosition, float findDistance, out Vector3 result)
        {
            return FindGroundedPosition(groundFindingType, GroundLayerMask, fromPosition, findDistance, out result, null, findGroundUpOffsetsRate);
        }

        public static bool FindGroundedPosition(GameAreaGroundFindingType groundFindingType, int groundLayerMask, Vector3 fromPosition, float findDistance, out Vector3 result, Transform excludingObject = null, float findGroundUpOffsetsRate = 0.5f)
        {
            result = fromPosition;
            switch (groundFindingType)
            {
                case GameAreaGroundFindingType.NavMesh:
                    if (NavMesh.SamplePosition(fromPosition, out NavMeshHit navHit, findDistance, NavMesh.AllAreas))
                    {
                        result = navHit.position;
                        return true;
                    }
                    return false;
                default:
                    return PhysicUtils.FindGroundedPosition(fromPosition, s_findGroundRaycastHits, findDistance, groundLayerMask, out result, excludingObject, findGroundUpOffsetsRate);
            }
        }

        public virtual int GroundLayerMask { get { return -1; } }
    }
}
