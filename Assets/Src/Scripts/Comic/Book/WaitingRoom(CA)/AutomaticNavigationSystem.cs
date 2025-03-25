using System;
using System.Collections.Generic;
using UnityEngine;
using static CustomArchitecture.CustomArchitecture;

namespace CustomArchitecture
{
    [Flags]
    public enum NavigationDirection : int
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3
    }

    // This class doesn't control weither she is active or not.
    // Make sure to call Start/StopNavigate manually everytime it is needed
    public class AutomaticNavigationSystem<T> : BaseBehaviour where T : Navigable
    {
        [SerializeField] protected bool         m_lockNavigation = true;
        [SerializeField] protected List<T>      m_navigables;
        [SerializeField] private T              m_startingNavigable = null;
        protected T                             m_focusedNavigable = null;
        private float                           m_delayComputed = .2f;
        private float                           m_timer = 0f;

        private NavigationDirection GetDirection(Vector2 input)
        {
            if (input == Vector2.zero)
                return NavigationDirection.None;

            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            {
                return input.x > 0 ? NavigationDirection.Right : NavigationDirection.Left;
            }
            else
            {
                return input.y > 0 ? NavigationDirection.Up : NavigationDirection.Down;
            }
        }

        protected void OnNavigate(InputType input, Vector2 v)
        {

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

            m_timer = 0f;
            m_lockNavigation = false;
        }

        public void StopNavigate()
        {
            if (m_focusedNavigable != null)
                m_focusedNavigable.Unfocus();

            m_timer = 0f;
            m_lockNavigation = true;
        }

        public void ChangeFocus(NavigationDirection direction)
        {
            if (m_focusedNavigable != null)
            {
                T n_focus = (T)m_focusedNavigable.GetLinkedNavigable(direction);

                if (n_focus != null)
                {
                    m_focusedNavigable.Unfocus();
                    m_focusedNavigable = n_focus;
                    m_focusedNavigable.Focus();
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
        { }
        #endregion
    }
}
