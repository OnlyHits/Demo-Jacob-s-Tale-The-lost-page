using CustomArchitecture;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Rendering.Universal;

namespace Comic
{
    public class Props_Lamp : AProps
    {
        [SerializeField] private Light2D m_light;
        [SerializeField] private float m_rotationAngle = 40f;
        [SerializeField] private float m_duration = 2f;
        [SerializeField] private bool m_move = true;
        [SerializeField] private bool m_flash = true;
        private Sequence m_moveSequence;
        private Sequence m_flashSequence;
        private float m_baseIntensity;

        private void Awake()
        {
            m_baseIntensity = m_light.intensity;
        }

        public override void StartBehaviour()
        {
            transform.rotation = Quaternion.Euler(0, 0, m_rotationAngle);


            if (m_move)
            {
                if (m_moveSequence != null)
                    m_moveSequence.Kill();
                
                m_moveSequence = DOTween.Sequence();
                m_moveSequence.Append(transform.DORotate(new Vector3(0, 0, 0), m_duration * .5f)
                    .SetEase(Ease.InQuad))
                    .Append(transform.DORotate(new Vector3(0, 0, -m_rotationAngle), m_duration * .5f)
                    .SetEase(Ease.OutQuad))
                    .Append(transform.DORotate(new Vector3(0, 0, 0), m_duration * .5f)
                    .SetEase(Ease.InQuad))
                    .Append(transform.DORotate(new Vector3(0, 0, m_rotationAngle), m_duration * .5f)
                    .SetEase(Ease.OutQuad));

                m_moveSequence.SetLoops(-1, LoopType.Restart);
            }

            if (m_flash)
            {
                SetupFlashSequence();
            }
        }

        private void SetupFlashSequence()
        {
            if (m_flashSequence != null)
                m_flashSequence.Kill();

            int flickerCount = UnityEngine.Random.Range(20, 40);

            m_flashSequence = DOTween.Sequence();

            m_light.intensity = m_baseIntensity;

            for (int i = 0; i < flickerCount; i++)
            {
                m_flashSequence.Append(DOTween.To(() => m_light.intensity, x => m_light.intensity = x, 
                    UnityEngine.Random.Range(0f, m_baseIntensity), UnityEngine.Random.Range(0.05f, 0.15f)));

                m_flashSequence.AppendInterval(UnityEngine.Random.Range(0.05f, 0.1f));
            }

            m_flashSequence.Append(DOTween.To(() => m_light.intensity, x => m_light.intensity = x, m_baseIntensity, 0.1f));

            float randomInterval = UnityEngine.Random.Range(10f, 20f);
            m_flashSequence.AppendInterval(randomInterval);

            m_flashSequence.OnComplete(SetupFlashSequence);
        }

        public void StartFlickering()
        {
            if (m_flashSequence != null)
                m_flashSequence.Kill();

            int flickerCount = UnityEngine.Random.Range(20, 40);

            m_flashSequence = DOTween.Sequence();

            m_light.intensity = m_baseIntensity;

            for (int i = 0; i < flickerCount; i++)
            {
                m_flashSequence.Append(DOTween.To(() => m_light.intensity, x => m_light.intensity = x, 
                    UnityEngine.Random.Range(0f, m_baseIntensity), UnityEngine.Random.Range(0.05f, 0.15f)));

                m_flashSequence.AppendInterval(UnityEngine.Random.Range(0.05f, 0.1f));
            }

            m_flashSequence.Append(DOTween.To(() => m_light.intensity, x => m_light.intensity = x, m_baseIntensity, 0.1f));

            m_flashSequence.SetLoops(-1, LoopType.Restart);

            m_flashSequence.OnKill(() => m_light.intensity = m_baseIntensity);
        }


        public override void StopBehaviour()
        {
            if (m_moveSequence != null)
            {
                m_moveSequence.Kill();
                m_moveSequence = null;
            }

            if (m_flashSequence != null)
            {
                m_flashSequence.Kill();
                m_flashSequence = null;
            }
        }

        public override void PauseBehaviour(bool pause)
        {
            if (m_moveSequence != null)
            {
                if (pause)
                    m_moveSequence.Pause();
                else
                    m_moveSequence.Play();
            }

            if (m_flashSequence != null)
            {
                if (pause)
                    m_flashSequence.Pause();
                else
                    m_flashSequence.Play();
            }
        }
    }
}
