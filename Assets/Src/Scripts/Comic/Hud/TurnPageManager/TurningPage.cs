using System.Data.Common;
using CustomArchitecture;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Comic
{
    public class TurningPage : APoolElement
    {
        [SerializeField] private Image      m_pageImage;
        [SerializeField] private Color32    m_blankPageColor;
        private HudTurnPageManager          m_manager;
        private bool                        m_isFirstHalf = false;
        private Sprite                      m_frontSprite;
        private Sprite                      m_backSprite;

        public bool IsFirstHalf() => m_isFirstHalf;

        #region APoolElement override
        public override void OnAllocate(params object[] parameter)
        {
            if (parameter.Length != 6
                || parameter[0] is not Bounds
                || parameter[1] is not Camera
                || parameter[2] is not Canvas
                || parameter[3] is not Sprite
                || parameter[4] is not Sprite
                || parameter[5] is not HudTurnPageManager)
            {
                Debug.LogWarning("Wrong parameters");
                return;
            }

            m_manager = (HudTurnPageManager)parameter[5];
            m_pageImage = gameObject.GetComponent<Image>();
            m_frontSprite = (Sprite)parameter[3];
            m_backSprite = (Sprite)parameter[4];

            ImageUtils.MatchSpriteBounds(m_pageImage, (Canvas)parameter[2], (Camera)parameter[1], (Bounds)parameter[0]);

            m_isFirstHalf = true;
            Compute = true;

        }

        public override void OnDeallocate()
        { }
        #endregion

        #region Turn page error sequence
        private Sequence GetRotationSequenceError(bool is_next, float error_angle, float error_ratio, Ease in_ease, Ease out_ease, float duration)
        {
            SetupPivot(is_next);

            float error_rotate_duration = (duration * .5f) * error_ratio;

            float from_rotation = is_next ? 90f : 270f;
            float to_rotation = is_next ? 270f : 90f;

            RectTransform rect = m_pageImage.GetComponent<RectTransform>();
            rect.eulerAngles = Vector3.zero;

            Sequence rotate_sequence = DOTween.Sequence();

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, from_rotation, 0), duration * 0.5f)
                .SetEase(in_ease)
                .OnComplete(() => {
                    m_isFirstHalf = false;
                    Vector3 rotation = rect.eulerAngles;
                    rotation.y = to_rotation;
                    rect.eulerAngles = rotation;

                    SetupPivot(!is_next);

                    SetupSprite(is_next? m_backSprite : m_frontSprite);
                    m_manager.RefreshRenderingSortOrder();
                }));

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, to_rotation + (error_angle * (is_next ? 1f : -1f)), 0), error_rotate_duration)
                .SetEase(out_ease));
            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, to_rotation, 0), error_rotate_duration)
                .SetEase(in_ease)
                .OnComplete(() => {
                    m_isFirstHalf = true;
                    Vector3 rotation = rect.eulerAngles;
                    rotation.y = from_rotation;
                    rect.eulerAngles = rotation;

                    SetupPivot(is_next);

                    SetupSprite(is_next ? m_frontSprite : m_backSprite);
                    m_manager.RefreshRenderingSortOrder();
                }));

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, 0, 0), duration * 0.5f)
                .SetEase(out_ease));

            rotate_sequence.OnComplete(() => Compute = false);

            return rotate_sequence;
        }

        public Sequence PlayPageErrorSequence(bool is_next, float error_angle, Ease in_ease, Ease out_ease, float duration)
        {
            OnStartAnimation(is_next);

            float max_error_angle = 90f;

            float error_ratio = error_angle / max_error_angle;

            Sequence rotate_sequence = GetRotationSequenceError(is_next, error_angle, error_ratio, in_ease, out_ease, duration);

            return rotate_sequence;
        }

        #endregion

        #region Turn page sequence
        public Sequence PlayRotationSequence(bool is_next, Ease in_ease, Ease out_ease, float duration)
        {
            OnStartAnimation(is_next);

            SetupPivot(is_next);

            float from_rotation = is_next ? 90f : 270f;
            float mid_rotation = is_next ? 270f : 90f;
            float to_rotation = is_next ? 360f : 0f;
            RectTransform rect = m_pageImage.GetComponent<RectTransform>();
            rect.eulerAngles = Vector3.zero;
            Sequence rotate_sequence = DOTween.Sequence();

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, from_rotation, 0), duration * 0.5f)
                .SetEase(in_ease)
                .OnComplete(() =>
                {
                    m_isFirstHalf = false;
                    Vector3 rotation = rect.eulerAngles;
                    rotation.y = mid_rotation;
                    rect.eulerAngles = rotation;

                    SetupPivot(!is_next);

                    SetupSprite(is_next ? m_backSprite : m_frontSprite);
                    m_manager.RefreshRenderingSortOrder();
                }));

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, to_rotation, 0), duration * 0.5f)
                .SetEase(out_ease));

            rotate_sequence.OnComplete(() => Compute = false);

            return rotate_sequence;
        }

        #endregion

        #region Class utility

        private void SetupSprite(Sprite sprite)
        {
            m_pageImage.sprite = sprite;

            // that shity but very convenient
            if (sprite.name == "BookPage_0"
                || sprite.name == "BookPageRight_0")
                m_pageImage.color = m_blankPageColor;
            else
                m_pageImage.color = Color.white;
        }

        private void OnStartAnimation(bool is_next)
        {
            gameObject.SetActive(true);

            SetupSprite(is_next ? m_frontSprite : m_backSprite);

            m_isFirstHalf = true;

            m_manager.RefreshRenderingSortOrder();
        }
        private void SetupPivot(bool right)
        {
            RectTransform rect = m_pageImage.GetComponent<RectTransform>();

            if (right)
                rect.pivot = new Vector2(0f, .5f);
            else
                rect.pivot = new Vector2(1f, .5f);
        }
        #endregion
    }
}