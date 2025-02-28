using UnityEngine;
using CustomArchitecture;
using UnityEngine.InputSystem;
using static PageHole;

namespace Comic
{
    [DefaultExecutionOrder(-2)]
    public class ComicGameCore : AGameCore<ComicGameCore>
    {

#if UNITY_EDITOR
        [Tooltip("If you init your game or hud scene and don't want to start the main scene, check this")]
        [SerializeField] private bool m_debugStandaloneScene = false;
#endif

        public MainGameMode MainGameMode
        {
            get { return GetGameMode<MainGameMode>(); }
            protected set { }
        }

        protected override void InstantiateGameModes()
        {
            CreateGameMode<MainGameMode>();

#if UNITY_EDITOR
            if (m_debugStandaloneScene == false)
            {
#endif
                SetStartingGameMode<MainGameMode>();
#if UNITY_EDITOR
            }
#endif
        }
    }
}
