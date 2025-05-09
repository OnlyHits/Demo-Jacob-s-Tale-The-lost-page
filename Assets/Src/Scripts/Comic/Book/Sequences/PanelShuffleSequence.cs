using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace Comic
{
    public class PanelShuffleSequence
    {
        // this is not really convenient until i dont make it heritated from MonoBehaviour
        // It may be preferable to make this a component but at least no editor setup is required 
        private readonly float  m_downDuration = 3f;
        private readonly float  m_upDuration = 2f;
        private readonly float  m_rotations = 2f;
        private readonly Ease   m_ease = Ease.OutSine;

        private Sequence m_sequence;
        private Vector2 m_center;

        public void Shuffle(List<Transform> target, Vector2 center)
        {
            m_sequence = DOTween.Sequence();
            m_center = center;

            int i = 0;
            foreach (var panel in target)
            {
                Tween t = CreateSpiralTween(panel, i);
                m_sequence.Join(t);
                ++i;
            }

            m_sequence.Play();
        }

        Tween CreateSpiralTween(Transform target, int index)
        {
            Vector3 originalPosition = target.position;
            Vector3 originalScale = target.localScale;

            Vector2 offset = (Vector2)originalPosition - m_center;
            float startAngle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            float radius = offset.magnitude;

            var spiralIn = DOTween.To(
                () => 0f,
                t =>
                {
                    float angle = startAngle + 360f * m_rotations * t;
                    float r = Mathf.Lerp(radius, 0f, t);
                    Vector2 newPos = m_center + new Vector2(
                        Mathf.Cos(angle * Mathf.Deg2Rad),
                        Mathf.Sin(angle * Mathf.Deg2Rad)
                    ) * r;
                    target.position = newPos;
                },
                1f,
                m_downDuration
            ).SetEase(m_ease);

            var shrink = target.DOScale(Vector3.zero, m_downDuration).SetEase(m_ease);

            var forward = DOTween.Sequence()
                .Join(spiralIn)
                .Join(shrink);

            var moveBack = target.DOMove(originalPosition, m_upDuration).SetEase(Ease.OutQuint);
            var scaleBack = target.DOScale(originalScale, m_upDuration).SetEase(Ease.OutQuint);

            var returnSequence = DOTween.Sequence()
                .AppendInterval(index * .2f)
                .Append(moveBack)
                .Join(scaleBack);

            return DOTween.Sequence()
                .Append(forward)
                .Append(returnSequence);
        }
    }
}

