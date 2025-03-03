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
        private Action m_onEndTurning;

        public void RegisterToEndTurning(Action function)
        {
            m_onEndTurning -= function;
            m_onEndTurning += function;
        }

        public void TurnPageError(bool is_next)
        {
            gameObject.SetActive(true);

            if (m_turnSequence != null)
            {
                m_turnSequence.Kill();
                m_turnSequence = null;
            }

            float max_error_angle = 90f;

            m_errorAngle = Mathf.Clamp(m_errorAngle, 0f, max_error_angle);

            float error_ratio = m_errorAngle / max_error_angle;
            float error_rotate_duration = (m_turnPageDuration * .5f) * error_ratio;

            float from_rotation = is_next ? 90f : 270f;
            float to_rotation = is_next ? 270f : 90f;

            RectTransform rect = m_turningPage.GetComponent<RectTransform>();
            rect.eulerAngles = Vector3.zero;

            var right_color = m_rightShadow.color;
            right_color.a = is_next ? 1f : 0f;
            m_rightShadow.color = right_color;
            right_color.a = is_next ? 0f : error_ratio;

            var left_color = m_leftShadow.color;
            left_color.a = is_next ? 0f : 1f;
            m_leftShadow.color = left_color;
            left_color.a = is_next ? error_ratio : 0f;

            SetupRect(is_next);
            m_turningPage.sprite = is_next ? m_frontSprite : m_backSprite;

            m_turnSequence = DOTween.Sequence();

            Sequence rotate_sequence = DOTween.Sequence();
            Sequence shadow_sequence = DOTween.Sequence();

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, from_rotation, 0), m_turnPageDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    Vector3 rotation = rect.eulerAngles;
                    rotation.y = to_rotation;
                    rect.eulerAngles = rotation;

                    SetupRect(!is_next);

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

                    SetupRect(is_next);

                    m_turningPage.sprite = is_next ? m_frontSprite : m_backSprite;
                }));

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, 0, 0), m_turnPageDuration * 0.5f)
                .SetEase(Ease.OutQuad));

            Image starting_shadow = is_next ? m_rightShadow : m_leftShadow;
            Image ending_shadow = is_next ? m_leftShadow : m_rightShadow;

            shadow_sequence.Append(starting_shadow.DOColor(is_next ? right_color : left_color, m_turnPageDuration * 0.5f).SetEase(Ease.InQuad));
            shadow_sequence.Append(ending_shadow.DOColor(is_next ? left_color : right_color, error_rotate_duration).SetEase(Ease.OutQuad));
            
            if (m_vibrateOnError)
                shadow_sequence.AppendInterval(m_vibrationDuration);
    
            shadow_sequence.Append(ending_shadow.DOColor(is_next ? right_color : left_color, error_rotate_duration).SetEase(Ease.InQuad));
            shadow_sequence.Append(starting_shadow.DOColor(is_next ? left_color : right_color, m_turnPageDuration * 0.5f).SetEase(Ease.OutQuad));

            m_turnSequence.Join(rotate_sequence);
            m_turnSequence.Join(shadow_sequence);

            m_turnSequence.OnComplete(() => {
                m_onEndTurning?.Invoke();
                m_turnSequence = null;
                gameObject.SetActive(false);
            });

        }

        public void PreviousPage()
        {
            gameObject.SetActive(true);

            if (m_turnSequence != null)
            {
                m_turnSequence.Kill();
                m_turnSequence = null;
            }

            RectTransform rect = m_turningPage.GetComponent<RectTransform>();

            rect.eulerAngles = new Vector3(0, 0, 0);

            var right_color = m_rightShadow.color;
            right_color.a = 0f;
            m_rightShadow.color = right_color;
            right_color.a = 1f;

            var left_color = m_leftShadow.color;
            left_color.a = 1f;
            m_leftShadow.color = left_color;
            left_color.a = 0f;

            SetupRect(false);
            m_turningPage.sprite = m_backSprite;

            m_turnSequence = DOTween.Sequence();

            Sequence rotate_sequence = DOTween.Sequence();
            Sequence shadow_sequence = DOTween.Sequence();

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, 270, 0), m_turnPageDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .OnComplete(() =>
                {
                    Vector3 rotation = rect.eulerAngles;
                    rotation.y = 90f;
                    rect.eulerAngles = rotation;

                    SetupRect(true);

                    m_turningPage.sprite = m_frontSprite;
                }));
            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, 0, 0), m_turnPageDuration * 0.5f)
                .SetEase(Ease.OutQuad));

            shadow_sequence.Append(m_leftShadow.DOColor(left_color, m_turnPageDuration * .5f).SetEase(Ease.InQuad));
            shadow_sequence.Append(m_rightShadow.DOColor(right_color, m_turnPageDuration * .5f).SetEase(Ease.OutQuad));

            m_turnSequence.Join(rotate_sequence);
            m_turnSequence.Join(shadow_sequence);
            m_turnSequence.OnComplete(() =>
            {
                m_onEndTurning?.Invoke();
                m_turnSequence = null;
                gameObject.SetActive(false);
            });
        }

        public void NextPage()
        {
            gameObject.SetActive(true);

            if (m_turnSequence != null)
            {
                m_turnSequence.Kill();
                m_turnSequence = null;
            }

            RectTransform rect = m_turningPage.GetComponent<RectTransform>();

            rect.eulerAngles = Vector3.zero;

            var right_color = m_rightShadow.color;
            right_color.a = 1f;
            m_rightShadow.color = right_color;
            right_color.a = 0f;

            var left_color = m_leftShadow.color;
            left_color.a = 0f;
            m_leftShadow.color = left_color;
            left_color.a = 1f;

            SetupRect(true);
            m_turningPage.sprite = m_frontSprite;

            m_turnSequence = DOTween.Sequence();

            Sequence rotate_sequence = DOTween.Sequence();
            Sequence shadow_sequence = DOTween.Sequence();

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, 90, 0), m_turnPageDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    Vector3 rotation = rect.eulerAngles;
                    rotation.y = 270f;
                    rect.eulerAngles = rotation;

                    SetupRect(false);

                    m_turningPage.sprite = m_backSprite;
                }));
            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, 360, 0), m_turnPageDuration * 0.5f)
                .SetEase(Ease.OutQuad));

            shadow_sequence.Append(m_rightShadow.DOColor(right_color, m_turnPageDuration * .5f).SetEase(Ease.InQuad));
            shadow_sequence.Append(m_leftShadow.DOColor(left_color, m_turnPageDuration * .5f).SetEase(Ease.OutQuad));

            m_turnSequence.Join(rotate_sequence);
            m_turnSequence.Join(shadow_sequence);
            m_turnSequence.OnComplete(() => {
                m_onEndTurning?.Invoke();
                m_turnSequence = null;
                gameObject.SetActive(false);
            });
        }

        public void TurnCover()
        {
            gameObject.SetActive(true);

            if (m_turnSequence != null)
            {
                m_turnSequence.Kill();
                m_turnSequence = null;
            }

            var right_color = m_rightShadow.color;
            right_color.a = 0f;
            m_rightShadow.color = right_color;

            var left_color = m_leftShadow.color;
            left_color.a = 0f;
            m_leftShadow.color = left_color;

            RectTransform rect = m_turningPage.GetComponent<RectTransform>();

            rect.eulerAngles = Vector3.zero;

            SetupRect(true);
            m_turningPage.sprite = m_frontSprite;

            m_turnSequence = DOTween.Sequence();

            Sequence rotate_sequence = DOTween.Sequence();

            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, 90, 0), m_turnCoverDuration * 0.5f)
                .SetEase(Ease.InQuad)
                .OnComplete(() => {
                    Vector3 rotation = rect.eulerAngles;
                    rotation.y = 270f;
                    rect.eulerAngles = rotation;

                    SetupRect(false);

                    m_turningPage.sprite = m_backSprite;
                }));
            rotate_sequence.Append(rect.DORotateQuaternion(Quaternion.Euler(0, 360, 0), m_turnCoverDuration * 0.5f)
                .SetEase(Ease.OutQuad));

            m_turnSequence.Join(rotate_sequence);

            m_turnSequence.OnComplete(() => {
                m_onEndTurning?.Invoke();
                m_turnSequence = null;
                gameObject.SetActive(false);
            });
        }

        private void SetupRect(bool right)
        {
            RectTransform rect = m_turningPage.GetComponent<RectTransform>();

            if (right)
            {
                rect.pivot = new Vector2(0f, .5f);
                // rect.anchorMin = new Vector2(.5f, 0f);
                // rect.anchorMax = new Vector2(1f, 1f);
            }
            else
            {
                rect.pivot = new Vector2(1f, .5f);
                // rect.anchorMin = new Vector2(0f, 0f);
                // rect.anchorMax = new Vector2(.5f, 1f);
            }
        }

        #region Utils

        public Canvas m_canvas;

        private void MatchImage(Image image, Vector2 min, Vector2 max)
        {
            RectTransform rect = image.GetComponent<RectTransform>();

            rect.sizeDelta = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        }

        public void MatchBounds(Camera rendering_camera, Vector3 min_screen, Vector3 max_screen)
        {
            if (rendering_camera == null)
                return;

            //Vector3 min_screen = sprite_bounds.min;
            //Vector3 max_screen = sprite_bounds.max;

            RectTransform canvasRect = m_canvas.GetComponent<RectTransform>();

            Vector2 min = min_screen;
            Vector2 max = max_screen;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, min_screen, rendering_camera, out min);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, max_screen, rendering_camera, out max);

            MatchImage(m_turningPage, min, max);
            MatchImage(m_rightShadow, min, max);
            MatchImage(m_leftShadow, min, max);
        }

        #endregion
    }
}