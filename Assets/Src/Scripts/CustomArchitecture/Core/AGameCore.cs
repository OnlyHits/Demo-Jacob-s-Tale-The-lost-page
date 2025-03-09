using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections;
using Comic;

namespace CustomArchitecture
{
    // this class heritate from MonoBehaviour and not BaseBehaviour
    // to ensure that there is no confusion with Init/Update functions
    public abstract class AGameCore<T> : MonoBehaviour where T : AGameCore<T>
    {
        private static T m_instance;

        private readonly List<AGameMode<T>> m_gameModes = new();
        private AGameMode<T> m_currentGameMode = null;
        private AGameMode<T> m_startingGameMode = null;

        // global objects use in all projects
        [SerializeField] private InputActionAsset m_inputActionAsset;
        private Settings m_settings = null;
        private SceneLoader m_sceneLoader = null;
        private GlobalInput m_globalInput;
        private DeviceManager m_deviceManager;

        public Settings GetSettings() => m_settings;
        public SceneLoader GetSceneLoader() => m_sceneLoader;
        public InputActionAsset GetInputAsset() => m_inputActionAsset;
        public GlobalInput GetGlobalInput() => m_globalInput;
        public DeviceManager GetDeviceManager() => m_deviceManager;

        // Prevent direct instantiation
        protected AGameCore() { }

        #region Singleton
        public static T Instance
        {
            get
            {
                // kind of depreciated, until agamecore is abstract
                if (m_instance == null)
                {
                    m_instance = FindFirstObjectByType<T>();

                    if (m_instance == null)
                    {
                        GameObject singletonObject = new GameObject("GameCore");
                        m_instance = singletonObject.AddComponent<T>();
                    }
                }
                return m_instance;
            }
            private set
            {
                m_instance = value;
            }
        }
        #endregion

        protected void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.Log("Already instantiate");
                Destroy(this.gameObject);
                return;
            }

            Instance = (T)this;
            DontDestroyOnLoad(gameObject);

            InstantiateDependencies();
            InstantiateGameModes();
        }

        private void InstantiateDependencies()
        {
            m_settings = new Settings();

            if (!ComponentUtils.GetOrCreateComponent<SceneLoader>(gameObject, out m_sceneLoader))
                Debug.LogError("Unable to create SceneLoader component");
            else
            {
                m_sceneLoader.SubscribeToSceneLoaded(OnSceneLoaded);
//                m_sceneLoader.SubscribeToEndLoading(OnEndLoading);
            }

            if (m_inputActionAsset == null)
                Debug.LogError("You need to assign an InputActionAsset in GameCore prefab");

            if (!ComponentUtils.GetOrCreateComponent<GlobalInput>(gameObject, out m_globalInput))
                Debug.LogError("Unable to create GlobalInput component");
            else
                m_globalInput.Init(m_inputActionAsset);

            if (!ComponentUtils.GetOrCreateComponent<DeviceManager>(gameObject, out m_deviceManager))
                Debug.LogError("Unable to create DeviceManager component");
            else
                m_deviceManager.Init();
        }

        private void Start()
        {
            if (m_startingGameMode != null)
            {
                StartGameMode(m_startingGameMode);
            }
        }

        protected void CreateGameMode<U>(params object[] parameters) where U : AGameMode<T>
        {
            if (Exist<U>())
            {
                Debug.LogError("Game mode already exist");
                return;
            }

            U game_mode = gameObject.AddComponent<U>();

            if (game_mode != null)
            {
                m_gameModes.Add(game_mode);
                game_mode.InitGameMode((T)this, parameters);
            }
            else
            {
                Debug.LogError("Add Component fail");
            }
        }

        protected void SetStartingGameMode<U>() where U : AGameMode<T>
        {
            AGameMode<T> game_mode = GetGameMode<U>();

            if (game_mode != null)
                m_startingGameMode = game_mode;
            else
                Debug.LogError("Game mode doesn't exist");
        }

        private bool Exist(AGameMode<T> game_mode)
        {
            foreach (AGameMode<T> mode in m_gameModes)
            {
                if (mode == game_mode)
                    return true;
            }
            return false;
        }

        private bool Exist<U>() where U : AGameMode<T>
        {
            foreach (AGameMode<T> game_mode in m_gameModes)
            {
                if (game_mode is U)
                    return true;
            }
            return false;
        }

        public U GetGameMode<U>() where U : AGameMode<T>
        {
            foreach (AGameMode<T> game_mode in m_gameModes)
            {
                if (game_mode is U)
                    return (U)game_mode;
            }

            Debug.LogError("Game mode doesn't exist");
            return null;
        }

        private void OnSceneLoaded()
        {
            StartCoroutine(m_currentGameMode.LoadGameMode());
        }

        public void OnGameModeLoaded()
        {
            StartCoroutine(OnGameModeLoadedInternal());
        }

        private IEnumerator OnGameModeLoadedInternal()
        {
            yield return StartCoroutine(m_sceneLoader.UnloadLoadingScene());
            m_currentGameMode.StartGameMode();
        }

        protected void LoadGameMode()
        {
            m_sceneLoader.LoadGameModeScenes(m_currentGameMode.GetUISceneName(), m_currentGameMode.GetGameSceneName());
        }

        protected void StartGameMode(AGameMode<T> game_mode)
        {
            if (!Exist(game_mode))
            {
                Debug.LogError("Game mode doesn't exist");
                return;
            }

            StopGameMode();

            m_currentGameMode = game_mode;

            LoadGameMode();
        }

        public void StartGameMode<U>() where U : AGameMode<T>
        {
            if (!Exist<U>())
            {
                Debug.LogError("Game mode doesn't exist");
                return;
            }

            StopGameMode();

            m_currentGameMode = GetGameMode<U>();

            LoadGameMode();
        }

        private void StopGameMode()
        {
            m_currentGameMode?.StopGameMode();
        }

        protected abstract void InstantiateGameModes();
    }
}