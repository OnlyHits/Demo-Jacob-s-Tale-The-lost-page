using Unity.XR.OpenVR;
using UnityEngine;

namespace CustomArchitecture
{
    [RequireComponent(typeof(Animator))]
    public class VFXSingleClip : APoolElement
    {
        private Animator m_animator = null;
        private string m_clipName = null;
        private bool m_isSetup = false;

        private void Awake()
        {
            m_animator = GetComponent<Animator>();

            if (m_animator == null)
            {
                Debug.LogWarning("Vfx single clip doesn't have animator (" + gameObject.name + ")");
                return;
            }

            AnimationClip[] clips = m_animator.runtimeAnimatorController.animationClips;

            if (clips.Length != 1)
            {
                Debug.LogError("Vfx single clip requires exactly ONE animation clip (" + gameObject.name + ")");
                return;
            }

            m_clipName = clips[0].name;

            m_isSetup = true;
        }

        private void Update()
        {
            if (!Compute && m_isSetup)
                return;

            AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(0);

            if (stateInfo.IsName(m_clipName) && stateInfo.normalizedTime >= 1f)
            {
                Compute = false;
            }
        }

        #region Pool element
        public override void OnAllocate(params object[] parameter)
        {
            if (!m_isSetup)
            {
                Compute = false;
                return;
            }

            m_animator.Play(m_clipName, 0, 0f);
            Compute = true;
        }
        public override void OnDeallocate()
        {
            Compute = false;
        }
        #endregion Pool element
    }
}