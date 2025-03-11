using UnityEngine;

namespace CustomArchitecture
{
    public abstract class APoolElement : MonoBehaviour
    {
        private bool m_isCompute;

        public bool Compute
        {
            get { return m_isCompute; }
            set { m_isCompute = value; }
        }

        // Call after on allocate
        public abstract void OnAllocate(params object[] parameter);
        public abstract void OnDeallocate();
    }
}