using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class EntityMovementForceApplier
    {
        [SerializeField]
        [Tooltip("Speed when apply then current speed will be decreased by deceleration * delta time")]
        private float speed = 20f;
        [SerializeField]
        [Tooltip("If current speed is less than this value, it will stop applying immediately (by set current speed to 0)")]
        private float minSpeed = 4f;
        [SerializeField]
        [Tooltip("Current speed will be decreased by this value * delta time, you can set this to 0 to make speed not decrease (but you should set duration more than 0)")]
        private float deceleration = 20f;
        [SerializeField]
        [Tooltip("If duration <= 0, then it is no duration, it will stop applying when current speed <= 0")]
        private float duration = 0f;

        public Vector3 Direction { get; set; }
        public float CurrentSpeed { get; set; }
        public float MinSpeed { get; set; }
        public float Deceleration { get; set; }
        public float Duration { get; set; }
        public float Elasped { get; set; }
        public Vector3 Velocity { get => Direction * CurrentSpeed; }

        public void Apply(Vector3 direction)
        {
            Direction = direction;
            CurrentSpeed = speed;
            MinSpeed = minSpeed;
            Deceleration = deceleration;
            Duration = duration;
            Elasped = 0f;
        }

        public bool Update(float deltaTime)
        {
            if (CurrentSpeed <= 0f)
                return false;
            CurrentSpeed -= deltaTime * Deceleration;
            Elasped += deltaTime;
            if (CurrentSpeed < MinSpeed)
            {
                CurrentSpeed = 0f;
            }
            if (Duration > 0f && Elasped >= Duration)
            {
                Elasped = Duration;
                CurrentSpeed = 0f;
            }
            return CurrentSpeed > 0f;
        }
    }
}