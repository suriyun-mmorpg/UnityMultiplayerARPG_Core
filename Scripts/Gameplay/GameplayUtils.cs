using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG
{
    public static class GameplayUtils
    {
        public static DirectionType2D GetDirectionTypeByVector2(Vector2 direction)
        {
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);
            if (absX / absY > 0.8f)
            {
                if (direction.x < 0) return DirectionType2D.Left;
                if (direction.x > 0) return DirectionType2D.Right;
            }
            else if (absY / absX > 0.8f)
            {
                if (direction.y < 0) return DirectionType2D.Down;
                if (direction.y > 0) return DirectionType2D.Up;
            }
            else
            {
                DirectionType2D result = DirectionType2D.Down;
                if (direction.x > 0.01f)
                {
                    result = DirectionType2D.Left;
                    if (direction.y > 0.01f)
                        result |= DirectionType2D.Up;
                    if (direction.y < -0.01f)
                        result |= DirectionType2D.Down;
                }
                else if (direction.x < -0.01f)
                {
                    result = DirectionType2D.Right;
                    if (direction.y > 0.01f)
                        result |= DirectionType2D.Up;
                    if (direction.y < -0.01f)
                        result |= DirectionType2D.Down;
                }
                else if (direction.y > 0.01f)
                {
                    result = DirectionType2D.Up;
                }
                return result;
            }
            return DirectionType2D.Down;
        }
    }
}
