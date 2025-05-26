using UnityEngine.InputSystem;
using UnityEngine;
using System;
using System.Collections.Generic;
using static CustomArchitecture.CustomArchitecture;
using UnityEngine.Rendering;

namespace CustomArchitecture
{
    public abstract class AInputManager : BaseBehaviour
    {
        [HideInInspector] public List<InputActionStruct<Vector2>> m_inputActionStructsV2 = new();
        [HideInInspector] public List<InputActionStruct<bool>> m_inputActionStructsBool = new();
        private UpdateType m_updateType = UpdateType.LateUpdate;

        public void SetUpdateType(UpdateType type)
        {
            m_updateType = type;
        }

        public enum UpdateType
        {
            Update,
            FixedUpdate,
            LateUpdate
        };

        [Serializable]
        public class InputActionStruct<T>
        {
            public InputActionStruct(InputAction a, Action<InputType, T> cb, T v, bool cmpEchFrm = false)
            {
                action = a;
                callback = cb;
                value = v;
                computeEachFrame = cmpEchFrm;
            }

            public InputAction action;
            public Action<InputType, T> callback;
            public T value;

            // Invoke the callback each frame the key is pressed
            public bool computeEachFrame;

            public void InvokeCallback(InputType i, T v)
            {
                callback?.Invoke(i, v);
            }

            public bool IsSameValue(T v)
            {
                bool isSame = false;
                if (value.GetType() == typeof(Vector2))
                {
                    var vDest = (Vector2)Convert.ChangeType(v, typeof(Vector2));
                    var vSrc = (Vector2)Convert.ChangeType(value, typeof(Vector2));
                    isSame = vDest == vSrc;
                }
                if (value.GetType() == typeof(bool))
                {
                    var vDest = (bool)Convert.ChangeType(v, typeof(bool));
                    var vSrc = (bool)Convert.ChangeType(value, typeof(bool));
                    isSame = vDest == vSrc;
                }
                return isSame;
            }

            public void SetValue(T v)
            {
                value = v;
            }

            public T GetValue()
            {
                if (value.GetType() == typeof(Vector2))
                {
                    Vector2 res = action.ReadValue<Vector2>();
                    return (T)Convert.ChangeType(res, typeof(T));
                }
                if (value.GetType() == typeof(bool))
                {
                    bool res = action.IsPressed();
                    return (T)Convert.ChangeType(res, typeof(T));
                }

                return value;
            }
        }

        private void TryRegisteredAction()
        {
            foreach (var ias in m_inputActionStructsV2)
            {
                TryGetAction<Vector2>(ias);
            }
            foreach (var ias in m_inputActionStructsBool)
            {
                TryGetAction<bool>(ias);
            }
        }

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        {
            if (m_updateType == UpdateType.FixedUpdate)
                TryRegisteredAction();
        }
        protected override void OnUpdate()
        {
            if (m_updateType == UpdateType.Update)
                TryRegisteredAction();
        }
        protected override void OnLateUpdate()
        {
            if (m_updateType == UpdateType.LateUpdate)
                TryRegisteredAction();
        }
        #endregion BaseBehaviour


        private void TryGetAction<T>(InputActionStruct<T> inputActStruct)
        {
            InputType input = GetInputType(inputActStruct.action);
            T value = inputActStruct.GetValue();

            if (input == InputType.NONE) return;

            if (input == InputType.PRESSED || input == InputType.RELEASED)
            {
                inputActStruct.InvokeCallback(input, value);
            }

            else if (inputActStruct.computeEachFrame)
            {
                // Invoke callback each frame
                if (input == InputType.COMPUTED)
                {
                    inputActStruct.InvokeCallback(input, value);
                }
            }
            else
            {
                // Invoke callback once
                if (inputActStruct.IsSameValue(value) == false)
                {
                    inputActStruct.InvokeCallback(input, value);
                }
            }
            inputActStruct.SetValue(value);
        }

        private InputType GetInputType(InputAction action)
        {
            InputType input = InputType.NONE;
            bool pressed = action.WasPressedThisFrame();
            bool released = action.WasReleasedThisFrame();
            bool isComputed = action.IsPressed();

            input = pressed ? InputType.PRESSED : input;
            input = released ? InputType.RELEASED : input;
            input = !pressed && !released && isComputed ? InputType.COMPUTED : input;

            return input;
        }

    }
}
