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

    public class AutomaticNavigationSystem : BaseBehaviour
    {
        [SerializeField] private List<Navigable>    m_navigables;

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
                nav.Init(m_navigables);
            }
        }
        #endregion
    }
}
