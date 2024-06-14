using UnityEngine;

namespace Echoin.Utility
{
    internal static class UnityUtil
    {
        public static Vector3 WithX(this in Vector3 vector, float x) => new(x, vector.y, vector.z);

        public static Vector3 WithY(this in Vector3 vector, float y) => new(vector.x, y, vector.z);

        public static Vector3 WithZ(this in Vector3 vector, float z) => new(vector.x, vector.y, z);

        public static Color WithAlpha(this in Color color, float alpha) => new(color.r, color.g, color.b, alpha);
    }
}
