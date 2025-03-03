using CustomArchitecture;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using static CustomArchitecture.CustomArchitecture;
using static PageHole;


namespace Comic
{
    public class NavigationInput : AInputManager
    {
        #region ACTIONS
        private InputAction m_navigationAction;
        private InputAction m_validateAction;
        private InputAction m_cancelAction;

        public InputAction GetValidateAction() => m_validateAction;
        public InputAction GetCancelAction() => m_cancelAction;
        #endregion ACTIONS


        #region CALLBACKS
        public Action<InputType, Vector2> onNavigationAction;
        public Action<InputType, bool> onValidateAction;
        public Action<InputType, bool> onCancelAction;

        #endregion CALLBACKS


        #region SUB CALLBACKS
        public void SubscribeToNavigate(Action<InputType, Vector2> function)
        {
            onNavigationAction -= function;
            onNavigationAction += function;
        }

        public void SubscribeToValidate(Action<InputType, bool> function)
        {
            onValidateAction -= function;
            onValidateAction += function;
        }

        public void SubscribeToCancel(Action<InputType, bool> function)
        {
            onCancelAction -= function;
            onCancelAction += function;
        }
        #endregion SUB CALLBACKS

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
            FindAction();
        }
        #endregion

        private void FindAction()
        {
            m_navigationAction = ComicGameCore.Instance.GetInputAsset().FindAction("Navigation");
            m_cancelAction = ComicGameCore.Instance.GetInputAsset().FindAction("Cancel");
            m_validateAction = ComicGameCore.Instance.GetInputAsset().FindAction("Validate");

        }

        private void InitInputActions()
        {
            InputActionStruct<Vector2> iNavigate = new InputActionStruct<Vector2>(m_navigationAction, onNavigationAction, Vector2.zero, true);
            InputActionStruct<bool> iValidate = new InputActionStruct<bool>(m_validateAction, onValidateAction, false);
            InputActionStruct<bool> iCancel = new InputActionStruct<bool>(m_cancelAction, onCancelAction, false);

            // Clear struct in case of already defined list (if reloaded for example)
            m_inputActionStructsV2.Clear();
            m_inputActionStructsBool.Clear();

            m_inputActionStructsV2.Add(iNavigate);
            m_inputActionStructsBool.Add(iValidate);
            m_inputActionStructsBool.Add(iCancel);
        }

        public override void Pause(bool pause)
        {
            base.Pause(pause);
        }
    }
}