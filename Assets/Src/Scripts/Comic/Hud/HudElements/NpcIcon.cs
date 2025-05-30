using CustomArchitecture;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using static PageHole;

namespace Comic
{
    public class NpcIcon : BaseBehaviour
    {
        private VoiceType m_type;
        [SerializeField] private RectTransform m_bubbleAnchor;
        [SerializeField] private Image m_iconImage;
        [SerializeField] private Image m_bgImage;
        [SerializeField] private Image m_selector;
        private Tween m_scaleTween;
        private bool m_isHighlight;
        [SerializeField] private Sprite[] m_iconspr;
        [SerializeField] private Image m_background;

        public bool IsHighlight() => m_isHighlight;
        public RectTransform GetBubbleAnchor() => m_bubbleAnchor;
        public void SetBubbleAnchor(RectTransform tr) => m_bubbleAnchor = tr;
        public VoiceType GetVoiceType() => m_type;

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
            if (parameters.Length != 2
                || parameters[0] is not VoiceType
                || parameters[1] is not Sprite)
                return;

            m_type = (VoiceType)parameters[0];
            m_iconImage.sprite = (Sprite)parameters[1];
        }
        #endregion

        public void Highlight(bool highlight)
        {
            m_isHighlight = highlight;
            m_selector.enabled = highlight;
            //if (m_isHighlight)
            //    m_background.sprite = m_iconspr[0];
            //else
            //    m_background.sprite = m_iconspr[1];
        }

        public void SetUnlock(bool unlock)
        {
            Color unlockColor = new Color(Color.white.r, Color.white.g, Color.white.b, 1f);
            Color lockedColor = new Color(Color.black.r, Color.black.g, Color.black.b, 0.64f);

            m_iconImage.enabled = unlock;
            m_bgImage.color = unlock ? unlockColor : lockedColor;
        }

        public void SetIconSprite(Sprite sprite)
        {
            m_iconImage.sprite = sprite;
        }

        public override void Pause(bool pause)
        {
            base.Pause(pause);

            if (pause && m_scaleTween != null)
                m_scaleTween.Pause();
            else if (!pause && m_scaleTween != null)
                m_scaleTween.Play();

        }

        public void Appear(float duration)
        {
            if (IsCompute())
            {
                m_scaleTween.Kill();
                m_scaleTween = null;
            }

            gameObject.SetActive(true);
            transform.GetComponent<RectTransform>().localScale = Vector3.zero;

            m_scaleTween = transform.GetComponent<RectTransform>()
                .DOScale(Vector3.one, duration * .7f)
                .SetEase(Ease.OutBack)
                .OnComplete(() => m_scaleTween = null)
                .OnKill(() => m_scaleTween = null);

            if (m_pause)
                m_scaleTween.Pause();
            else
                m_scaleTween.Play();
        }

        public void Disappear(float duration)
        {
            if (IsCompute())
            {
                m_scaleTween.Kill();
                m_scaleTween = null;
            }

            m_scaleTween = transform.GetComponent<RectTransform>()
                .DOScale(Vector3.zero, duration * 1.3f)
                .SetEase(Ease.InBack)
                .OnComplete(() => { m_scaleTween = null; gameObject.SetActive(false); })
                .OnKill(() => { m_scaleTween = null; gameObject.SetActive(false); });

            if (m_pause)
                m_scaleTween.Pause();
            else
                m_scaleTween.Play();

        }

        public bool IsCompute()
        {
            return m_scaleTween != null && m_scaleTween.IsActive() && m_scaleTween.IsPlaying();
        }
    }
}