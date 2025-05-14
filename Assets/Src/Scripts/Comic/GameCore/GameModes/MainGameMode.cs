using System;
using Sirenix.OdinInspector;
using CustomArchitecture;
using UnityEngine;
using System.Collections.Generic;
using static CustomArchitecture.CustomArchitecture;
using System.Collections;

namespace Comic
{
    public interface MainGameModeProvider
    {
        public List<ChapterSavedData> GetUnlockChaptersData();
        public GameConfig GetGameConfig();
        public PageManager GetPageManager();
        public NewCharacterManager GetCharacterManager();

        public void UnlockVoice(VoiceType type, bool force_unlock);
        public void UnlockPower(PowerType type, bool force_unlock);
        public void UnlockChapter(Chapters type, bool unlock_voice, bool unlock_power);
        public void LockChapter(Chapters type);

        public void SubscribeToLockVoice(Action<VoiceType> function);
        public void SubscribeToLockPower(Action<PowerType> function);
        public void SubscribeToLockChapter(Action<Chapters> function);

        public void SubscribeToUnlockVoice(Action<VoiceType> function);
        public void SubscribeToUnlockPower(Action<PowerType> function);
        public void SubscribeToUnlockChapter(Action<Chapters> function);

        public void SubscribeToPowerSelected(Action<PowerType> function);
        public void SubscribeToNextPower(Action function);
        public void SubscribeToPrevPower(Action function);

        public void TriggerDialogue(DialogueName type);

        public void ClearSaveDebug();
    }

    public class MainGameMode : AGameMode<ComicGameCore>, MainGameModeProvider
    {
#if UNITY_EDITOR && !DEVELOPMENT_BUILD
        private bool m_playStartAnimation_DEBUG = false;
#endif

        // globals datas
        private GameConfig m_gameConfig;
        private GameProgression m_gameProgression;
        private URP_CameraManager m_cameraManager;

        // local datas
        private HudManager m_hudManager;
        private GameManager m_gameManager;
        private NavigationManager m_navigationManager;

        private Action<VoiceType> m_onUnlockVoiceCallback;
        private Action<PowerType> m_onUnlockPowerCallback;
        private Action<Chapters> m_onUnlockChapterCallback;

        private Action<VoiceType> m_onLockVoiceCallback;
        private Action<PowerType> m_onLockPowerCallback;
        private Action<Chapters> m_onLockChapterCallback;

        private Action m_onEndGame;

        // ---- really necessary? ----
        public List<ChapterSavedData> GetUnlockChaptersData() => m_gameProgression.GetUnlockedChaptersDatas();

        // ---- MainGameCore dependencies ----
        public GameProgression GetGameProgression() => m_gameProgression;
        public GameConfig GetGameConfig() => m_gameConfig;
        public URP_CameraManager GetCameraManager() => m_cameraManager;
        public NavigationManager GetNavigationManager() => m_navigationManager;

        // ---- Sub managers ----
        public PageManager GetPageManager() => m_gameManager?.GetPageManager();
        public NewCharacterManager GetCharacterManager() => m_gameManager?.GetCharacterManager();
        public PowerManager GetPowerManager() => m_gameManager?.GetPowerManager();
        public DialogueManager GetDialogueManager() => m_gameManager?.GetDialogueManager();
        public ViewManager GetViewManager() => m_hudManager?.GetViewManager();
        public HudManager GetHudManager() => m_hudManager;

        public override void InitGameMode(params object[] parameters)
        {
            base.InitGameMode(parameters);

            m_gameSceneName = "GameScene";
            m_uiSceneName = "HudScene";

            m_gameConfig = SerializedScriptableObject.CreateInstance<GameConfig>();
            m_gameProgression = new GameProgression();

            ComponentUtils.GetOrCreateComponent<NavigationManager>(gameObject, out m_navigationManager);

            m_cameraManager = GetComponentInChildren<URP_CameraManager>(); // should be in AGameCore
            m_cameraManager.Init(); // AGameCore too

            m_gameCore.GetGlobalInput().onPause += OnPause;
        }

        // todo : check if resources.Load is done on one frame or multiple
        // Resource.Load is obsolete, now pass by AddressableFactory
        public override IEnumerator LoadGameMode()
        {
            m_hudManager = ComponentUtils.FindObjectAcrossScenes<HudManager>();
            m_gameManager = ComponentUtils.FindObjectAcrossScenes<GameManager>();

            m_navigationManager.Init(m_gameManager, m_hudManager, m_gameCore.GetGlobalInput(), m_cameraManager);

            if (GetUnlockChaptersData().Count == 0)
            {
                UnlockChapter(Chapters.The_Prequel, false, false);
            }

            if (m_gameManager != null)
            {
                yield return StartCoroutine(m_gameManager.Load());

                m_gameManager.Init();
                m_cameraManager.RegisterCameras(m_gameManager.GetRegisteredCameras());
            }
            if (m_hudManager != null)
            {
                m_hudManager.Init(GetCameraManager());
                m_cameraManager.RegisterCameras(m_hudManager.GetRegisteredCameras());
            }

            if (m_gameManager != null)
            {
                m_gameManager.LateInit();
            }
            if (m_hudManager != null)
            {
                m_hudManager.LateInit(m_gameManager.GetCoverSpriteRenderer(true));
            }

            GetDialogueManager().SubscribeToEndDialogue(OnEndMainDialogue);

            // Update : is okay but should have a globally better handle of Init/LateInit 
            m_navigationManager.LateInit();

            m_cameraManager.LateInit(
                m_gameManager.GetCoverSpriteRenderer(true),
                m_gameManager.GetCoverSpriteRenderer(false),
                m_gameManager.GetPageSpriteRenderer(true),
                m_gameManager.GetPageSpriteRenderer(false));

            Compute = true;

#if UNITY_EDITOR && !DEVELOPMENT_BUILD
            if (!m_playStartAnimation_DEBUG)
            {
                m_gameManager.GetPageManager().SetStartingPage();
            }
#endif

            yield return new WaitForEndOfFrame();

            m_gameCore.OnGameModeLoaded();
        }

        public override void StartGameMode()
        {
#if UNITY_EDITOR && !DEVELOPMENT_BUILD
            if (m_playStartAnimation_DEBUG)
            {
                m_navigationManager.StartGameSequence();
            }
#else
            m_navigationManager.StartGameSequence();
#endif
        }

        public void OnEndMainDialogue(DialogueName type)
        {
            if (type == DialogueName.Dialogue_UnlockBF)
                UnlockChapter(Chapters.The_First_Chapter, true, true);
            else if (type == DialogueName.Dialogue_UnlockBully)
                UnlockChapter(Chapters.The_Second_Chapter, true, true);
            else if (type == DialogueName.Dialogue_UnlockBL)
                UnlockChapter(Chapters.The_Third_Chapter, true, true);
            else if (type == DialogueName.Dialogue_UnlockBoss)
                PlayEndGame();
        }

        public void TriggerDialogue(DialogueName type)
        {
            //if (GetDialogueManager().StartDialogue(type))
            //    GetCharacterManager().GetPlayer().Pause(true);
        }

        #region END GAME

        public void PlayEndGame()
        {
            //GetCharacterManager().GetPlayer().Pause(true);

            GetViewManager().Show<CreditView>();
            GetDialogueManager().StartCreditDialogue();

            m_onEndGame?.Invoke();
        }

        public void SubscribeToEndGame(Action function)
        {
            m_onEndGame -= function;
            m_onEndGame += function;
        }

        #endregion END GAME

        #region Progression

        public void UnlockVoice(VoiceType type, bool force_unlock = false)
        {
            Chapters target_chapter = m_gameConfig.GetChapterByVoice(type);

            if (!m_gameProgression.HasUnlockChapter(target_chapter))
            {
                if (force_unlock)
                {
                    UnlockChapter(target_chapter, false, false);
                }
                else
                {
                    Debug.LogWarning("Couldn't unlock voice, unlock chapter first");
                    return;
                }
            }

            if (m_gameProgression.HasUnlockVoice(target_chapter))
            {
                Debug.LogWarning("You already unlock this voice");
                return;
            }

            m_gameProgression.UnlockVoice(target_chapter);
            m_onUnlockVoiceCallback?.Invoke(type);
        }

        public void UnlockPower(PowerType type, bool force_unlock = false)
        {
            Chapters target_chapter = m_gameConfig.GetChapterByPower(type);

            if (!m_gameProgression.HasUnlockChapter(target_chapter))
            {
                if (force_unlock)
                {
                    UnlockChapter(target_chapter, false, true);
                    return;
                }
                else
                {
                    Debug.LogWarning("Couldn't unlock voice, unlock chapter first");
                    return;
                }
            }

            if (m_gameProgression.HasUnlockPower(target_chapter))
            {
                Debug.LogWarning("You already unlock this power");
                return;
            }

            m_gameProgression.UnlockPower(target_chapter);
            m_onUnlockPowerCallback?.Invoke(type);
        }

        public void UnlockChapter(Chapters type, bool unlock_voice, bool unlock_power)
        {
            if (m_gameProgression.HasUnlockChapter(type))
            {
                Debug.LogWarning("You already unlock this chapter");
                return;
            }

            m_gameProgression.UnlockChapter(type);

            ChapterConfig chapterConfig = m_gameConfig.GetChapterDatas(type);

            if (chapterConfig != null)
            {
                if (unlock_voice)
                    UnlockVoice(chapterConfig.m_voiceType, false);

                if (unlock_power)
                    UnlockPower(chapterConfig.m_powerType, false);
            }

            m_onUnlockChapterCallback?.Invoke(type);
        }

        public void LockChapter(Chapters type)
        {
            if (!m_gameProgression.HasUnlockChapter(type))
            {
                Debug.LogWarning("Chapter already locked");
                return;
            }

            m_gameProgression.LockChapter(type);

            m_onLockChapterCallback?.Invoke(type);
            m_onLockVoiceCallback?.Invoke(m_gameConfig.GetVoiceByChapter(type));
            m_onLockPowerCallback?.Invoke(m_gameConfig.GetPowerByChapter(type));
        }

        public bool IsChapterUnlocked(Chapters chapterType)
        {
            List<ChapterSavedData> chapterDatas = m_gameProgression.GetUnlockedChaptersDatas();

            foreach (var data in chapterDatas)
            {
                if (data.m_chapterType == chapterType)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        public void ClearSaveDebug()
        {
            m_gameProgression.ClearSaveDebug();
        }

        #region Callbacks

        public void SubscribeToPowerSelected(Action<PowerType> function)
        {
            if (GetDialogueManager() == null)
            {
                Debug.LogWarning("DialogueManager could not be found");
                return;
            }

            GetDialogueManager().SubscribeToPowerSelected(function);
        }

        public void SubscribeToNextPower(Action function)
        {
            if (GetCharacterManager() == null)
            {
                Debug.LogWarning("Player could not be found");
                return;
            }

//            GetCharacterManager().GetCurrentCharacter().SubscribeToNextPower(function);
        }

        public void SubscribeToPrevPower(Action function)
        {
            if (GetCharacterManager() == null)
            {
                Debug.LogWarning("Page manager could not be found");
                return;
            }

//            GetCharacterManager().GetCurrentCharacter().SubscribeToPrevPower(function);
        }

        public void SubscribeToLockChapter(Action<Chapters> function)
        {
            m_onLockChapterCallback -= function;
            m_onLockChapterCallback += function;
        }

        public void SubscribeToLockPower(Action<PowerType> function)
        {
            m_onLockPowerCallback -= function;
            m_onLockPowerCallback += function;
        }

        public void SubscribeToLockVoice(Action<VoiceType> function)
        {
            m_onLockVoiceCallback -= function;
            m_onLockVoiceCallback += function;
        }

        public void SubscribeToUnlockChapter(Action<Chapters> function)
        {
            m_onUnlockChapterCallback -= function;
            m_onUnlockChapterCallback += function;
        }

        public void SubscribeToUnlockPower(Action<PowerType> function)
        {
            m_onUnlockPowerCallback -= function;
            m_onUnlockPowerCallback += function;
        }

        public void SubscribeToUnlockVoice(Action<VoiceType> function)
        {
            m_onUnlockVoiceCallback -= function;
            m_onUnlockVoiceCallback += function;
        }

        #endregion

        #region PAUSE & GLOBAL INPUT

        private void OnPause(InputType input, bool b)
        {
            if (input == InputType.RELEASED)
            {
                m_navigationManager.TryPause();
            }
        }

        #endregion PAUSE & GLOBAL INPUT

        // destroy all managed objects
        public override void StopGameMode()
        {
            Compute = false;
        }

        // restart all managed gameObject or destroy & instantiate
        public override void RestartGameMode()
        {
            Compute = true;
        }
    }
}
