using UnityEngine;

namespace MiniGolf.Helpers
{
    public static class MathUtils
    {
        /// <summary>
        /// Project a screen position to a horizontal plane at planeY.
        /// If camera ray is parallel returns ray origin.
        /// </summary>
        public static Vector3 ScreenToWorldPlane(Vector2 screenPosition, Camera cam, float planeY = 0f)
        {
            Ray ray = cam.ScreenPointToRay(screenPosition);
            if (Mathf.Approximately(ray.direction.y, 0f)) return ray.origin;
            float t = (planeY - ray.origin.y) / ray.direction.y;
            return ray.origin + ray.direction * t;
        }
    }
}
