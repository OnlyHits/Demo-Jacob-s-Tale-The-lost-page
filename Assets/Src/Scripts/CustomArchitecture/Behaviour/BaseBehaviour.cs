using UnityEngine;

namespace CustomArchitecture
{
    public abstract class BaseBehaviour : MonoBehaviour
    {
        [Header("BaseBehaviour")]
        [ReadOnly, SerializeField] protected bool m_pause;

        protected abstract void OnUpdate();
        protected abstract void OnFixedUpdate();
        protected abstract void OnLateUpdate();

        // is call after GameMode awake
        public abstract void Init(params object[] parameters);
        // is call after all init are done
        public abstract void LateInit(params object[] parameters);

        public bool IsPaused() => m_pause;

        public virtual void Pause(bool pause = true)
        {
            m_pause = pause;
        }

        protected void Update()
        {
            if (m_pause)
                return;

            OnUpdate();
        }

        protected void FixedUpdate()
        {
            if (m_pause)
                return;

            OnFixedUpdate();
        }

        protected void LateUpdate()
        {
            if (m_pause)
                return;

            OnLateUpdate();
        }
    }
}