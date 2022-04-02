using UnityEngine;

namespace Toggle.Utils
{
    public static class Vector2IntUtils
    {
        public static Vector3 ToVector3(this Vector2Int vi)
        {
            return new Vector3(vi.x, vi.y, 0);
        }
    }
}