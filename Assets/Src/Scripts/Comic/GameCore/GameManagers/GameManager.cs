using CustomArchitecture;
using UnityEngine;

namespace Comic
{
    public class GameManager : BaseBehaviour
    {
        private PageManager m_pageManager;
        private CharacterManager m_characterManager;
        private PowerManager m_powerManager;
        private GameCameraRegister m_cameras;
        private DialogueManager m_dialogueManager;

        public PageManager GetPageManager() => m_pageManager;
        public CharacterManager GetCharacterManager() => m_characterManager;
        public PowerManager GetPowerManager() => m_powerManager;
        public GameCameraRegister GetRegisteredCameras() => m_cameras;
        public DialogueManager GetDialogueManager() => m_dialogueManager;

        public void Init()
        {
            m_pageManager = gameObject.GetComponent<PageManager>();
            m_characterManager = gameObject.GetComponent<CharacterManager>();
            m_powerManager = gameObject.GetComponent<PowerManager>();
            m_dialogueManager = gameObject.GetComponent<DialogueManager>();
            m_cameras = gameObject.GetComponent<GameCameraRegister>();

            m_pageManager.Init();
            m_characterManager.Init();
            m_powerManager.Init();
            m_dialogueManager.Init();
        }

        public void LateInit()
        {
            var viewManager = ComicGameCore.Instance.MainGameMode.GetViewManager();

            if (viewManager != null)
            {
                DialogueView dialogue_view = viewManager.GetView<DialogueView>();
                CreditView credit_view = viewManager.GetView<CreditView>();

                m_dialogueManager.LateInit(dialogue_view, credit_view);
            }

            m_dialogueManager.SubscribeToEndDialogue(ComicGameCore.Instance.GetGameMode<MainGameMode>().OnEndMainDialogue);
            m_pageManager.RegisterSwitchPageManagerCallbacks();
        }
    }
}