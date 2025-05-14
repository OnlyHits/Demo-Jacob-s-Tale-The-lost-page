using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using System.Collections;
using static Comic.Comic;

namespace Comic
{
    public class NewCharacterManager : BaseBehaviour
    {
        // to think about
        [SerializeField] private Transform              m_characterContainer;
        private Dictionary<CharacterType, GameObject>   m_characters;
        private NewCharacter                            m_currentCharacter;
        // all characters are going to use this input controller
        private PlayerInputsController                  m_inputsController;

        public NewCharacter GetCurrentCharacter() => m_currentCharacter;
        public PlayerInputsController GetInputController() => m_inputsController;

        static int i = 0;
        private void TestSwitch()
        {
            ++i;
            if (i > 4)
                i = 0;
            
            SwitchCharacter((CharacterType)i);
        }

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                TestSwitch();
            }
        }
        public override void LateInit(params object[] parameters)
        {
            if (m_inputsController != null)
                m_inputsController.LateInit();

            foreach (var character in m_characters.Values)
                character.GetComponent<NewCharacter>().LateInit();

            SwitchCharacter(CharacterType.Character_Jacob);
        }

        public override void Init(params object[] parameters)
        {
            if (!ComponentUtils.GetOrCreateComponent<PlayerInputsController>(gameObject, out m_inputsController))
                Debug.LogWarning("Unable to get or create PanelInput");
            else
                m_inputsController.Init(ComicGameCore.Instance.GetInputAsset());

            // init characters after input controller
            foreach (var character in m_characters.Values)
                character.GetComponent<NewCharacter>().Init(m_inputsController);
        }

        #endregion

        public void SwitchCharacter(CharacterType type)
        {
            if (!m_characters.ContainsKey(type))
            {
                return;
            }

            if (m_currentCharacter != null)
                m_currentCharacter.gameObject.SetActive(false);
    
            m_currentCharacter = m_characters[type].GetComponent<NewCharacter>();
            m_currentCharacter.gameObject.SetActive(true);
        }

        public IEnumerator Load()
        {
            m_characters = new();
            int completedCount = 0;

            for (int i = 0; i < characterPath.Count; i++)
            {
                int index = i;
                StartCoroutine(AddressableFactory.CreateAsync(
                    characterPath[index],
                    m_characterContainer,
                    (obj) =>
                    {
                        m_characters.Add(obj.GetComponent<NewCharacter>().GetConfiguration().type, obj);
                        obj.SetActive(false);
                        ++completedCount;
                    }));
            }

            while (completedCount < characterPath.Count)
                yield return null;
        }
        
        public void PauseAllCharacters(bool pause = true)
        {
            foreach (var character in m_characters.Values)
            {
                character.GetComponent<NewCharacter>().Pause(pause);
            }
        }

    }
}