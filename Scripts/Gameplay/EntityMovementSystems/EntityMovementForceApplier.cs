using System.Collections.Generic;
using LiteNetLib.Utils;
using UnityEngine;

namespace MultiplayerARPG
{
    [System.Serializable]
    public class EntityMovementForceApplier : INetSerializable
    {
        [SerializeField]
        private ApplyMovementForceMode mode;
        [SerializeField]
        [Tooltip("Speed when apply then current speed will be decreased by deceleration * delta time")]
        private float speed = 20f;
        [SerializeField]
        [Tooltip("Current speed will be decreased by this value * delta time, you can set this to 0 to make speed not decrease (but you should set duration more than 0)")]
        private float deceleration = 20f;
        [SerializeField]
        [Tooltip("If duration <= 0, then it is no duration, it will stop applying when current speed <= 0")]
        private float duration = 0f;

        public Vector3 Direction { get; set; }
        public ApplyMovementForceMode Mode { get; set; }
        public float CurrentSpeed { get; set; }
        public float Deceleration { get; set; }
        public float Duration { get; set; }
        public float Elasped { get; set; }
        public Vector3 Velocity { get => Direction * CurrentSpeed; }

        public EntityMovementForceApplier Apply(Vector3 direction)
        {
            Direction = direction;
            Mode = mode;
            CurrentSpeed = speed;
            Deceleration = deceleration;
            Duration = duration;
            Elasped = 0f;
            return this;
        }

        public EntityMovementForceApplier Apply(Vector3 direction, ApplyMovementForceMode mode, float speed, float deceleration, float duration)
        {
            Apply(direction);
            Mode = mode;
            CurrentSpeed = speed;
            Deceleration = deceleration;
            Duration = duration;
            return this;
        }

        public void Deserialize(NetDataReader reader)
        {
            Mode = (ApplyMovementForceMode)reader.GetByte();
            Direction = reader.GetVector3();
            CurrentSpeed = reader.GetFloat();
            Deceleration = reader.GetFloat();
            Elasped = reader.GetFloat();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)Mode);
            writer.PutVector3(Direction);
            writer.Put(CurrentSpeed);
            writer.Put(Deceleration);
            writer.Put(Elasped);
        }

        public bool Update(float deltaTime)
        {
            if (CurrentSpeed <= 0f)
                return false;
            CurrentSpeed -= deltaTime * Deceleration;
            Elasped += deltaTime;
            if (Duration > 0f && Elasped >= Duration)
            {
                Elasped = Duration;
                CurrentSpeed = 0f;
            }
            return CurrentSpeed > 0f;
        }
    }


    public static class EntityMovementForceApplierExtensions
    {
        public static void RemoveReplaceMovementForces(this IList<EntityMovementForceApplier> forceAppliers)
        {
            for (int i = forceAppliers.Count - 1; i >= 0; --i)
            {
                if (forceAppliers[i].Mode.IsReplaceMovement())
                    forceAppliers.RemoveAt(i);
            }
        }

        public static void UpdateForces(this IList<EntityMovementForceApplier> forceAppliers, float deltaTime, float characterMoveSpeed, out Vector3 forceMotion, out EntityMovementForceApplier replaceMovementForceApplier)
        {
            forceMotion = Vector3.zero;
            replaceMovementForceApplier = null;
            for (int i = forceAppliers.Count - 1; i >= 0; --i)
            {
                if (!forceAppliers[i].Update(deltaTime) || forceAppliers[i].CurrentSpeed < characterMoveSpeed)
                {
                    forceAppliers.RemoveAt(i);
                    continue;
                }
                if (!forceAppliers[i].Mode.IsReplaceMovement())
                    forceMotion += forceAppliers[i].Velocity;
                else
                    replaceMovementForceApplier = forceAppliers[i];
            }
        }
    }
}