using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class GameplayUtils
    {
        public static DirectionType GetDirectionTypeByVector2(Vector2 direction)
        {
            Vector2 normalized = direction.normalized;
            float absX = Mathf.Abs(normalized.x);
            float absY = Mathf.Abs(normalized.y);
            if (absX / absY > 0.8f)
            {
                if (normalized.x < 0) return DirectionType.Left;
                if (normalized.x > 0) return DirectionType.Right;
            }
            else if (absY / absX > 0.8f)
            {
                if (normalized.y < 0) return DirectionType.Down;
                if (normalized.y > 0) return DirectionType.Up;
            }
            else
            {
                DirectionType result = DirectionType.Down;
                if (normalized.x > 0.01f)
                {
                    result = DirectionType.Left;
                    if (normalized.y > 0.01f)
                        result |= DirectionType.Up;
                    if (normalized.y < -0.01f)
                        result |= DirectionType.Down;
                }
                else if (normalized.x < -0.01f)
                {
                    result = DirectionType.Right;
                    if (normalized.y > 0.01f)
                        result |= DirectionType.Up;
                    if (normalized.y < -0.01f)
                        result |= DirectionType.Down;
                }
                else if (normalized.y > 0.01f)
                {
                    result = DirectionType.Up;
                }
                return result;
            }
            return DirectionType.Down;
        }
    }
}
