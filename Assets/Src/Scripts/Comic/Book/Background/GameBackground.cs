using CustomArchitecture;
using Unity.Cinemachine;
using UnityEngine;

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
        { }
        public override void Init(params object[] parameters)
        {
            m_cinemachineCamera.Init();
        }
        #endregion

        public void EnableVisual(bool enable)
        {
            m_backgroundVisual.SetActive(enable);
        }

    }
}