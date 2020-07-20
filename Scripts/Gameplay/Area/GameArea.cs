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
        public const int FIND_GROUND_RAYCAST_HIT_SIZE = 10;
        private static readonly RaycastHit[] findGroundRaycastHits = new RaycastHit[FIND_GROUND_RAYCAST_HIT_SIZE];
        public Color gizmosColor = Color.magenta;
        public GameAreaType type;
        [Header("Radius Area")]
        public float randomRadius = 5f;
        [Header("Square Area")]
        public float squareSizeX;
        public float squareSizeZ;

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
                            randomedPosition = Random.insideUnitSphere * randomRadius;
                            break;
                        case GameAreaType.Square:
                            randomedPosition = new Vector3(Random.Range(-squareSizeX * 0.5f, squareSizeX * 0.5f), 0, Random.Range(-squareSizeZ * 0.5f, squareSizeZ * 0.5f));
                            break;
                    }
                    randomedPosition = PhysicUtils.FindGroundedPosition(transform.position + new Vector3(randomedPosition.x, 0, randomedPosition.z), findGroundRaycastHits, GROUND_DETECTION_DISTANCE, GroundLayerMask);
                    break;
                case DimensionType.Dimension2D:
                    switch (type)
                    {
                        case GameAreaType.Radius:
                            randomedPosition = Random.insideUnitCircle * randomRadius;
                            break;
                        case GameAreaType.Square:
                            randomedPosition = new Vector3(Random.Range(-squareSizeX * 0.5f, squareSizeX * 0.5f), Random.Range(-squareSizeZ * 0.5f, squareSizeZ * 0.5f));
                            break;
                    }
                    randomedPosition = transform.position + new Vector3(randomedPosition.x, randomedPosition.y);
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
                    float height = (squareSizeX + squareSizeZ) / 2;
                    Gizmos.DrawWireCube(transform.position, new Vector3(squareSizeX, height, squareSizeZ));
                    break;
            }
        }
#endif

        public virtual int GroundLayerMask { get { return -1; } }
    }
}
