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

        public string GetGameSceneName() => m_gameSceneName;
        public string GetUISceneName() => m_uiSceneName;

        /// <summary>
        /// This function is called first
        /// </summary>
        public abstract void StartGameMode();

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

        public override void Init(params object[] parameters)
        {
            if (parameters.Length > 0 && parameters[0] is T game_core)
                m_gameCore = game_core;
            else
                Debug.LogError("AGameMode no game core found");
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
