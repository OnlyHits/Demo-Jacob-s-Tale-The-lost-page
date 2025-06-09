using CustomArchitecture;
using UnityEngine;

namespace Comic
{
    [RequireComponent(typeof(ParticleSystem))]
    public class FootStepParticle : APoolElement
    {
        [SerializeField] private ParticleSystem m_particleSystem;

        public void Awake()
        {
            if (m_particleSystem == null)
                m_particleSystem = GetComponent<ParticleSystem>();
        }

        protected override void OnUpdate()
        {
            Compute = m_particleSystem.isPlaying;
        }

        public void StopParticleSystem()
        {
            if (m_particleSystem.isPlaying)
                m_particleSystem.Stop();
        }

        #region Pool element
        public override void OnAllocate(params object[] parameter)
        {
            m_particleSystem.Play();
            Compute = true;
        }
        public override void OnDeallocate()
        {
            Compute = false;
        }
        #endregion Pool element
    }
}