using UnityEngine;

namespace MultiplayerARPG
{
    /// <summary>One movement step of supporting contact; never retains a platform without renewed contact.</summary>
    public sealed class EntityMovementPlatform
    {
        private Transform _support;
        private Vector3 _localPoint;
        private Vector3 _worldPoint;
        private float _normalY;

        public Transform Support => _support;

        public void Reset()
        {
            _support = null;
            _normalY = 0f;
        }

        public void RecordContact(Vector3 point, Vector3 normal, Transform support, float minimumNormalY)
        {
            // A wall or chassis side must not become a platform or erase a floor contact.
            if (support == null || normal.y <= 0f || normal.y < minimumNormalY || normal.y < _normalY)
                return;
            _support = support;
            _normalY = normal.y;
            _localPoint = support.InverseTransformPoint(point);
            _worldPoint = point;
        }

        public Vector3 ConsumeVelocity(bool grounded, float deltaTime)
        {
            Vector3 velocity = Vector3.zero;
            if (grounded && deltaTime > 0f && _support != null && _support.gameObject.activeInHierarchy)
                velocity = (_support.TransformPoint(_localPoint) - _worldPoint) / deltaTime;
            // Move must report a new supporting contact, including when it hits nothing.
            Reset();
            return velocity;
        }
    }
}
