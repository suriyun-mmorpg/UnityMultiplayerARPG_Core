using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class EntityMovementForceApplier
    {
        [SerializeField]
        private float speed = 20f;
        [SerializeField]
        private float minSpeed = 4f;
        [SerializeField]
        private float deceleration = 20f;

        public Vector3 Direction { get; set; }
        public float CurrentSpeed { get; set; }
        public float MinSpeed { get; set; }
        public float Deceleration { get; set; }
        public Vector3 Velocity { get => Direction * CurrentSpeed; }

        public void Apply(Vector3 direction)
        {
            Direction = direction;
            CurrentSpeed = speed;
            MinSpeed = minSpeed;
            Deceleration = deceleration;
        }

        public bool Update(float deltaTime)
        {
            if (CurrentSpeed <= 0f)
                return false;
            CurrentSpeed -= deltaTime * Deceleration;
            if (CurrentSpeed < MinSpeed)
                CurrentSpeed = 0f;
            return CurrentSpeed > 0f;
        }
    }
}