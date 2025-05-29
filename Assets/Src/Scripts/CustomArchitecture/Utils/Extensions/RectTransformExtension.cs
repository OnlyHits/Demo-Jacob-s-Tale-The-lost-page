using DG.Tweening;
using UnityEngine;

namespace CustomArchitecture
{
    public static class RectTransformExtension
    {
        /// <summary>
        /// Copies rect transfrom anchors, size and position
        /// </summary>
        /// <param name="copyTo"></param>
        /// <param name="copyFrom"></param>
        public static void CopyRectTransform(this RectTransform copyTo, RectTransform copyFrom)
        {
            copyTo.anchorMin = copyFrom.anchorMin;
            copyTo.anchorMax = copyFrom.anchorMax;
            copyTo.anchoredPosition = copyFrom.anchoredPosition;
            copyTo.sizeDelta = copyFrom.sizeDelta;
        }

        /// <summary>
        /// Get pivot position in world space
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Vector3 GetPivotInWorldSpace(this RectTransform source)
        {
            // Rewrite Rect.NormalizedToPoint without any clamping.
            Vector2 pivot = new Vector2(
                source.rect.xMin + source.pivot.x * source.rect.width,
                source.rect.yMin + source.pivot.y * source.rect.height);
            // Apply scaling and rotations.
            return source.TransformPoint(new Vector3(pivot.x, pivot.y, 0f));
        }

        /// <summary>
        /// Set pivot position in world coordinate, without moving the position
        /// </summary>
        /// <param name="source"></param>
        /// <param name="pivot"></param>
        /// <returns></returns>
        public static void SetPivotInWorldSpace(this RectTransform source, Vector3 pivot)
        {
            // Strip scaling and rotations.
            pivot = source.InverseTransformPoint(pivot);
            Vector2 pivot2 = new Vector2(
                (pivot.x - source.rect.xMin) / source.rect.width,
                (pivot.y - source.rect.yMin) / source.rect.height);

            // Now move the pivot, keeping and restoring the position which is based on it.
            Vector2 offset = pivot2 - source.pivot;
            offset.Scale(source.rect.size);
            Vector3 worldPos = source.position + source.TransformVector(offset);
            source.pivot = pivot2;
            source.position = worldPos;
        }

        /// <summary>
        /// Get world bounds
        /// </returns>
        public static Bounds GetWorldBounds(this RectTransform rectTransform)
        {
            if (rectTransform == null) return new Bounds();

            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);

            Vector3 min = corners[0];
            Vector3 max = corners[0];
            for (int i = 1; i < 4; i++)
            {
                min = Vector3.Min(min, corners[i]);
                max = Vector3.Max(max, corners[i]);
            }

            return new Bounds((min + max) / 2f, max - min);
        }
    }
}