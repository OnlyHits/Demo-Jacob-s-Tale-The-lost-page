using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using CustomArchitecture;
using System.Linq;

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

        private Sequence m_sequence = null;
        private Tween m_vortexRotation = null;
        private Vector2 m_center;

        [SerializeField] private GameObject m_vortexPrefab;
        private GameObject m_vortex;
        private Vector3 m_vortexBaseScale;

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
            m_vortexBaseScale = m_vortex.transform.localScale;
            m_vortex.SetActive(false);
        }
        #endregion

        public void Shuffle(List<Transform> target, PageConfiguration config, Vector2 center)
        {
            if (m_sequence != null && m_sequence.IsPlaying())
            {
                return;
            }

            if (m_vortexRotation != null)
            {
                m_vortexRotation.Kill();
                m_vortexRotation = null;
            }

            m_center = center;

            m_vortex.SetActive(true);
            m_vortex.transform.position = center;
            m_vortex.transform.localScale = Vector3.zero;
            m_vortex.transform.rotation = Quaternion.identity;

            float introDuration = 0.5f;
            float rotationSpeed = 180f;
            float introRotation = rotationSpeed * introDuration;

            m_vortexRotation = m_vortex.transform
                .DORotate(new Vector3(0, 0, 360f), 2f, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart)
                .SetAutoKill(false);

            var vortex_sequence = DOTween.Sequence()
                .Append(m_vortex.transform.DOScale(m_vortexBaseScale, introDuration).SetEase(Ease.OutBack))
                .Join(StartPanelAnimation(target, config))
                .OnComplete(() =>
                {
                    if (m_sequence != null && m_sequence.IsPlaying())
                    {
                        m_sequence.Kill();
                        m_sequence = null;
                    }

                    if (m_vortexRotation != null)
                    {
                        m_vortexRotation.Kill();
                        m_vortexRotation = null;
                    }

                    m_vortex.SetActive(false);
                });

            m_sequence = DOTween.Sequence()
                .Join(vortex_sequence)
                //.Join(m_vortexRotation)
                .Play();
        }

        Sequence StartPanelAnimation(List<Transform> target, PageConfiguration config)
        {
            var loop_sequence = DOTween.Sequence();
            float interval = .2f;

            int i = 0;
            foreach (var panel in target)
            {
                Tween t = CreateSpiralTween(panel, config, i, interval, i == target.Count - 1);
                loop_sequence.Join(t);

                ++i;
            }

            return loop_sequence;
        }

        Tween CreateSpiralTween(Transform target, PageConfiguration config, int index, float interval, bool last_panel)
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

            var moveBack = target.DOMove(config.m_panelPositions[index], m_upDuration).SetEase(Ease.OutQuint);
            var scaleBack = target.DOScale(originalScale, m_upDuration).SetEase(Ease.OutQuint);

            var spit_scale = DOTween.Sequence()
                .Append(m_vortex.transform.DOScale(m_vortexBaseScale * 1.2f, interval / 2).SetEase(Ease.OutSine))
                .Append(m_vortex.transform.DOScale(m_vortexBaseScale, interval / 2).SetEase(Ease.InSine));

            if (last_panel)
            {
                spit_scale.AppendInterval(interval);
                spit_scale.Append(m_vortex.transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InBack));
            }

            var returnSequence = DOTween.Sequence()
                .AppendInterval(index * interval)
                .Append(moveBack)
                .Join(scaleBack)
                .Join(spit_scale);

            return DOTween.Sequence()
                .Append(forward)
                .Append(returnSequence);
        }
    }
}

