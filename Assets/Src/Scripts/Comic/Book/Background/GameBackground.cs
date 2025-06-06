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