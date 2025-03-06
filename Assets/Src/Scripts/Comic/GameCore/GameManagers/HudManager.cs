using CustomArchitecture;
using System;
using UnityEngine;
using System.Collections;
using UnityEditor.PackageManager;

namespace Comic
{
    public class HudManager : BaseBehaviour
    {
        [SerializeField] private TurningPage    m_turningPage;
        private HudCameraRegister               m_cameras;
        private ViewManager                     m_viewManager;
        private URP_CameraManager               m_cameraManager;
        private bool                            m_requestScreenshotDebug;
        private bool                            m_turnPageErrorDebug;

        public ViewManager GetViewManager() => m_viewManager;
        public HudCameraRegister GetRegisteredCameras() => m_cameras;
        public TurningPage GetTurningPage() => m_turningPage;

        public void SetFrontSprite(Sprite sprite) => m_turningPage.SetFrontSprite(sprite);
        public void SetBackSprite(Sprite sprite) => m_turningPage.SetBackSprite(sprite);
        public void RegisterToEndTurning(Action function) => m_turningPage.RegisterToEndTurning(function);
        public void TurnCover() => m_turningPage.TurnCover();

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }

        private void SetupBookCoverView(Bounds sprite_bounds)
        {
            Vector3 minWorld = sprite_bounds.min;
            Vector3 maxWorld = sprite_bounds.max;

            m_viewManager.GetView<BookCoverView>().MatchBounds(
                m_cameraManager.GetCameraBase().WorldToScreenPoint(minWorld),
                m_cameraManager.GetCameraBase().WorldToScreenPoint(maxWorld));
        }

        public override void LateInit(params object[] parameters)
        {
            if (parameters.Length < 1
                || parameters[0] is not SpriteRenderer)
            {
                Debug.LogWarning("Bad parameters");
                return;
            }

            m_viewManager.LateInit();
            m_cameras.LateInit();
            m_turningPage.LateInit();

            SetupBookCoverView(((SpriteRenderer)parameters[0]).bounds);
        }

        public override void Init(params object[] parameters)
        {
            if (parameters.Length != 1
                || parameters[0] is not URP_CameraManager)
            {
                Debug.LogWarning("Unable to get URP_CameraManager");
                return;
            }

            m_viewManager = gameObject.GetComponent<ViewManager>();
            m_cameras = gameObject.GetComponent<HudCameraRegister>();

            m_cameras.Init();
            m_viewManager.Init();

            m_cameraManager = (URP_CameraManager)parameters[0];

            //MatchBounds(m_cameraManager.GetScreenshotBounds());

            m_cameraManager.SubscribeToScreenshot(OnScreenshot);
        }
        #endregion

        public void SetupPageForScreenshot(bool pause)
        {
            if (!pause)
                GetViewManager().Show<ProgressionView>(true);
        }

        public void RestorePageAfterScreenshot(bool pause)
        {
            if (!pause)
                GetViewManager().ShowLast();
            else
                GetViewManager().Show<PauseView>();
        }

        public void OnPageChangeEnd(bool pause)
        {
            if (!pause)
                GetViewManager().Show<ProgressionView>();
        }

        public bool CanResume()
        {
            if (GetViewManager().GetCurrentView() is PauseView pauseView)
            {
                if (pauseView.IsBasePanelShown)
                {
                    return true;
                }
            }

            return false;
        }

        private void OnScreenshot(bool front, Sprite sprite)
        {
            if (front)
                m_turningPage.SetFrontSprite(sprite);
            else
                m_turningPage.SetBackSprite(sprite);
        }

        public void TurnPage(bool next_page, bool dirty, bool error)
        {
            if (dirty)
            {
                StartCoroutine(DirtyTurnPage(next_page));
            }
            else
            {
                if (error)
                    TurnPageErrorInternal(next_page);
                else
                    TurnPageInternal(next_page);
            }
        }

        private IEnumerator DirtyTurnPage(bool next_page)
        {
            if (m_requestScreenshotDebug)
                yield return StartCoroutine(m_cameraManager.TakeScreenshot(false));

            if (m_turnPageErrorDebug)
                TurnPageErrorInternal(next_page);
            else
                TurnPageInternal(next_page);

            yield return null;
        }

        private void TurnPageInternal(bool next_page)
        {
            if (!next_page)
                m_turningPage.PreviousPage();
            else
                m_turningPage.NextPage();
        }

        private void TurnPageErrorInternal(bool next_page)
        {
            m_turningPage.TurnPageError(next_page);
        }

        public void MatchBounds(Bounds sprite_bounds)
        {
            Vector3 minWorld = sprite_bounds.min;
            Vector3 maxWorld = sprite_bounds.max;

            m_turningPage.MatchBounds(
                m_cameraManager.GetCameraBase().WorldToScreenPoint(minWorld),
                m_cameraManager.GetCameraBase().WorldToScreenPoint(maxWorld));
        }
    }
}