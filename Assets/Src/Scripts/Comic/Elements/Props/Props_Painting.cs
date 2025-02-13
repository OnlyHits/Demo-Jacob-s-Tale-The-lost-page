using CustomArchitecture;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Rendering.Universal;

namespace Comic
{
    public class Props_Painting : AProps
    {
        [SerializeField] private float m_rotationAngle = 20f;
        [SerializeField] private bool m_move = true;
        private Sequence m_rotateSequence;
        private Quaternion m_baseRotation;

        private void Awake()
        {
            m_baseRotation = transform.rotation;
        }

        public override void StartBehaviour()
        {
            transform.rotation = Quaternion.Euler(0, 0, m_rotationAngle);

            if (m_move)
            {
                SetupRotate();
            }
        }

        private void SetupRotate()
        {
            if (m_rotateSequence != null)
                m_rotateSequence.Kill();

            m_rotateSequence = DOTween.Sequence();

            int flickerCount = UnityEngine.Random.Range(5, 10);

            for (int i = 0; i < flickerCount; i++)
            {
                float randomRotation = UnityEngine.Random.Range(-m_rotationAngle, m_rotationAngle);

                m_rotateSequence.Append(
                    transform.DORotate(new Vector3(0, 0, randomRotation), UnityEngine.Random.Range(0.3f, 0.7f))
                        .SetEase(Ease.InOutSine)
                );

                m_rotateSequence.AppendInterval(UnityEngine.Random.Range(0.3f, 0.7f));
            }

            m_rotateSequence.Append(
                transform.DORotate(m_baseRotation.eulerAngles, 0.4f)
                    .SetEase(Ease.InOutSine)
            );

            float randomInterval = UnityEngine.Random.Range(20f, 40f);
            m_rotateSequence.AppendInterval(randomInterval);

            m_rotateSequence.OnComplete(SetupRotate);
        }


        public override void StopBehaviour()
        {
            if (m_rotateSequence != null)
            {
                m_rotateSequence.Kill();
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
