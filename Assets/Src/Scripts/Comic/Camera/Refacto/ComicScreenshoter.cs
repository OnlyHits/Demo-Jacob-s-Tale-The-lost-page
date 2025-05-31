using UnityEngine;
using System.Collections;
using System.IO;
using CustomArchitecture;
using static Comic.Comic;

namespace Comic
{
    public class ComicScreenshoter : Screenshoter<ComicScreenshot>
    {
        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        public override void Init(params object[] parameters)
        {
            base.Init(parameters);
        }
        #endregion

        public IEnumerator TakeCoverScreenshot()
        {
            if (!m_screenshotDatas.ContainsKey(ComicScreenshot.Screenshot_Cover_Right)
                || !m_screenshotDatas.ContainsKey(ComicScreenshot.Screenshot_Cover_Left))
                yield break;

            m_baseCamera.targetTexture = m_screenshotRenderTexture;

            yield return new WaitForEndOfFrame();

            RenderTexture.active = m_screenshotRenderTexture;

            m_baseCamera.Render();

            CaptureScreenshot(m_screenshotDatas[ComicScreenshot.Screenshot_Cover_Right]);
            CaptureScreenshot(m_screenshotDatas[ComicScreenshot.Screenshot_Cover_Left]);

            yield return null;

            RenderTexture.active = null;
            m_baseCamera.targetTexture = null;
        }

        public IEnumerator TakePageScreenshot()
        {
            if (!m_screenshotDatas.ContainsKey(ComicScreenshot.Screenshot_Page_Right)
                || !m_screenshotDatas.ContainsKey(ComicScreenshot.Screenshot_Page_Left))
                yield break;

            m_baseCamera.targetTexture = m_screenshotRenderTexture;

            yield return new WaitForEndOfFrame();

            RenderTexture.active = m_screenshotRenderTexture;

            m_baseCamera.Render();

            CaptureScreenshot(m_screenshotDatas[ComicScreenshot.Screenshot_Page_Right]);
            CaptureScreenshot(m_screenshotDatas[ComicScreenshot.Screenshot_Page_Left]);

            yield return null;

            RenderTexture.active = null;
            m_baseCamera.targetTexture = null;
        }
    }
}
