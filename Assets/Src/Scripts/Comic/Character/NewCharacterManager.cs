using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using static Comic.Comics;

namespace Comic
{
    public enum CharacterType
    {
        Character_Jacob,
        Character_BestFriend,
        Character_Bully,
        Character_Beloved,
        Character_Boss
    }

    public class CharacterManager : BaseBehaviour
    {
        // to think about
        [SerializeField] private Transform              m_characterContainer;
        private Dictionary<CharacterType, GameObject>   m_characters;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        { }

        #endregion

        public IEnumerator Load()
        {
            int completedCount = 0;

            for (int i = 0; i < addresses.Count; i++)
            {
                int index = i;
                StartCoroutine(AddressableFactory.Create(
                    Comics.characterPath[index],
                    m_characterContainer,
                    (obj) =>
                    {
                        m_characters.Add(Character_Jacob, obj);
                    }));
            }

            while (completedCount < addresses.Count)
                yield return null;
        }

        private void LoadCharacters()
        {
            Player playerPrefab = Resources.Load<Player>("Player/Player", m_characterContainer);
            Character bestFriend = Resources.Load<Character>("NPC/BestFriend", m_characterContainer);
            Character beloved = Resources.Load<Character>("NPC/Beloved");
            Character bully = Resources.Load<Character>("NPC/Bully");
            Character boss = Resources.Load<Character>("NPC/Boss");
            //Character mom = Resources.Load<Character>("NPC/Mom");

            m_player = Instantiate(playerPrefab);
            m_npcs = new()
            {
                { VoiceType.Voice_BestFriend,    Instantiate(bestFriend)},
                { VoiceType.Voice_Beloved,       Instantiate(beloved)},
                { VoiceType.Voice_Bully,         Instantiate(bully)},
                { VoiceType.Voice_Boss,          Instantiate(boss)},
                //{ VoiceType.Voice_Mom,           Instantiate(mom)},
            };
        }
}
