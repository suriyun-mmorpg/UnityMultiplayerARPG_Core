using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class GameplayUtils
    {
        public static DirectionType2D GetDirectionTypeByVector2(Vector2 direction)
        {
            Vector2 normalized = direction.normalized;
            float absX = Mathf.Abs(normalized.x);
            float absY = Mathf.Abs(normalized.y);
            if (absX / absY > 0.8f)
            {
                if (normalized.x < 0) return DirectionType2D.Left;
                if (normalized.x > 0) return DirectionType2D.Right;
            }
            else if (absY / absX > 0.8f)
            {
                if (normalized.y < 0) return DirectionType2D.Down;
                if (normalized.y > 0) return DirectionType2D.Up;
            }
            else
            {
                DirectionType2D result = DirectionType2D.Down;
                if (normalized.x > 0.01f)
                {
                    result = DirectionType2D.Left;
                    if (normalized.y > 0.01f)
                        result |= DirectionType2D.Up;
                    if (normalized.y < -0.01f)
                        result |= DirectionType2D.Down;
                }
                else if (normalized.x < -0.01f)
                {
                    result = DirectionType2D.Right;
                    if (normalized.y > 0.01f)
                        result |= DirectionType2D.Up;
                    if (normalized.y < -0.01f)
                        result |= DirectionType2D.Down;
                }
                else if (normalized.y > 0.01f)
                {
                    result = DirectionType2D.Up;
                }
                return result;
            }
            return DirectionType2D.Down;
        }
    }
}
