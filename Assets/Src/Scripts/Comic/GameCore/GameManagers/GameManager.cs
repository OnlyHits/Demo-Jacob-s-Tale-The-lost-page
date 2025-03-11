using CustomArchitecture;
using UnityEngine;
using static PageHole;

namespace Comic
{
    public class GameManager : BaseBehaviour
    {
        [SerializeField] private SpriteRenderer m_leftPageSR;
        [SerializeField] private SpriteRenderer m_rightPageSR;
        [SerializeField] private SpriteRenderer m_coverRightSR;
        [SerializeField] private SpriteRenderer m_coverLeftSR;

        [SerializeField] private GameObject m_gameBackground;
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

        public SpriteRenderer GetPageSpriteRenderer(bool right)
        {
            return right ? m_rightPageSR : m_leftPageSR;
        }

        public SpriteRenderer GetCoverSpriteRenderer(bool right)
        {
            return right ? m_coverRightSR : m_coverLeftSR;
        }

        public void EnableBookBackground(bool enable, bool right)
        {
            if (right)
            {
                m_coverRightSR.gameObject.SetActive(enable);
                m_rightPageSR.gameObject.SetActive(enable);
            }
            else
            {
                m_coverLeftSR.gameObject.SetActive(enable);
                m_leftPageSR.gameObject.SetActive(enable);
            }
        }

        public void EnableGameBackground(bool enable)
        {
            m_gameBackground.SetActive(enable);
        }

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            if (m_pageManager != null) m_pageManager.LateInit(parameters);
            if (m_characterManager != null) m_characterManager.LateInit(parameters);
            if (m_powerManager != null) m_powerManager.LateInit(parameters);
            if (m_cameras != null) m_cameras.LateInit(parameters);

            var viewManager = ComicGameCore.Instance.MainGameMode.GetViewManager();

            if (viewManager != null)
            {
                DialogueView dialogue_view = viewManager.GetView<DialogueView>();
                CreditView credit_view = viewManager.GetView<CreditView>();

                m_dialogueManager.LateInit(dialogue_view, credit_view);
            }
        }
        public override void Init(params object[] parameters)
        {
            m_pageManager = gameObject.GetComponent<PageManager>();
            m_characterManager = gameObject.GetComponent<CharacterManager>();
            m_powerManager = gameObject.GetComponent<PowerManager>();
            m_dialogueManager = gameObject.GetComponent<DialogueManager>();
            m_cameras = gameObject.GetComponent<GameCameraRegister>();

            if (m_pageManager != null) m_pageManager.Init(parameters);
            if (m_characterManager != null) m_characterManager.Init(parameters);
            if (m_powerManager != null) m_powerManager.Init(parameters);
            if (m_dialogueManager != null) m_dialogueManager.Init(parameters);
            if (m_cameras != null) m_cameras.Init(parameters);
        }
        #endregion
    }
}