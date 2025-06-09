using UnityEngine;

namespace CustomArchitecture
{
    // after reflection, make it heritate from BaseBehaviour
    // is not a bad idea, for pause purpose
    public abstract class APoolElement : BaseBehaviour
    {
        private bool m_isCompute;

        public bool Compute
        {
            get { return m_isCompute; }
            set { m_isCompute = value; }
        }

        // this is override here to keep consistancy since derived from BaseBahaviour 
        public override void Init(params object[] parameters)
        { }
        public override void LateInit(params object[] parameters)
        { }
        protected override void OnUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnFixedUpdate()
        { }

        public abstract void OnAllocate(params object[] parameter);
        public abstract void OnDeallocate();
    }
}