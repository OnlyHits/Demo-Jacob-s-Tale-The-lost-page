using CustomArchitecture;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using static PageHole;

namespace Comic
{
    public class TurningPage : BaseBehaviour
    {
        [SerializeField] private Image m_turningPage;
        [SerializeField] private Image m_leftShadow;
        [SerializeField] private Image m_rightShadow;

        [SerializeField] private float m_turnPageDuration;
        [SerializeField] private float m_turnCoverDuration;
        [SerializeField, Range(0f, 90f)] private float m_errorAngle = 45f;
        [SerializeField] private bool m_vibrateOnError = false;
        [SerializeField] private float m_vibrationDuration = .1f;

        private Sprite m_frontSprite;
        private Sprite m_backSprite;

        private Sequence m_turnSequence;

        public bool IsTurning() => m_turnSequence != null;

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
        }
        #endregion

        public void SetFrontSprite(Sprite sprite) => m_frontSprite = sprite;
        public void SetBackSprite(Sprite sprite) => m_backSprite = sprite;


        #region Turn page error

        private Sequence GetShadowSequenceError(bool is_next, float error_ratio)
        {
            float error_rotate_duration = (m_turnPageDuration * .5f) * error_ratio;

            Sequence shadow_sequence = DOTween.Sequence();

            var right_color = m_rightShadow.color;
            right_color.a = is_next ? 1f : 0f;
            m_rightShadow.color = right_color;
            right_color.a = is_next ? 0f : error_ratio;

            var left_color = m_leftShadow.color;
            left_color.a = is_next ? 0f : 1f;
            m_leftShadow.color = left_color;
            left_color.a = is_next ? error_ratio : 0f;

            Image starting_shadow = is_next ? m_rightShadow : m_leftShadow;
            Image ending_shadow = is_next ? m_leftShadow : m_rightShadow;

            shadow_sequence.Append(starting_shadow.DOColor(is_next ? right_color : left_color, m_turnPageDuration * 0.5f).SetEase(Ease.InQuad));
            shadow_sequence.Append(ending_shadow.DOColor(is_next ? left_color : right_color, error_rotate_duration).SetEase(Ease.OutQuad));

            if (m_vibrateOnError)
                shadow_sequence.AppendInterval(m_vibrationDuration);

            shadow_sequence.Append(ending_shadow.DOColor(is_next ? right_color : left_color, error_rotate_duration).SetEase(Ease.InQuad));
            shadow_sequence.Append(starting_shadow.DOColor(is_next ? left_color : right_color, m_turnPageDuration * 0.5f).SetEase(Ease.OutQuad));

            return shadow_sequence;
        }

        private Sequence GetRotationSequenceError(bool is_next, float error_ratio)
        {
            SetupPivot(is_next);

            float error_rotate_duration = (m_turnPageDuration * .5f) * error_ratio;

            float from_rotation = is_next ? 90f : 270f;
            float to_rotation = is_next ? 270f : 90f;

            RectTransform rect = m_turningPage.GetComponent<RectTransform>();
            rect.eulerAngles = Vector3.zero;

            Sequence rotate_sequence = DOTween.Sequence();

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, from_rotation, 0), m_turnPageDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    Vector3 rotation = rect.eulerAngles;
                    rotation.y = to_rotation;
                    rect.eulerAngles = rotation;

                    SetupPivot(!is_next);

                    m_turningPage.sprite = is_next ? m_backSprite : m_frontSprite;
                }));

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, to_rotation + (m_errorAngle * (is_next ? 1f : -1f)), 0), error_rotate_duration)
                .SetEase(Ease.OutQuad));

            if (m_vibrateOnError)
                rotate_sequence.Append(rect.DOShakeRotation(m_vibrationDuration, new Vector3(0, 4, 0), 20, 0, false));

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, to_rotation, 0), error_rotate_duration)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    Vector3 rotation = rect.eulerAngles;
                    rotation.y = from_rotation;
                    rect.eulerAngles = rotation;

                    SetupPivot(is_next);

                    m_turningPage.sprite = is_next ? m_frontSprite : m_backSprite;
                }));

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, 0, 0), m_turnPageDuration * 0.5f)
                .SetEase(Ease.OutQuad));

            return rotate_sequence;
        }

        public void TurnPageError(bool is_next)
        {
            OnStartAnimation(is_next);

            float max_error_angle = 90f;

            m_errorAngle = Mathf.Clamp(m_errorAngle, 0f, max_error_angle);

            float error_ratio = m_errorAngle / max_error_angle;

            m_turnSequence = DOTween.Sequence();

            Sequence rotate_sequence = GetRotationSequenceError(is_next, error_ratio);
            Sequence shadow_sequence = GetShadowSequenceError(is_next, error_ratio);

            m_turnSequence.Join(rotate_sequence);
            m_turnSequence.Join(shadow_sequence);

            m_turnSequence.OnComplete(() => {
                m_turnSequence = null;
                gameObject.SetActive(false);
            });
        }

        #endregion

        #region Turn page

        private Sequence GetShadowSequence(bool is_next, Ease in_ease, Ease out_ease, float duration)
        {
            Sequence shadow_sequence = DOTween.Sequence();

            var right_color = m_rightShadow.color;
            right_color.a = is_next ? 1f : 0f;
            m_rightShadow.color = right_color;
            right_color.a = is_next ? 0f : 1f;

            var left_color = m_leftShadow.color;
            left_color.a = is_next ? 0f : 1f;
            m_leftShadow.color = left_color;
            left_color.a = is_next ? 1f : 0f;

            Image starting_shadow = is_next ? m_rightShadow : m_leftShadow;
            Image ending_shadow = is_next ? m_leftShadow : m_rightShadow;

            shadow_sequence.Append(starting_shadow.DOColor(is_next ? right_color : left_color, duration * 0.5f).SetEase(in_ease));
            shadow_sequence.Append(ending_shadow.DOColor(is_next ? left_color : right_color, duration * 0.5f).SetEase(out_ease));

            return shadow_sequence;
        }

        private Sequence GetRotationSequence(bool is_next, Ease in_ease, Ease out_ease, float duration)
        {
            SetupPivot(is_next);

            float from_rotation = is_next ? 90f : 270f;
            float mid_rotation = is_next ? 270f : 90f;
            float to_rotation = is_next ? 360f : 0f;
            RectTransform rect = m_turningPage.GetComponent<RectTransform>();
            rect.eulerAngles = Vector3.zero;

            Sequence rotate_sequence = DOTween.Sequence();

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, from_rotation, 0), duration * 0.5f)
                .SetEase(in_ease)
                .OnComplete(() =>
                {
                    Vector3 rotation = rect.eulerAngles;
                    rotation.y = mid_rotation;
                    rect.eulerAngles = rotation;

                    SetupPivot(!is_next);

                    m_turningPage.sprite = is_next ? m_backSprite : m_frontSprite;
                }));

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, to_rotation, 0), duration * 0.5f)
                .SetEase(out_ease));

            return rotate_sequence;
        }

        private void TurnAnimation(bool is_next, bool play_shadow, float duration, Ease in_ease, Ease out_ease)
        {
            OnStartAnimation(is_next);

            m_turnSequence = DOTween.Sequence();

            Sequence rotate_sequence = GetRotationSequence(is_next, in_ease, out_ease, duration);
            m_turnSequence.Join(rotate_sequence);

            if (play_shadow)
            {
                Sequence shadow_sequence = GetShadowSequence(is_next, in_ease, out_ease, duration);
                m_turnSequence.Join(shadow_sequence);
            }

            m_turnSequence.OnComplete(() => {
                m_turnSequence = null;
                gameObject.SetActive(false);
            });
        }

        #endregion

        public void TurnPage(bool is_next)
        {
            TurnAnimation(is_next, true, m_turnPageDuration, Ease.InQuad, Ease.OutQuad);
        }

        public void TurnCover(bool is_next)
        {
            TurnAnimation(is_next, false, m_turnCoverDuration , Ease.InCirc, Ease.OutQuad);
        }

        private void OnStartAnimation(bool is_next)
        {
            gameObject.SetActive(true);

            if (m_turnSequence != null)
            {
                m_turnSequence.Kill();
                m_turnSequence = null;
            }

            m_turningPage.sprite = is_next ? m_frontSprite : m_backSprite;
            var right_color = m_rightShadow.color;
            right_color.a = 0f;
            m_rightShadow.color = right_color;
            var left_color = m_leftShadow.color;
            left_color.a = 0f;
            m_leftShadow.color = left_color;
        }

        private void SetupPivot(bool right)
        {
            RectTransform rect = m_turningPage.GetComponent<RectTransform>();

            if (right)
                rect.pivot = new Vector2(0f, .5f);
            else
                rect.pivot = new Vector2(1f, .5f);
        }

        #region Utils

        public Canvas m_canvas;

        private void MatchImage(Image image, Vector2 min, Vector2 max)
        {
            RectTransform rect = image.GetComponent<RectTransform>();

            rect.sizeDelta = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        }

        public void MatchBounds(Vector3 min_screen, Vector3 max_screen)
        {
            RectTransform canvasRect = m_canvas.GetComponent<RectTransform>();

            Vector2 min = min_screen;
            Vector2 max = max_screen;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, min_screen, m_canvas.worldCamera, out min);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, max_screen, m_canvas.worldCamera, out max);

            MatchImage(m_turningPage, min, max);
            MatchImage(m_rightShadow, min, max);
            MatchImage(m_leftShadow, min, max);
        }

        #endregion
    }
}