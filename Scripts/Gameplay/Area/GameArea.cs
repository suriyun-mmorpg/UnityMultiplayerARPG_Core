using UnityEngine;

namespace MultiplayerARPG
{
    public enum GameAreaType
    {
        Radius,
        Square,
    }

    public class GameArea : MonoBehaviour
    {
        public const float GROUND_DETECTION_DISTANCE = 100f;
        public const float GROUND_DETECTION_Y_OFFSETS = 3f;
        public const int FIND_GROUND_RAYCAST_HIT_SIZE = 10;
        private static readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[FIND_GROUND_RAYCAST_HIT_SIZE];
        public Color gizmosColor = Color.magenta;
        public GameAreaType type;
        [Header("Radius Area")]
        public float randomRadius = 5f;
        [Header("Square Area")]
        public float squareSizeX;
        public float squareSizeZ;
        public float squareGizmosHeight = 5f;

        protected GameInstance CurrentGameInstance { get { return GameInstance.Singleton; } }

        public Vector3 GetRandomPosition()
        {
            Vector3 randomedPosition = transform.position;

            switch (GameInstance.Singleton.DimensionType)
            {
                case DimensionType.Dimension3D:
                    switch (type)
                    {
                        case GameAreaType.Radius:
                            randomedPosition += new Vector3(Random.Range(-1f, 1f) * randomRadius, GROUND_DETECTION_Y_OFFSETS, Random.Range(-1f, 1f) * randomRadius);
                            break;
                        case GameAreaType.Square:
                            randomedPosition += new Vector3(Random.Range(-0.5f, 0.5f) * squareSizeX, GROUND_DETECTION_Y_OFFSETS, Random.Range(-0.5f, 0.5f) * squareSizeZ);
                            break;
                    }
                    randomedPosition = PhysicUtils.FindGroundedPosition(randomedPosition, findGroundRaycastHits, GROUND_DETECTION_DISTANCE, GroundLayerMask);
                    break;
                case DimensionType.Dimension2D:
                    switch (type)
                    {
                        case GameAreaType.Radius:
                            randomedPosition += new Vector3(Random.Range(-1f, 1f) * randomRadius, Random.Range(-1f, 1f) * randomRadius);
                            break;
                        case GameAreaType.Square:
                            randomedPosition += new Vector3(Random.Range(-0.5f, 0.5f) * squareSizeX, Random.Range(-0.5f, 0.5f) * squareSizeZ);
                            break;
                    }
                    break;
            }

            return randomedPosition;
        }

        public Quaternion GetRandomRotation()
        {
            if (GameInstance.Singleton.DimensionType == DimensionType.Dimension3D)
                return Quaternion.Euler(Vector3.up * Random.Range(0, 360));
            return Quaternion.identity;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = gizmosColor;
            switch (type)
            {
                case GameAreaType.Radius:
                    Gizmos.DrawWireSphere(transform.position, randomRadius);
                    break;
                case GameAreaType.Square:
                    Gizmos.DrawWireCube(transform.position + Vector3.up * squareGizmosHeight * 0.5f * 0.5f, new Vector3(squareSizeX, squareGizmosHeight * 0.5f, squareSizeZ));
                    Gizmos.DrawWireCube(transform.position + Vector3.down * squareGizmosHeight * 0.5f * 0.5f, new Vector3(squareSizeX, squareGizmosHeight * 0.5f, squareSizeZ));
                    break;
            }
        }
#endif

        public virtual int GroundLayerMask { get { return -1; } }
    }
}
