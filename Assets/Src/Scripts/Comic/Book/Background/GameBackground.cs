using Comc;
using CustomArchitecture;
using UnityEngine;
using static CustomArchitecture.CustomArchitecture;

namespace Comic
{
    public class GameBackground : BaseBehaviour
    {
        [SerializeField] private GameObject                 m_backgroundVisual;
        [SerializeField] private CinemachineCameraExtended  m_cinemachineCamera = null;
        public CinemachineCameraExtended GetCinemachineCamera() => m_cinemachineCamera;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            m_cinemachineCamera.LateInit();
            m_cinemachineCamera.FitBounds(m_backgroundVisual.GetComponent<SpriteRenderer>().bounds);

            // well... not good but whatever for the moment
            var forward_camera = ComicCinemachineMgr.Instance.ForwardCamera;
            var bounds = m_backgroundVisual.GetComponent<SpriteRenderer>().bounds;

            float distance = Mathf.Abs(forward_camera.transform.localPosition.z * forward_camera.transform.lossyScale.z);

            float panelWidth = bounds.size.x * m_cinemachineCamera.WidthFactor;
            float panelHeight = bounds.size.y * m_cinemachineCamera.HeightFactor;

            float aspect = (float)Screen.width / Screen.height;

            float fovVerticalRad = 2f * Mathf.Atan((panelHeight / 2f) / distance);
            float fovHorizontalRad = 2f * Mathf.Atan((panelWidth / 2f) / distance);

            float fovHorizontalToVerticalRad = 2f * Mathf.Atan(Mathf.Tan(fovHorizontalRad / 2f) / aspect);

            float finalFovRad = Mathf.Max(fovVerticalRad, fovHorizontalToVerticalRad);
            float finalFovDeg = Mathf.Clamp(finalFovRad * Mathf.Rad2Deg, 1f, 179f);

            forward_camera.fieldOfView = finalFovDeg;
        }
        public override void Init(params object[] parameters)
        {
            m_cinemachineCamera.Init();
            ComicCinemachineMgr.Instance.RegisterPermanentCamera(m_cinemachineCamera.Camera);
        }
        #endregion

        public void EnableVisual(bool enable)
        {
            m_backgroundVisual.SetActive(enable);
        }

    }
}