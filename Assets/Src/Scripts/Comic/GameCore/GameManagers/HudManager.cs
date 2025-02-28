using CustomArchitecture;
using System;
using static PageHole;
using UnityEngine;

namespace Comic
{
    public class HudManager : BaseBehaviour
    {
        private ViewManager m_viewManager;
        private HudCameraRegister m_cameras;
        private NavigationInput m_navigationInput;
        public ViewManager GetViewManager() => m_viewManager;
        public HudCameraRegister GetRegisteredCameras() => m_cameras;
        public NavigationInput GetNavigationInput() => m_navigationInput;

        public void RegisterToEndTurning(Action function) => m_cameras.RegisterToEndTurning(function);

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            m_viewManager.LateInit();
            m_cameras.LateInit();
            m_navigationInput.LateInit();
        }
        public override void Init(params object[] parameters)
        {
            m_viewManager = gameObject.GetComponent<ViewManager>();
            m_cameras = gameObject.GetComponent<HudCameraRegister>();
            m_navigationInput = GetComponent<NavigationInput>();

            // will change that
            m_cameras.Init(ComicGameCore.Instance.MainGameMode.GetCameraManager().GetCameraBase(),
                ComicGameCore.Instance.MainGameMode.GetCameraManager().GetScreenshotBounds());
            
            m_viewManager.Init();
            m_navigationInput.Init();
        }
        #endregion
    }
}