using UnityEngine;
using CustomArchitecture;
using Comic;

namespace Comc
{
    public class ComicCinemachineMgr : CinemachineMgr<ComicCinemachineMgr>
    {
        [SerializeField] private Camera m_mainCamera;

        [SerializeField, ReadOnly] private ComicScreenshoter m_screenshoter;
        private float               m_orthoSize;

        public Camera MainCamera { get { return m_mainCamera; } }
        public ComicScreenshoter Screenshoter { get { return m_screenshoter; } }

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            // hack cause cinemachine blend near plan when switching from otho to perspectiv
            // to an invalid value (-1)
            m_mainCamera.nearClipPlane = 0.01f;
        }
        public override void LateInit(params object[] parameters)
        {
        }
        public override void Init(params object[] parameters)
        {
            if (!ComponentUtils.GetOrCreateComponent<ComicScreenshoter>(gameObject, out m_screenshoter))
                Debug.LogWarning("Unable to get or create PanelInput");
            else
                m_screenshoter.Init(m_mainCamera);
        }
        #endregion
    }
}