using UnityEngine;
using CustomArchitecture;
using Comic;
using UnityEngine.Rendering.Universal;

namespace Comc
{
    public class ComicCinemachineMgr : CinemachineMgr<ComicCinemachineMgr>
    {
        [SerializeField, Space] private Camera m_mainCamera;

        [SerializeField, Space, Header("Forward camera")] private Camera m_forwardCamera;
        [SerializeField] private RenderTexture m_forwardRenderTexture;

        [SerializeField, Space, ReadOnly] private ComicScreenshoter m_screenshoter;

        public RenderTexture ForwardRT { get { return m_forwardRenderTexture; } }
        public Camera MainCamera { get { return m_mainCamera; } }
        public Camera ForwardCamera { get { return m_forwardCamera; } }
        public ComicScreenshoter Screenshoter { get { return m_screenshoter; } }

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            // c caca
            if (m_forwardRenderTexture.width != Screen.width || m_forwardRenderTexture.height != Screen.height)
            {
                m_forwardRenderTexture.Release();

                m_forwardRenderTexture.width = Screen.width;
                m_forwardRenderTexture.height = Screen.height;

                m_forwardRenderTexture.Create();
            }
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

            m_forwardCamera.targetTexture = m_forwardRenderTexture;
        }
        #endregion

        #region Camera stacking
        public void RegisterCameraStack(Camera camera)
        {
            var baseData = m_mainCamera.GetUniversalAdditionalCameraData();

            if (!baseData.cameraStack.Contains(camera))
            {
                baseData.cameraStack.Add(camera);
            }
        }
        #endregion
    }
}