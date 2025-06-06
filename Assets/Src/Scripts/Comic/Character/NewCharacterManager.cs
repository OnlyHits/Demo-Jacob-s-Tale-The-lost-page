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

        // these are going to be in a vfx manager later accesible via Singleton probably
        [Header("Vfx prefab")] public GameObject        m_footStepPrefab;
        [Header("Vfx container")] public Transform      m_footStepContainer;

        public AllocationPool<FootStepVfx>              m_footStepVfx;

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

            if (m_footStepVfx != null)
                m_footStepVfx.Update(Time.deltaTime);
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
            m_footStepVfx = new AllocationPool<FootStepVfx>(m_footStepPrefab, m_footStepContainer);

            if (!ComponentUtils.GetOrCreateComponent<PlayerInputsController>(gameObject, out m_inputsController))
                Debug.LogWarning("Unable to get or create PanelInput");
            else
                m_inputsController.Init(ComicGameCore.Instance.GetInputAsset());

            // init characters after input controller
            foreach (var character in m_characters.Values)
                character.GetComponent<NewCharacter>().Init(this, m_inputsController);
        }
        #endregion

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
                        m_characters.Add(obj.GetComponent<NewCharacter>().GetCharacterType(), obj);
                        obj.SetActive(false);
                        ++completedCount;
                    }));
            }

            while (completedCount < characterPath.Count)
                yield return null;
        }

        #region Vfx
        public void AllocateFootStep(Vector3 position, bool flip_x, float speed, bool ignoreSpeed = false)
        {
            if (m_footStepVfx == null)
            {
                return;
            }

            m_footStepVfx.AllocateElement(position, flip_x, speed, ignoreSpeed);
        }
        #endregion Vfx

        #region Manager behaviour
        public void SwitchCharacter(CharacterType type)
        {
            if (!m_characters.ContainsKey(type))
            {
                return;
            }

            Vector2 velocity = Vector2.zero;
            float angular_velocity = 0f;
            Vector3 position = Vector3.zero;

            if (m_currentCharacter != null)
            {
                velocity = m_currentCharacter.GetRigidbody().linearVelocity;
                angular_velocity = m_currentCharacter.GetRigidbody().angularVelocity;
                position = m_currentCharacter.transform.position;
                m_currentCharacter.gameObject.SetActive(false);
            }
            else
                position = m_characterContainer.transform.position; // hack to control the first instantiation. need to remake the spawn position logic

            position.z = PLAYER_BASE_Z;
            m_currentCharacter = m_characters[type].GetComponent<NewCharacter>();
            m_currentCharacter.GetRigidbody().linearVelocity = velocity;
            m_currentCharacter.GetRigidbody().angularVelocity = angular_velocity;
            m_currentCharacter.transform.position = position;

            m_currentCharacter.gameObject.SetActive(true);
        }

        public void PauseAllCharacters(bool pause = true)
        {
            foreach (var character in m_characters.Values)
            {
                character.GetComponent<NewCharacter>().Pause(pause);
            }
        }
        #endregion Manager behaviour
    }
}