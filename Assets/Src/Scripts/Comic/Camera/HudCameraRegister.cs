using UnityEngine;
using System;
using static PageHole;

namespace Comic
{
    public class HudCameraRegister : ACameraRegister
    {
        [SerializeField] private RectTransform m_screenshotRect;
        [SerializeField] private TurningPage m_turningPage;

        public void SetFrontSprite(Sprite sprite) => m_turningPage.SetFrontSprite(sprite);
        public void SetBackSprite(Sprite sprite) => m_turningPage.SetBackSprite(sprite);
        public void RegisterToEndTurning(Action function) => m_turningPage.RegisterToEndTurning(function);

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
                || parameters[0] is not Camera
                || parameters[1] is not Bounds)
                return;

            Camera world_camera = (Camera)parameters[0];
            Bounds sprite_bounds = (Bounds)parameters[1];

            Vector3 minWorld = sprite_bounds.min;
            Vector3 maxWorld = sprite_bounds.max;

            if (m_cameras.Count > 1)
                m_turningPage.MatchBounds(m_cameras[1],
                    world_camera.WorldToScreenPoint(minWorld),
                    world_camera.WorldToScreenPoint(maxWorld));
        }
        #endregion

        public void TurnPage(bool next_page)
        {
            if (!next_page)
                m_turningPage.PreviousPage();
            else
                m_turningPage.NextPage();
        }

        public void TurnPageError(bool next_page)
        {
            if (!next_page)
                m_turningPage.SwitchPageError(next_page);
            else
                m_turningPage.SwitchPageError(next_page);
        }

        public override Camera GetCameraForScreenshot()
        {
            if (m_cameras.Count > 0 && m_cameras[0] != null)
                return m_cameras[0];

            return null;
        }

        public override Vector3? GetScreenshotMin(Camera base_camera)
        {
            if (m_screenshotRect != null)
            {
                Vector3[] world_corners = new Vector3[4];
                m_screenshotRect.GetWorldCorners(world_corners);

                return base_camera.WorldToScreenPoint(world_corners[0]);
            }
            else
            {
                Debug.LogError("Rect transform for screenshot is null");
                return null;
            }
        }

        public override Vector3? GetScreenshotMax(Camera base_camera)
        {
            if (m_screenshotRect != null)
            {
                // Convert RectTransform to screen space
                Vector3[] world_corners = new Vector3[4];
                m_screenshotRect.GetWorldCorners(world_corners);


                return base_camera.WorldToScreenPoint(world_corners[2]); // Top-right
            }
            else
            {
                Debug.LogError("Rect transform for screenshot is null");
                return null;
            }
        }
    }
}
