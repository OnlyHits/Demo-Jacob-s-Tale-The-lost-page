using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using CustomArchitecture;
using System.Linq;
using static UnityEditor.PlayerSettings;
using UnityEditor;

namespace Comic
{
    // this class should be instantiated once and pass to the page to get used
    // ie : instantiate in game mode
    public class PanelShuffleSequence : BaseBehaviour
    {
        private readonly float  m_downDuration = 3f;
        private readonly float  m_upDuration = 2f;
        private readonly float  m_rotations = 2f;
        private readonly Ease   m_ease = Ease.OutSine;

        private Sequence m_sequence;
        private Vector2 m_center;

        [SerializeField] private GameObject m_vortexPrefab;
        private GameObject m_vortex;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            if (parameters.Count() < 1 || parameters[0] is not Transform)
            {
                Debug.LogError("Invalid parameters");
                return;
            }

            m_vortex = Instantiate(m_vortexPrefab, (Transform)parameters[0]);
            m_vortex.SetActive(false);
        }
        #endregion

        public void Shuffle(List<Transform> target, Vector2 center)
        {
            m_center = center;
            var vortex_scale = m_vortex.transform.localScale;

            m_vortex.SetActive(true);
            m_vortex.transform.position = center;
            m_vortex.transform.localScale = Vector3.zero;
            m_vortex.transform.rotation = Quaternion.identity;

            float introDuration = 0.5f;
            float rotationSpeed = 180f;
            float introRotation = rotationSpeed * introDuration;

            var vortexIntro = DOTween.Sequence()
                .Append(m_vortex.transform.DOScale(vortex_scale, introDuration).SetEase(Ease.OutBack))
                .Join(m_vortex.transform.DORotate(new Vector3(0, 0, introRotation), introDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear));

            var vortexLoop = m_vortex.transform
                .DORotate(new Vector3(0, 0, 360f), 2f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);

            m_sequence = DOTween.Sequence();

            int i = 0;
            foreach (var panel in target)
            {
                Tween t = CreateSpiralTween(panel, i);
                m_sequence.Join(t);
                ++i;
            }

            // When the shuffle ends, stop the vortex spinning and hide it
            m_sequence.OnComplete(() =>
            {
                vortexLoop.Kill();
                m_vortex.SetActive(false);
            });

            // Combine intro and main sequence
            DOTween.Sequence()
                .Append(vortexIntro)
                .Append(m_sequence)
                .Play();
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

