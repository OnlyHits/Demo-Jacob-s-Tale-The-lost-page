using DG.Tweening;
using UnityEngine;
using System;
using UnityEngine.UI;
using CustomArchitecture;
using System.Collections;
using System.Collections.Generic;

namespace Comic
{
    public class HudTurnPageManager : BaseBehaviour
    {
        private AllocationPool<TurningPage>     m_pages;
        [SerializeField] private GameObject     m_turningPagePrefab;
        [SerializeField] private Transform      m_poolContainer;
        [SerializeField] private Image          m_leftShadow;
        [SerializeField] private Image          m_rightShadow;
        [SerializeField] private Sprite         m_blankPageLeftSprite;
        [SerializeField] private Sprite         m_blankPageRightSprite;

        [SerializeField] private float          m_turnPageDuration;
        [SerializeField] private float          m_turnCoverDuration;

        [SerializeField] private Canvas         m_canvas;

        [SerializeField, Range(0f, 90f)] private float m_errorAngle = 90f;

        private Sprite                          m_frontSprite;
        private Sprite                          m_backSprite;
        private Sequence                        m_shadowSequence;

        public void SetFrontSprite(Sprite sprite) => m_frontSprite = sprite;
        public void SetBackSprite(Sprite sprite) => m_backSprite = sprite;
        public void RefreshRenderingSortOrder() => m_pages.SortElements(false);

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            m_pages?.Update(Time.deltaTime);
        }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            m_pages = new AllocationPool<TurningPage>(m_turningPagePrefab, m_poolContainer, 10, SortOrderMethod.Sort_Custom, SortPages);
        }
        #endregion

        #region Shadow Sequence
        private Sequence GetShadowSequenceError(bool is_next, float error_ratio, Ease in_ease, Ease out_ease, float duration)
        {
            float error_rotate_duration = (duration * .5f) * error_ratio;

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

            shadow_sequence.Append(starting_shadow.DOColor(is_next ? right_color : left_color, duration * 0.5f).SetEase(in_ease));
            shadow_sequence.Append(ending_shadow.DOColor(is_next ? left_color : right_color, error_rotate_duration).SetEase(out_ease));
            shadow_sequence.Append(ending_shadow.DOColor(is_next ? right_color : left_color, error_rotate_duration).SetEase(in_ease));
            shadow_sequence.Append(starting_shadow.DOColor(is_next ? left_color : right_color, duration * 0.5f).SetEase(out_ease));

            return shadow_sequence;
        }
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
        #endregion

        #region Turn Sequence
        private void TurnAnimation(TurningPage page, bool is_next, bool play_shadow, float duration, Ease in_ease, Ease out_ease)
        {
            page.PlayRotationSequence(is_next, in_ease, out_ease, duration);

            if (play_shadow)
            {
                SetupShadow();
                m_shadowSequence = DOTween.Sequence();
                m_shadowSequence = GetShadowSequence(is_next, in_ease, out_ease, duration);
                m_shadowSequence.OnComplete(() =>
                {
                    m_shadowSequence = null;
                    m_leftShadow.gameObject.SetActive(false);
                    m_rightShadow.gameObject.SetActive(false);
                });
            }
        }

        private void TurnAnimationError(TurningPage page, bool is_next, bool play_shadow, float duration, Ease in_ease, Ease out_ease)
        {
            page.PlayPageErrorSequence(is_next, m_errorAngle, in_ease, out_ease, duration);

            if (play_shadow)
            {
                float max_error_angle = 90f;
                float error_ratio = m_errorAngle / max_error_angle;

                SetupShadow();
                m_shadowSequence = DOTween.Sequence();
                m_shadowSequence = GetShadowSequenceError(is_next, error_ratio, in_ease, out_ease, duration);
                m_shadowSequence.OnComplete(() =>
                {
                    m_shadowSequence = null;
                    m_leftShadow.gameObject.SetActive(false);
                    m_rightShadow.gameObject.SetActive(false);
                });
            }
        }

        public IEnumerator TurnMultiplePagesCoroutine(bool is_next, Bounds sprite_bounds, Camera base_camera)
        {
            float base_ratio = 1f;
            int page_number = 10;

            for (int i = 0; i < page_number; ++i)
            {
                Sprite front_sprite = null;
                Sprite back_sprite = null;

                if (i == 0)
                    front_sprite = m_frontSprite;
                else
                {
                    if (is_next)
                        front_sprite = m_blankPageRightSprite;
                    else
                        front_sprite = m_blankPageLeftSprite;
                }

                if (i == page_number - 1)
                {
                    back_sprite = m_backSprite;
                }
                else
                {
                    if (is_next)
                        back_sprite = m_blankPageLeftSprite;
                    else
                        back_sprite = m_blankPageRightSprite;
                }

                var new_page = m_pages.AllocateElement(sprite_bounds, base_camera, m_canvas, front_sprite, back_sprite, (HudTurnPageManager)this);

                SetPageSessionData(new_page, sprite_bounds, base_camera);

                TurnAnimation(new_page, is_next, false, m_turnPageDuration, Ease.InQuad, Ease.OutQuad);

                yield return new WaitForSeconds(0.1f * base_ratio);

//                base_ratio += .1f;
            }

            yield return new WaitUntil(() => !m_pages.IsCompute());
        }

        public IEnumerator TurnPageCoroutine(bool is_next, Bounds sprite_bounds, Camera base_camera)
        {
            var new_page = m_pages.AllocateElement(sprite_bounds, base_camera, m_canvas, m_frontSprite, m_backSprite, (HudTurnPageManager)this);

            SetPageSessionData(new_page, sprite_bounds, base_camera);

            TurnAnimation(new_page, is_next, true, m_turnPageDuration, Ease.InQuad, Ease.OutQuad);

            yield return new WaitUntil(() => !m_pages.IsCompute());
        }

        public IEnumerator TurnCoverCoroutine(bool is_next, Bounds sprite_bounds, Camera base_camera)
        {
            var new_page = m_pages.AllocateElement(sprite_bounds, base_camera, m_canvas, m_frontSprite, m_backSprite, (HudTurnPageManager)this);

            SetPageSessionData(new_page, sprite_bounds, base_camera);

            TurnAnimation(new_page, is_next, false, m_turnCoverDuration, Ease.InQuad, Ease.OutQuad);

            yield return new WaitUntil(() => !m_pages.IsCompute());
        }

        public IEnumerator TurnPageErrorCoroutine(bool is_next, Bounds sprite_bounds, Camera base_camera)
        {
            var new_page = m_pages.AllocateElement(sprite_bounds, base_camera, m_canvas, m_frontSprite, m_backSprite, (HudTurnPageManager)this);

            SetPageSessionData(new_page, sprite_bounds, base_camera);

            TurnAnimationError(new_page, is_next, true, m_turnPageDuration, Ease.InQuad, Ease.OutQuad);

            yield return new WaitUntil(() => !m_pages.IsCompute());
        }
        #endregion

        private void SetPageSessionData(TurningPage new_page, Bounds sprite_bounds, Camera base_camera)
        {
            ImageUtils.MatchSpriteBounds(m_leftShadow, m_canvas, base_camera, sprite_bounds);
            ImageUtils.MatchSpriteBounds(m_rightShadow, m_canvas, base_camera, sprite_bounds);
        }

        private void SetupShadow()
        {
            m_leftShadow.gameObject.SetActive(true);
            m_rightShadow.gameObject.SetActive(true);

            if (m_shadowSequence != null)
            {
                m_shadowSequence.Kill();
                m_shadowSequence = null;
            }

            var right_color = m_rightShadow.color;
            right_color.a = 0f;
            m_rightShadow.color = right_color;
            var left_color = m_leftShadow.color;
            left_color.a = 0f;
            m_leftShadow.color = left_color;
        }

        private void SortPages(List<TurningPage> pages)
        {
            for (int i = 0, index = pages.Count - 1; i < pages.Count; ++i, --index)
            {
                if (pages[i].IsFirstHalf())
                    pages[i].transform.SetSiblingIndex(index);
                else
                    pages[i].transform.SetSiblingIndex(i);
            }
        }
    }
}