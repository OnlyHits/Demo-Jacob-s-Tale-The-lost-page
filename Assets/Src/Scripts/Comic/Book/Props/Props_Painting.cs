using CustomArchitecture;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Rendering.Universal;

namespace Comic
{
    public class Props_Painting : ClampProps
    {
        [SerializeField] private float m_rotationAngle = 20f;
        [SerializeField] private bool m_move = true;
        private Sequence m_rotateSequence;
        private Vector3 m_baseEulerRotation;
        [SerializeField] private Vector2 m_intervalRange = new Vector2(20f, 40f);

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
            base.Init();

            m_baseEulerRotation = transform.localEulerAngles;

            transform.localRotation = Quaternion.Euler(m_baseEulerRotation.x, m_baseEulerRotation.y, 0);
        }
        #endregion

        public override void StartBehaviour()
        {
            if (m_move)
            {
                transform.localRotation = Quaternion.Euler(m_baseEulerRotation.x, m_baseEulerRotation.y, 0);
                SetupRotate();
            }
        }

        private void SetupRotate()
        {
            if (m_rotateSequence != null)
                m_rotateSequence.Kill();

            m_rotateSequence = DOTween.Sequence();

            float randomInterval = UnityEngine.Random.Range(m_intervalRange.x, m_intervalRange.y);
            m_rotateSequence.AppendInterval(randomInterval);

            int flickerCount = UnityEngine.Random.Range(5, 10);

            for (int i = 0; i < flickerCount; i++)
            {
                float randomRotation = UnityEngine.Random.Range(-m_rotationAngle, m_rotationAngle);

                m_rotateSequence.Append(
                    transform.DOLocalRotate(new Vector3(m_baseEulerRotation.x, m_baseEulerRotation.y, randomRotation), UnityEngine.Random.Range(0.3f, 0.7f))
                        .SetEase(Ease.InOutSine)
                );

                m_rotateSequence.AppendInterval(UnityEngine.Random.Range(0.3f, 0.7f));
            }

            m_rotateSequence.Append(
                transform.DOLocalRotate(m_baseEulerRotation, 0.4f)
                    .SetEase(Ease.InOutSine)
            );

            m_rotateSequence.OnComplete(SetupRotate);
        }

        public override void StopBehaviour()
        {
            if (m_rotateSequence != null)
            {
                m_rotateSequence.Kill();
                transform.localRotation = Quaternion.Euler(m_baseEulerRotation.x, m_baseEulerRotation.y, 0);
                m_rotateSequence = null;
            }
        }

        public override void PauseBehaviour(bool pause)
        {
            if (m_rotateSequence != null)
            {
                if (pause)
                    m_rotateSequence.Pause();
                else
                    m_rotateSequence.Play();
            }
        }
    }
}
