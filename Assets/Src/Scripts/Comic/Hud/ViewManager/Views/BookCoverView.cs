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

            Debug.Log($"Screen Min: {min_screen}, Max: {max_screen}");
            Debug.Log($"Local UI Min: {min}, Max: {max}");

            MatchImage(m_coverImage, min, max);
        }

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
