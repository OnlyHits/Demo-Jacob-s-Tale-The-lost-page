using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;

namespace Comic
{
    public class CharacterManager : BaseBehaviour
    {
        [SerializeField, ReadOnly] private Player m_player;
        private Dictionary<VoiceType, Character> m_npcs;
        public Player GetPlayer() => m_player;

        public void Init()
        {
            LoadCharacters();
            InitCharacters();

            ComicGameCore.Instance.MainGameMode.SubscribeToBeforeSwitchPage(OnBeforeSwitchPage);
            ComicGameCore.Instance.MainGameMode.SubscribeToAfterSwitchPage(OnAfterSwitchPage);
            ComicGameCore.Instance.MainGameMode.SubscribeToUnlockVoice(OnUnlockVoice);
            ComicGameCore.Instance.MainGameMode.SubscribeToLockVoice(OnLockVoice);
            ComicGameCore.Instance.MainGameMode.SubscribeToUnlockChapter(OnUnlockChapter);

            SpawnCharacters();
        }

        private void Start()
        {
        }

        private void SpawnCharacters()
        {
            foreach (var data in ComicGameCore.Instance.MainGameMode.GetUnlockChaptersData())
            {
                // Spawn NPCS
                TrySpawnNPCsByChapter(data.m_chapterType);

                // Disable NPC if already got in UI
                if (data.m_hasUnlockVoice)
                {
                    VoiceType voiceType = ComicGameCore.Instance.MainGameMode.GetGameConfig().GetVoiceByChapter(data.m_chapterType);
                    EnableNPC(voiceType, false);
                }
            }
        }

        private void LoadCharacters()
        {
            Player playerPrefab = Resources.Load<Player>("Player/Player");
            Character bestFriend = Resources.Load<Character>("NPC/BestFriend");
            Character beloved = Resources.Load<Character>("NPC/Beloved");
            Character bully = Resources.Load<Character>("NPC/Bully");
            Character boss = Resources.Load<Character>("NPC/Boss");
            Character mom = Resources.Load<Character>("NPC/Mom");

            m_player = Instantiate(playerPrefab);
            m_npcs = new()
            {
                { VoiceType.Voice_BestFriend,    Instantiate(bestFriend)},
                { VoiceType.Voice_Beloved,       Instantiate(beloved)},
                { VoiceType.Voice_Bully,         Instantiate(bully)},
                { VoiceType.Voice_Boss,          Instantiate(boss)},
                { VoiceType.Voice_Mom,           Instantiate(mom)},
            };
        }

        private void InitCharacters()
        {
            m_player.Init();
            foreach (Character npc in m_npcs.Values)
            {
                npc.Init();
            }
        }

        #region SPAWN NPCs

        private void TrySpawnNPCsByChapter(Chapters chapter)
        {
            var gameConfig = ComicGameCore.Instance.MainGameMode.GetGameConfig();
            Dictionary<VoiceType, int> npcsSpawnPages = gameConfig.GetNpcsSpawnPageByChapter(chapter);

            if (npcsSpawnPages == null)
            {
                return;
            }

            if (npcsSpawnPages.Count <= 0)
            {
                return;
            }

            foreach (VoiceType npcVoice in npcsSpawnPages.Keys)
            {
                if (!m_npcs.ContainsKey(npcVoice))
                {
                    return;
                }
                int idxPageToSpawn = npcsSpawnPages[npcVoice];

                Transform spawn = ComicGameCore.Instance.MainGameMode.GetPageManager().GetSpawnPointByPageIndex(idxPageToSpawn);

                if (spawn == null)
                {
                    Debug.LogWarning("No spawn pos for page " + idxPageToSpawn.ToString() + ", npc " + npcVoice.ToString() + " cannot spawn properly");
                    return;
                }

                m_npcs[npcVoice].transform.position = spawn.position;
                m_npcs[npcVoice].transform.parent = spawn;
                EnableNPC(npcVoice, true);
            }
        }

        #endregion SPAWN NPCs


        #region CALLBACK RECEPTION

        private void OnUnlockVoice(VoiceType voiceType)
        {
            if (!m_npcs.ContainsKey(voiceType))
            {
                Debug.LogWarning(voiceType + " is not register");
                return;
            }

            EnableNPC(voiceType, false);
        }

        public void OnLockVoice(VoiceType voiceType)
        {
            if (!m_npcs.ContainsKey(voiceType))
            {
                Debug.LogWarning(voiceType + " is not register");
                return;
            }

            EnableNPC(voiceType, true);
        }

        private void OnUnlockChapter(Chapters chapter)
        {
            TrySpawnNPCsByChapter(chapter);
        }

        private void EnableNPC(VoiceType voiceType, bool enable)
        {
            if (!m_npcs.ContainsKey(voiceType))
            {
                Debug.LogWarning("Try to enable a npc of type " + voiceType.ToString() + " which is not in npc list from CharacterManager");
                return;
            }
            m_npcs[voiceType].gameObject.SetActive(enable);
        }

        #endregion CALLBACK RECEPTION


        #region SWITCH PAGE

        private void OnBeforeSwitchPage(bool nextPage, Page p1, Page p2)
        {
            PauseAllCharacters(true);
        }

        private void OnAfterSwitchPage(bool nextPage, Page p1, Page p2)
        {
            //PauseAllCharacters(false);
        }

        public void PauseAllCharacters(bool pause = true)
        {
            m_player.Pause(pause);
            foreach (Npc npc in m_npcs.Values)
            {
                npc.Pause(pause);
            }
        }

        #endregion SWITCH PAGE
    }
}
