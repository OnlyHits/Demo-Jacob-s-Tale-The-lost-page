using CustomArchitecture;
using System;
using System.Collections.Generic;
using UnityEngine;

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

    public class AutomaticNavigationSystem<T> : BaseBehaviour where T : Navigable
    {
        [SerializeField] private List<T>    m_navigables;
        private T                           m_currentNavigable = null;

        public List<T> GetNavigables() => m_navigables;

        public void Navigate(NavigationDirection direction)
        {
            if (m_navigables == null || m_navigables.Count == 0)
                return;

            // ensure that we focus a navigable
            if (m_currentNavigable == null)
            {
                m_currentNavigable = m_navigables[0];
            }

            var nav = m_currentNavigable.GetLinkedNavigable(direction);

            if (nav != null)
            {
                m_currentNavigable.Unfocus();
                m_currentNavigable = (T)nav;
                m_currentNavigable.Focus();
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
            foreach (var nav in m_navigables)
            {
                ((Navigable)nav).Init(m_navigables);
            }
        }
        #endregion
    }
}
