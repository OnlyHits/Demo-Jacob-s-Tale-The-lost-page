using UnityEngine;
using CustomArchitecture;
using Comic;

namespace Comc
{
    public class ComicCinemachineMgr : CinemachineMgr<ComicCinemachineMgr>
    {
        [SerializeField] private Camera m_mainCamera;
        [SerializeField] private Camera m_perspectivCamera;

        [SerializeField, ReadOnly] private ComicScreenshoter m_screenshoter;
        private float               m_orthoSize;

        public Camera MainCamera { get { return m_mainCamera; } }
        public Camera PerspectivCamera { get { return m_mainCamera; } }
        public ComicScreenshoter Screenshoter { get { return m_screenshoter; } }


        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        {
            if (m_mainCamera != null && m_orthoSize != m_mainCamera.orthographicSize)
            {
                FitOrthoSize();
                m_orthoSize = m_mainCamera.orthographicSize;
            }

            FollowPlayer();
        }
        protected override void OnUpdate()
        { }
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

        #region Perspectiv Camera Behaviour
        private void FitOrthoSize()
        {
            if (m_mainCamera == null || m_perspectivCamera == null)
                return;

            float orthoSize = m_mainCamera.orthographicSize;
            float distance = Mathf.Abs(m_perspectivCamera.transform.localPosition.z);

            float fov = 2f * Mathf.Atan(orthoSize / distance) * Mathf.Rad2Deg;
            m_perspectivCamera.fieldOfView = fov;
        }

        private void FollowPlayer()
        {
            if (ComicGameCore.Instance.MainGameMode.GetPageManager().GetCurrentPage().IsPlayerInFocusedPanel())
            {
                var player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter();

                Vector3 direction = player.transform.position - m_perspectivCamera.transform.position;
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

                m_perspectivCamera.transform.rotation = Quaternion.Slerp(
                    m_perspectivCamera.transform.rotation,
                    targetRotation,
                    Time.deltaTime * 3f
                );
            }
            else
            {
                m_perspectivCamera.transform.rotation = Quaternion.identity;
            }
}
        #endregion Fov & Ortho size
    }
}