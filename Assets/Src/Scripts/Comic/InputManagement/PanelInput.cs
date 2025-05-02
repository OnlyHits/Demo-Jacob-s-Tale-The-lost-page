using CustomArchitecture;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using static CustomArchitecture.CustomArchitecture;

namespace Comic
{
    public class PanelInput : AInputManager
    {
        #region ACTIONS
        private InputAction m_navigationAction;
        private InputAction m_interactAction;
        private InputAction m_nextBehaviour;
        private InputAction m_prevBehaviour;

        #endregion ACTIONS

        #region CALLBACKS
        public Action<InputType, Vector2> onNavigationAction;
        public Action<InputType, bool> onInteractAction;
        public Action<InputType, bool> onNextBehaviourAction;
        public Action<InputType, bool> onPrevBehaviourAction;

        public void SubscribeToNavigate(Action<InputType, Vector2> function)
        {
            onNavigationAction -= function;
            onNavigationAction += function;
        }
        public void SubscribeToInteract(Action<InputType, bool> function)
        {
            onInteractAction -= function;
            onInteractAction += function;
        }
        public void SubscribeToPrevBehaviour(Action<InputType, bool> function)
        {
            onNextBehaviourAction -= function;
            onNextBehaviourAction += function;
        }
        public void SubscribeToNextBehaviour(Action<InputType, bool> function)
        {
            onPrevBehaviourAction -= function;
            onPrevBehaviourAction += function;
        }

        #endregion CALLBACKS

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
            m_navigationAction = inputActionAsset.FindAction("PanelNavigation/PanelNav");
            m_interactAction = inputActionAsset.FindAction("PanelNavigation/ActiveBehaviour");
            m_nextBehaviour = inputActionAsset.FindAction("PanelNavigation/NextBehaviour");
            m_prevBehaviour = inputActionAsset.FindAction("PanelNavigation/PrevBehaviour");
        }

        private void InitInputActions()
        {
            InputActionStruct<Vector2> iNavigate = new InputActionStruct<Vector2>(m_navigationAction, onNavigationAction, Vector2.zero, true);
            InputActionStruct<bool> iInteract = new InputActionStruct<bool>(m_interactAction, onInteractAction, false);
            InputActionStruct<bool> iNextBehaviour = new InputActionStruct<bool>(m_nextBehaviour, onNextBehaviourAction, false);
            InputActionStruct<bool> iPrevBehaviour = new InputActionStruct<bool>(m_prevBehaviour, onPrevBehaviourAction, false);

            // in case of reloading the game
            m_inputActionStructsV2.Clear();
            m_inputActionStructsBool.Clear();

            m_inputActionStructsV2.Add(iNavigate);
            m_inputActionStructsBool.Add(iInteract);
            m_inputActionStructsBool.Add(iNextBehaviour);
            m_inputActionStructsBool.Add(iPrevBehaviour);
        }
    }
}