using CustomArchitecture;
using UnityEngine;
using System.Collections;
using static Comic.Comic;
using Comc;

namespace Comic
{
    public class HudManager : BaseBehaviour
    {
        [SerializeField] private HudTurnPageManager m_hudPageMgr;
        private HudCameraRegister m_cameras;
        private ViewManager m_viewManager;
//        private URP_CameraManager m_cameraManager;
        private bool m_requestScreenshotDebug;
        private bool m_turnPageErrorDebug;

        public ViewManager GetViewManager() => m_viewManager;
        public HudCameraRegister GetRegisteredCameras() => m_cameras;
        public HudTurnPageManager GetHudPageMgr() => m_hudPageMgr;

        public void SetFrontSprite(Sprite sprite) => m_hudPageMgr.SetFrontSprite(sprite);
        public void SetBackSprite(Sprite sprite) => m_hudPageMgr.SetBackSprite(sprite);

        public IEnumerator TurnMultiplePages(bool is_next, Bounds page_bounds, int page_number, float duration)
        {
            yield return StartCoroutine(m_hudPageMgr.TurnMultiplePagesCoroutine(is_next, page_bounds, ComicCinemachineMgr.Instance.MainCamera, page_number, duration));
        }

        public IEnumerator TurnCover(Bounds page_bounds, float duration)
        {
            yield return StartCoroutine(m_hudPageMgr.TurnCoverCoroutine(true, page_bounds, ComicCinemachineMgr.Instance.MainCamera, duration));
        }

        public IEnumerator TurnPage(bool next_page, Bounds page_bounds, float duration)
        {
            yield return StartCoroutine(m_hudPageMgr.TurnPageCoroutine(next_page, page_bounds, ComicCinemachineMgr.Instance.MainCamera, duration));
        }

        public IEnumerator TurnPageError(bool next_page, Bounds page_bounds, float duration)
        {
            yield return StartCoroutine(m_hudPageMgr.TurnPageErrorCoroutine(next_page, page_bounds, ComicCinemachineMgr.Instance.MainCamera, duration));
        }


        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }

        public override void LateInit(params object[] parameters)
        {
            if (parameters.Length < 1 || parameters[0] is not SpriteRenderer)
            {
                Debug.LogWarning("Bad parameters");
                return;
            }

            m_viewManager.LateInit();
            m_cameras.LateInit();
            m_hudPageMgr.LateInit();

            SetupBookCoverView(((SpriteRenderer)parameters[0]).bounds);
        }

        public override void Init(params object[] parameters)
        {
            //if (parameters.Length != 1 || parameters[0] is not URP_CameraManager)
            //{
            //    Debug.LogWarning("Unable to get URP_CameraManager");
            //    return;
            //}

            m_viewManager = gameObject.GetComponent<ViewManager>();
            m_cameras = gameObject.GetComponent<HudCameraRegister>();

            m_cameras.Init();
            m_viewManager.Init();
            m_hudPageMgr.Init();

            //m_cameraManager = (URP_CameraManager)parameters[0];

            ComicCinemachineMgr.Instance.Screenshoter.SubscribeToScreenshot(OnScreenshot);
        }
        #endregion

        private void SetupBookCoverView(Bounds sprite_bounds)
        {
            Vector3 minWorld = sprite_bounds.min;
            Vector3 maxWorld = sprite_bounds.max;

            m_viewManager.GetView<BookCoverView>().MatchBounds(
                ComicCinemachineMgr.Instance.MainCamera.WorldToScreenPoint(minWorld),
                ComicCinemachineMgr.Instance.MainCamera.WorldToScreenPoint(maxWorld));
        }

        public void SetupPageForScreenshot(bool pause)
        {
            if (!pause)
            {
                GetViewManager().Show<ProgressionView>(true);
            }
        }

        public void RestorePageAfterScreenshot(bool pause)
        {
            if (!pause)
            {
                GetViewManager().ShowLast();
            }
            else
            {
                GetViewManager().Show<PauseView>();
            }
        }

        public void OnPageChangeEnd(bool pause)
        {
            if (!pause)
            {
                GetViewManager().Show<ProgressionView>();
            }
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

        private void OnScreenshot(ComicScreenshot type, Sprite sprite)
        {
            if (type == ComicScreenshot.Screenshot_Page_Right || type == ComicScreenshot.Screenshot_Cover_Right)
            {
                m_hudPageMgr.SetFrontSprite(sprite);
            }
            else if (type == ComicScreenshot.Screenshot_Page_Left || type == ComicScreenshot.Screenshot_Cover_Left)
            {
                m_hudPageMgr.SetBackSprite(sprite);
            }
        }

        public void TurnPageDirty(bool next_page, bool error, Bounds page_bounds)
        {
            StartCoroutine(DirtyTurnPage(next_page));
            //else
            //{
            //    if (error)
            //        TurnPageErrorInternal(next_page, page_bounds);
            //    else
            //        TurnPageInternal(next_page, page_bounds);
            //}
        }

        private IEnumerator DirtyTurnPage(bool next_page)
        {
            if (m_requestScreenshotDebug)
                yield return StartCoroutine(ComicCinemachineMgr.Instance.Screenshoter.TakeScreenshot(next_page ? ComicScreenshot.Screenshot_Page_Right : ComicScreenshot.Screenshot_Page_Left));

            if (m_turnPageErrorDebug)
                yield return StartCoroutine(TurnPageError(next_page, new Bounds(), 0.8f));
            else
                yield return StartCoroutine(TurnPage(next_page, new Bounds(), 0.8f));

            //yield return null;
        }

        //public void MatchBounds(Bounds sprite_bounds)
        //{
        //    Vector3 minWorld = sprite_bounds.min;
        //    Vector3 maxWorld = sprite_bounds.max;

        //    m_turningPage.MatchBounds(
        //        m_cameraManager.GetCameraBase().WorldToScreenPoint(minWorld),
        //        m_cameraManager.GetCameraBase().WorldToScreenPoint(maxWorld));
        //}
    }
}