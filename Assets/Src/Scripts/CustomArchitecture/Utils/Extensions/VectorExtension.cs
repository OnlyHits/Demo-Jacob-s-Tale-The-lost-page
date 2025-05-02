//using System;
//using System.Collections.Generic;
using UnityEngine;

namespace CustomArchitecture
{
    public static class VectorExtension
    {
        public static Vector2 Abs(this Vector2 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        public static Vector3 Abs(this Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
        public static Vector4 Abs(this Vector4 v) => new Vector4(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z), Mathf.Abs(v.w));

        public static Vector2 Clamp(this Vector2 value, Vector2 min, Vector2 max) =>
            new Vector2(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y));

        public static Vector3 Clamp(this Vector3 value, Vector3 min, Vector3 max) =>
            new Vector3(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y), Mathf.Clamp(value.z, min.z, max.z));

        public static Vector4 Clamp(this Vector4 value, Vector4 min, Vector4 max) =>
            new Vector4(
                Mathf.Clamp(value.x, min.x, max.x),
                Mathf.Clamp(value.y, min.y, max.y),
                Mathf.Clamp(value.z, min.z, max.z),
                Mathf.Clamp(value.w, min.w, max.w));
    }
}