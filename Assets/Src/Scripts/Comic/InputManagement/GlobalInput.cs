using CustomArchitecture;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using static CustomArchitecture.CustomArchitecture;

namespace CustomArchitecture
{
    public class GlobalInput : AInputManager
    {

        #region Actions
        // for test purpose, it will be remove and replace by another input class later
        private InputAction m_panelNavAction;
        private InputAction m_pauseAction;
        public InputAction GetPauseAction() => m_pauseAction;
        public InputAction GetPanelActivationAction() => m_panelNavAction;
        #endregion Actions

        #region Callbacks
        public Action<InputType, bool> onPause;
        public Action<InputType, bool> onActivatePanelNav;

        public void SubscribeToActivatePanelNav(Action<InputType, bool> function)
        {
            onActivatePanelNav -= function;
            onActivatePanelNav += function;
        }

        #endregion Callbacks

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
        }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            InitInputActions();
        }

        public override void Init(params object[] parameters)
        {
            if (parameters.Length != 1
                || parameters[0] is not InputActionAsset)
            {
                Debug.LogWarning("GlobalInput : Unable to find InputActionAsset");
                return;
            }

            FindAction((InputActionAsset)parameters[0]);
        }
        #endregion

        private void FindAction(InputActionAsset inputActionAsset)
        {
            m_pauseAction = inputActionAsset.FindAction("Pause");
            m_panelNavAction = inputActionAsset.FindAction("PanelNavigation/ActiveNavigation");
        }

        private void InitInputActions()
        {
            InputActionStruct<bool> iPause = new(m_pauseAction, onPause, false);
            InputActionStruct<bool> iActivePanelNav = new(m_panelNavAction, onActivatePanelNav, false);

            // in case of reloading the game
            m_inputActionStructsBool.Clear();

            m_inputActionStructsBool.Add(iPause);
            m_inputActionStructsBool.Add(iActivePanelNav);
        }
    }
}