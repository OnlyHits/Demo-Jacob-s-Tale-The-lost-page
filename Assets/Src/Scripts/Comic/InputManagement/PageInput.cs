using CustomArchitecture;
using UnityEngine.InputSystem;
using UnityEngine;
using System;
using static CustomArchitecture.CustomArchitecture;

namespace Comic
{
    public class PageInput : AInputManager
    {
        #region ACTIONS
        private InputAction m_nextPageAction;
        private InputAction m_prevPageAction;
        #endregion ACTIONS

        #region CALLBACKS
        private Action<InputType, bool> onNextPageAction;
        private Action<InputType, bool> onPrevPageAction;

        public void SubscribeToPreviousPage(Action<InputType, bool> function)
        {
            onPrevPageAction -= function;
            onPrevPageAction += function;
        }

        public void SubscribeToNextPage(Action<InputType, bool> function)
        {
            onNextPageAction -= function;
            onNextPageAction += function;
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
            m_nextPageAction = ComicGameCore.Instance.GetInputAsset().FindAction("NextPage");
            m_prevPageAction = ComicGameCore.Instance.GetInputAsset().FindAction("PrevPage");
        }

        private void InitInputActions()
        {
            InputActionStruct<bool> iNextPage = new InputActionStruct<bool>(m_nextPageAction, onNextPageAction, false);
            InputActionStruct<bool> iPrevPage = new InputActionStruct<bool>(m_prevPageAction, onPrevPageAction, false);

            // in case of reloading the game
            m_inputActionStructsBool.Clear();

            m_inputActionStructsBool.Add(iNextPage);
            m_inputActionStructsBool.Add(iPrevPage);
        }
    }
}