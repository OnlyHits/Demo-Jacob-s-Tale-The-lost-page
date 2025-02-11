using UnityEngine;

namespace CustomArchitecture
{
    public abstract class AGameMode<T> : BaseBehaviour where T : AGameCore<T>
    {
        protected string m_gameName;
        protected T m_gameCore;
        [SerializeField, ReadOnly] protected bool m_isCompute;
        protected string m_gameSceneName = null;
        protected string m_uiSceneName = null;

        /// <summary>
        /// This function is called first
        /// </summary>
        public abstract void StartGameMode();

        /// <summary>
        /// Make all dynamic instanciation here
        /// </summary>
        public abstract void OnLoadingEnded();

        /// <summary>
        /// Destroy all managed objects
        /// </summary>
        public abstract void StopGameMode();

        /// <summary>
        /// Restart all managed gameObject or destroy and instantiate
        /// </summary>
        public abstract void RestartGameMode();

        public bool Compute
        {
            get { return m_isCompute; }
            protected set { m_isCompute = value; }
        }

        public virtual void Init(T game_core, params object[] parameters)
        {
            m_gameCore = game_core;
        }

        public string GetName
        {
            get
            {
                return m_gameName;
            }
            set
            {
                m_gameName = value;
            }
        }
    }
}
