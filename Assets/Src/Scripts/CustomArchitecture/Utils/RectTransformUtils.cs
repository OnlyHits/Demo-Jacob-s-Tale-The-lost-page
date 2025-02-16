using DG.Tweening;
using UnityEngine;

namespace CustomArchitecture
{
    public static class RectTransformUtils
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


        #region TWEEN

        public static Tween m_anchorTween = null;
        public static Tween m_sizeTween = null;

        /// <summary>
        /// Copies rect transfrom anchors, size and position by tween
        /// </summary>
        /// <param name="copyTo"></param>
        /// <param name="copyFrom"></param>
        /// <param name="duration"></param>
        /// <param name="ease"></param>
        /// <param name="snap"></param>
        public static void DORectTransform(this RectTransform copyTo, RectTransform copyFrom, float duration, Ease ease = Ease.Linear, bool snap = false)
        {
            // copyTo.anchorMin = copyFrom.anchorMin;
            // copyTo.anchorMax = copyFrom.anchorMax;

            var destAnchor = copyFrom.anchoredPosition;
            var fromAnchor = copyTo.anchoredPosition;
            //copyTo.anchoredPosition = copyFrom.anchoredPosition;

            var destSize = copyFrom.sizeDelta;
            var fromSize = copyTo.sizeDelta;
            //copyTo.sizeDelta = copyFrom.sizeDelta;

            DOTween.To(() => fromAnchor, x => copyTo.anchoredPosition = x, destAnchor, duration);
            DOTween.To(() => fromSize, x => copyTo.sizeDelta = x, destSize, duration);
        }

        public static void DOKillRect(this RectTransform _)
        {
            m_anchorTween?.Kill();
            m_sizeTween?.Kill();
        }

        #endregion TWEEN


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

        // Set the RectTransform's pivot point in world coordinates, without moving the position.
        // This is like dragging the pivot handle in the editor.
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
    }
}