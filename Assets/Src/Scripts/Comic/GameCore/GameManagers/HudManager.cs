using UnityEngine;
using CustomArchitecture;

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

        public void Init()
        {
            m_viewManager = gameObject.GetComponent<ViewManager>();
            m_cameras = gameObject.GetComponent<HudCameraRegister>();
            m_navigationInput = GetComponent<NavigationInput>();

            // Init BEFORE navigation input
            m_viewManager.Init();

            // Init AFTER views
            m_navigationInput.Init();

        }
    }
}