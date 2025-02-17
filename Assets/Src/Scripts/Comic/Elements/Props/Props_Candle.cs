using CustomArchitecture;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.Rendering.Universal;
using System.Linq;

namespace Comic
{
    public class Props_Candle : AProps
    {
        public enum BehaviourType
        {
            Lit,
            Unlit,
            LitRandom,
        }

        [SerializeField] private Sprite[] m_sprites;
        [SerializeField] private Light2D m_light;
        [SerializeField] private BehaviourType m_behaviourType;
        private Sequence m_litSequence;
        private float m_baseIntensity;
        private SpriteRenderer m_spriteRenderer;

        private void Awake()
        {
        }


        public override void Init()
        {
            m_spriteRenderer = GetComponent<SpriteRenderer>();
            m_baseIntensity = m_light.intensity;

            if (m_behaviourType == BehaviourType.Unlit)
            {
                m_spriteRenderer.sprite = m_sprites[0];
                m_light.intensity = 0f;
            }
            else if (m_behaviourType == BehaviourType.Lit)
            {
                m_spriteRenderer.sprite = m_sprites[1];
            }
            else if (m_behaviourType == BehaviourType.LitRandom)
            {
                m_spriteRenderer.sprite = m_sprites[0];
                m_light.intensity = 0f;
            }
        }

        public override void StartBehaviour()
        {
            if (m_behaviourType == BehaviourType.Unlit)
            {
                m_spriteRenderer.sprite = m_sprites[0];
                m_light.intensity = 0f;
            }
            else if (m_behaviourType == BehaviourType.Lit)
            {
                m_spriteRenderer.sprite = m_sprites[1];
                PlayLitSequence();
            }
            else if (m_behaviourType == BehaviourType.LitRandom)
                PlayLitRandomSequence();
        }

        private void PlayLitSequence()
        {
            if (m_litSequence != null)
                m_litSequence.Kill();

            m_litSequence = DOTween.Sequence();

            m_litSequence.Append(DOTween.To(
                () => m_light.intensity,
                x => m_light.intensity = x,
                UnityEngine.Random.Range(m_baseIntensity * .8f, m_baseIntensity),
                UnityEngine.Random.Range(0.05f, 0.15f)
            ));

            m_litSequence.AppendInterval(UnityEngine.Random.Range(0.05f, 0.1f));

            m_litSequence.OnStepComplete(() =>
            {
                PlayLitSequence();
            });
        }

        private void PlayLitRandomSequence()
        {
            if (m_litSequence != null)
                m_litSequence.Kill();

            int flickerCount = UnityEngine.Random.Range(20, 40);

            m_litSequence = DOTween.Sequence();

            float randomInterval = UnityEngine.Random.Range(10f, 20f);
            m_litSequence.AppendInterval(randomInterval);

            if (m_sprites != null && m_sprites.Count() > 0)
                m_spriteRenderer.sprite = m_sprites[0];

            m_light.intensity = 0f;
            m_litSequence.AppendCallback(() => { m_spriteRenderer.sprite = m_sprites[1]; });

            for (int i = 0; i < flickerCount; i++)
            {
                m_litSequence.Append(DOTween.To(() => m_light.intensity, x => m_light.intensity = x,
                    UnityEngine.Random.Range(m_baseIntensity * .8f, m_baseIntensity), UnityEngine.Random.Range(0.05f, 0.15f)));

                m_litSequence.AppendInterval(UnityEngine.Random.Range(0.05f, 0.1f));
            }

            m_litSequence.Append(DOTween.To(() => m_light.intensity, x => m_light.intensity = x, 0f, 0.1f));
            m_litSequence.AppendCallback(() => { m_spriteRenderer.sprite = m_sprites[0]; });

            m_litSequence.OnComplete(PlayLitRandomSequence);
        }

        public void StartFlickering()
        {
            if (m_litSequence != null)
                m_litSequence.Kill();

            int flickerCount = UnityEngine.Random.Range(20, 40);

            m_litSequence = DOTween.Sequence();

            m_light.intensity = m_baseIntensity;

            for (int i = 0; i < flickerCount; i++)
            {
                m_litSequence.Append(DOTween.To(() => m_light.intensity, x => m_light.intensity = x,
                    UnityEngine.Random.Range(0f, m_baseIntensity), UnityEngine.Random.Range(0.05f, 0.15f)));

                m_litSequence.AppendInterval(UnityEngine.Random.Range(0.05f, 0.1f));
            }

            m_litSequence.Append(DOTween.To(() => m_light.intensity, x => m_light.intensity = x, m_baseIntensity, 0.1f));

            m_litSequence.SetLoops(-1, LoopType.Restart);

            m_litSequence.OnKill(() => m_light.intensity = m_baseIntensity);
        }

        public override void StopBehaviour()
        {
            if (m_litSequence != null)
            {
                m_litSequence.Kill();
                m_litSequence = null;

                if (m_behaviourType == BehaviourType.LitRandom)
                {
                    m_spriteRenderer.sprite = m_sprites[0];
                    m_light.intensity = 0f;
                }
            }
        }

        public override void PauseBehaviour(bool pause)
        {
            if (m_litSequence != null)
            {
                if (pause)
                    m_litSequence.Pause();
                else
                    m_litSequence.Play();
            }
        }
    }
}
