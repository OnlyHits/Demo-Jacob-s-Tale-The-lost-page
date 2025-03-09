using UnityEngine;

namespace CustomArchitecture
{
    public abstract class BaseBehaviour : MonoBehaviour
    {
        [Header("BaseBehaviour")]
        [ReadOnly, SerializeField] protected bool m_pause;

        /// <summary>
        /// Is called after GameMode awake
        /// </summary>
        /// <param name="parameters"></param>
        public abstract void Init(params object[] parameters);
        
        /// <summary>
        /// Is call after all init are done
        /// </summary>
        /// <param name="parameters"></param>
        public abstract void LateInit(params object[] parameters);

        protected abstract void OnUpdate();
        protected abstract void OnFixedUpdate();
        protected abstract void OnLateUpdate();

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