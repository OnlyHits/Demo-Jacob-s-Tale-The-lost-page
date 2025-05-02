using DG.Tweening;
using UnityEngine;

namespace CustomArchitecture
{
    public static class RectTransformUtils
    {
        // todo : maybe do another class with tween
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
    }
}