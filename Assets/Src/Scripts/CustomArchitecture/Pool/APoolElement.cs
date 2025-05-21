using UnityEngine;

namespace CustomArchitecture
{
    // after reflection, make it heritate from BaseBehaviour
    // is not a bad idea, for pause purpose
    public abstract class APoolElement : MonoBehaviour
    {
        private bool m_isCompute;

        public bool Compute
        {
            get { return m_isCompute; }
            set { m_isCompute = value; }
        }

        public abstract void OnAllocate(params object[] parameter);
        public abstract void OnDeallocate();
    }
}