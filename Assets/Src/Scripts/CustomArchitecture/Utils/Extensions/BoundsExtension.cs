using System.Drawing;
using UnityEngine;

namespace CustomArchitecture
{
    public static class BoundsExtension
    {
        /// <summary>
        /// Is x/y position between bounds min and max
        /// </summary>
        /// <returns>true if inside, false otherwise</returns>
        public static bool Contain2D(this Bounds bounds, Vector2 point)
        {
            return point.x >= bounds.min.x && point.x <= bounds.max.x &&
                   point.y >= bounds.min.y && point.y <= bounds.max.y;
        }
    }
}