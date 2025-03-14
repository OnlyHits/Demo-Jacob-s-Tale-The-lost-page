using CustomArchitecture;
using UnityEngine;
using System.Collections;
using static CustomArchitecture.CustomArchitecture;

namespace Comic
{
    public class BubbleChoice : Bubble
    {
        [SerializeField] private TMP_AnimatedText m_choiceOneDialogue;
        [SerializeField] private TMP_AnimatedText m_choiceTwoDialogue;
        [SerializeField] private GameObject m_cursorOne;
        [SerializeField] private GameObject m_cursorTwo;
        protected override bool IsBubbleChoice() => true;

        private bool m_accept = true;
        private bool m_validate = false;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }
        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }

        public override void LateInit(params object[] parameters)
        {
            base.LateInit(parameters);
        }
        public override void Init(params object[] parameters)
        {
            if (parameters.Length != 1
                || parameters[0] is not RectTransform)
                return;

            base.Init(parameters[0]);

            ComicGameCore.Instance.MainGameMode
                .GetNavigationManager().GetNavigationInput().SubscribeToCancel(OnCancel);
            ComicGameCore.Instance.MainGameMode
                .GetNavigationManager().GetNavigationInput().SubscribeToValidate(OnValidation);
            ComicGameCore.Instance.MainGameMode
                .GetNavigationManager().GetNavigationInput().SubscribeToNavigate(OnNavigate);

            m_cursorOne.SetActive(true);
            m_cursorTwo.SetActive(false);
        }

        public void SetupChoiceOne(DialogueType type)
        {
            DialogueConfig config = TMP_AnimatedTextController.Instance.GetDialogueConfig<ComicGameCore>(type);
            DynamicDialogueData datas = TMP_AnimatedTextController.Instance.GetDialogueDatas<ComicGameCore>(type);

            m_choiceOneDialogue.StartDialogue(config, datas);

            m_cursorOne.SetActive(true);
            m_cursorTwo.SetActive(false);

            m_validate = false;
            m_accept = true;
        }
        #endregion

        public void SetupChoiceTwo(DialogueType type)
        {
            DialogueConfig config = TMP_AnimatedTextController.Instance.GetDialogueConfig<ComicGameCore>(type);
            DynamicDialogueData datas = TMP_AnimatedTextController.Instance.GetDialogueDatas<ComicGameCore>(type);

            m_choiceTwoDialogue.StartDialogue(config, datas);
            m_accept = true;
        }

        protected override IEnumerator WaitForInput()
        {
            yield return new WaitUntil(() => m_validate);
        }

        private void OnNavigate(InputType input, Vector2 v)
        {
            if (input == InputType.PRESSED)
            {
                m_accept = (v.x < 0f);
                m_cursorOne.SetActive(m_accept);
                m_cursorTwo.SetActive(!m_accept);
            }
            else if (input == InputType.COMPUTED)
            {
                m_accept = (v.x < 0f);
                m_cursorOne.SetActive(m_accept);
                m_cursorTwo.SetActive(!m_accept);
            }
            else if (input == InputType.RELEASED)
            {
            }
        }

        private void OnValidation(InputType input, bool b)
        {
            if (input == InputType.PRESSED)
            {
                m_validate = true;
            }
            else if (input == InputType.COMPUTED)
            {
                m_validate = true;
            }
            else if (input == InputType.RELEASED)
            {
                m_validate = false;
            }
        }

        private void OnCancel(InputType input, bool b)
        {
            if (input == InputType.PRESSED)
            {
            }
            else if (input == InputType.COMPUTED)
            {
            }
            else if (input == InputType.RELEASED)
            {
            }
        }
    }
}