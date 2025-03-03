using CustomArchitecture;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using static PageHole;
using Unity.VisualScripting;

namespace Comic
{
    public class BookCoverView : AView
    {
        [SerializeField] private Image  m_coverImage;
        [SerializeField] private Canvas m_canvas;

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
        { }
        #endregion

        public void MatchBounds(SpriteRenderer spriteRenderer)
        {
            Bounds spriteBounds = spriteRenderer.bounds;
            Vector3 worldSize = spriteBounds.size;

            Vector2 screenSize = WorldToUISize(worldSize, spriteRenderer);

            m_coverImage.GetComponent<RectTransform>().sizeDelta = screenSize;
        }

        Vector2 WorldToUISize(Vector3 worldSize, SpriteRenderer spriteRenderer)
        {
            Camera cam = m_canvas.worldCamera != null ? m_canvas.worldCamera : Camera.main;

            if (cam == null)
            {
                Debug.LogError("No camera found for Canvas!");
                return Vector2.zero;
            }

            // Convert world positions to screen points
            Vector2 screenPointMin = cam.WorldToScreenPoint(spriteRenderer.bounds.min);
            Vector2 screenPointMax = cam.WorldToScreenPoint(spriteRenderer.bounds.max);

            // Get the size in screen space
            Vector2 screenSize = screenPointMax - screenPointMin;

            // Convert screen size to UI size (based on Canvas scaler)
            return screenSize / m_canvas.scaleFactor;
        }

        //public void MatchBounds(Vector3 min_screen, Vector3 max_screen)
        //{
        //    Camera rendering_camera = m_canvas.worldCamera;

        //    if (rendering_camera == null)
        //        return;

        //    RectTransform canvasRect = m_canvas.GetComponent<RectTransform>();

        //    Vector2 min = min_screen;
        //    Vector2 max = max_screen;

        //    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, min_screen, rendering_camera, out min);
        //    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, max_screen, rendering_camera, out max);

        //    RectTransform rect = m_coverImage.GetComponent<RectTransform>();

        //    rect.sizeDelta = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        //}

        public override void ActiveGraphic(bool active)
        {
            Image[] images = gameObject.GetComponentsInChildren<Image>(true);

            foreach (Image image in images)
            {
                image.enabled = active;
            }
        }

        public override void Pause(bool pause)
        {
            base.Pause();
        }
    }
}
