using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using static PageHole;

namespace Comic
{
    public class Power : BaseBehaviour
    {
        [SerializeField] private PowerType m_powerType = PowerType.Power_None;
        public PowerType GetPowerType() => m_powerType;

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


        public virtual void Activate(bool activate = true)
        {

        }

    }
}
