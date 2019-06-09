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
        public const float GROUND_DETECTION_DISTANCE = 512f;
        public Color gizmosColor = Color.magenta;
        public GameAreaType type;
        [Header("Radius Area")]
        public float randomRadius = 5f;
        [Header("Square Area")]
        public float squareSizeX;
        public float squareSizeZ;

        protected GameInstance gameInstance { get { return GameInstance.Singleton; } }

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
                            randomedPosition = new Vector3(Random.Range(-squareSizeX / 2f, squareSizeX / 2f), 0, Random.Range(-squareSizeZ / 2f, squareSizeZ / 2f));
                            break;
                    }
                    randomedPosition = transform.position + new Vector3(randomedPosition.x, 0, randomedPosition.z);

                    // Raycast to find hit floor
                    Vector3? aboveHitPoint = null;
                    Vector3? underHitPoint = null;
                    int raycastLayerMask = GroundLayerMask;
                    RaycastHit tempHit;
                    if (Physics.Raycast(randomedPosition, Vector3.up, out tempHit, GROUND_DETECTION_DISTANCE, raycastLayerMask))
                        aboveHitPoint = tempHit.point;
                    if (Physics.Raycast(randomedPosition, Vector3.down, out tempHit, GROUND_DETECTION_DISTANCE, raycastLayerMask))
                        underHitPoint = tempHit.point;
                    // Set drop position to nearest hit point
                    if (aboveHitPoint.HasValue && underHitPoint.HasValue)
                    {
                        if (Vector3.Distance(randomedPosition, aboveHitPoint.Value) < Vector3.Distance(randomedPosition, underHitPoint.Value))
                            randomedPosition = aboveHitPoint.Value;
                        else
                            randomedPosition = underHitPoint.Value;
                    }
                    else if (aboveHitPoint.HasValue)
                        randomedPosition = aboveHitPoint.Value;
                    else if (underHitPoint.HasValue)
                        randomedPosition = underHitPoint.Value;
                    break;
                case DimensionType.Dimension2D:
                    switch (type)
                    {
                        case GameAreaType.Radius:
                            randomedPosition = Random.insideUnitCircle * randomRadius;
                            break;
                        case GameAreaType.Square:
                            randomedPosition = new Vector3(Random.Range(-squareSizeX / 2f, squareSizeX / 2f), Random.Range(-squareSizeZ / 2f, squareSizeZ / 2f));
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

        public virtual int GroundLayerMask { get { return -1; } }
    }
}
