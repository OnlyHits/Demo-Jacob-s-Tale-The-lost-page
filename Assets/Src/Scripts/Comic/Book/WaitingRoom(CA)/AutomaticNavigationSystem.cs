using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static CustomArchitecture.CustomArchitecture;

namespace CustomArchitecture
{
    // This class doesn't control weither she is active or not.
    // Make sure to call Start/StopNavigate manually everytime it is needed
    public class AutomaticNavigationSystem<T> : BaseBehaviour where T : Navigable
    {
        [SerializeField] protected bool         m_isRunning = false;
        [SerializeField] protected List<T>      m_navigables;
        [SerializeField] private T              m_startingNavigable = null;
        protected T                             m_focusedNavigable = null;

        [SerializeField] private float          m_delayComputed = .2f;
        private float                           m_timer = 0f;

        // make a setter
        protected Action<T>                     m_onChangeFocus;

        public bool IsRunning() => m_isRunning;
        public T GetStartingNavigable() => m_startingNavigable;
        public List<T> GetNavigables() => m_navigables;

        private Direction GetDirection(Vector2 input)
        {
            if (input == Vector2.zero)
                return Direction.None;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                return input.y > 0 ? Direction.Up : Direction.Down;
            }
        }

        protected void OnNavigate(InputType input, Vector2 v)
        {
            if (!m_isRunning)
                return;

            if (input == InputType.PRESSED)
            {
                ChangeFocus(GetDirection(v));
            }
            else if (input == InputType.COMPUTED)
            {
                if (m_timer >= m_delayComputed)
                {
                    ChangeFocus(GetDirection(v));
                    m_timer = 0f;
                }
                else
                    m_timer += Time.deltaTime;
            }
            else if (input == InputType.RELEASED)
            {
                m_timer = 0f;
            }
        }

        public void StartNavigate()
        {
            // can't start if there is no navigables
            if (m_navigables == null || m_navigables.Count == 0)
                return;

            // make sure to have a starting view set
            if (m_startingNavigable == null)
                m_startingNavigable = m_navigables[0];

            m_startingNavigable.Focus();

            m_focusedNavigable = m_startingNavigable;

            m_onChangeFocus(m_focusedNavigable);

            m_timer = 0f;
            m_isRunning = true;
        }

        public void StopNavigate()
        {
            if (m_focusedNavigable != null)
                m_focusedNavigable.Unfocus();

            m_focusedNavigable = null;
            m_timer = 0f;
            m_isRunning = false;
        }

        public void ChangeFocus(Direction direction)
        {
            if (m_focusedNavigable != null)
            {
                T n_focus = (T)m_focusedNavigable.GetLinkedNavigable(direction);

                if (n_focus != null)
                {
                    m_focusedNavigable.Unfocus();
                    m_focusedNavigable = n_focus;
                    m_focusedNavigable.Focus();

                    m_onChangeFocus(m_focusedNavigable);
                }
            }
        }

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
            // make sure to have a starting view set
            if (m_startingNavigable == null && m_navigables != null && m_navigables.Count > 0)
                m_startingNavigable = m_navigables[0];

        }
        #endregion
    }
}
