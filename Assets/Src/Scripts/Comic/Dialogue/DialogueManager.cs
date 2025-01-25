using System;
using Sirenix.Serialization;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using CustomArchitecture;
using System.Collections;
using ExtensionMethods;

namespace Comic
{
    public class DialogueManager : BaseBehaviour
    {
        [SerializeField] private DialogueView   m_dialogueView;
        private JacobDialogueConfig             m_dialogueConfig;
        private Coroutine                       m_dialogueCoroutine;
        private Action<PowerType>               m_changePowerCallback;

        public void Init()
        {
            ComicGameCore.Instance.GetGameMode<MainGameMode>().SubscribeToUnlockVoice(UnlockVoice);
            ComicGameCore.Instance.GetGameMode<MainGameMode>().SubscribeToLockVoice(LockVoice);

            m_dialogueConfig = SerializedScriptableObject.CreateInstance<JacobDialogueConfig>();
            m_dialogueCoroutine = null;

            InitDialogueView();
        }

        public void OnSwitchPower()
        {
            VoiceType type = m_dialogueView.HighlightNext();

            m_changePowerCallback?.Invoke(ProgressionUtils.GetPowerByVoice(type));
        }

        #region VoiceIcon

        private void InitDialogueView()
        {
            foreach (var data in ComicGameCore.Instance.GetGameMode<MainGameMode>().GetUnlockChaptersData())
            {
                if (data.m_hasUnlockVoice)
                {
                    m_dialogueView.AddVoice(ComicGameCore.Instance.GetGameMode<MainGameMode>().GetGameConfig().GetVoiceByChapter(data.m_chapterType));
                }
            }
        }

        private bool IsNpcAllowedToBeVoice(VoiceType type)
        {
            if (type == VoiceType.Voice_Beloved
                || type == VoiceType.Voice_Bully
                || type == VoiceType.Voice_Boss
                || type == VoiceType.Voice_BestFriend)
                return true;

            return false;
        }

        public void LockVoice(VoiceType type)
        {
            m_dialogueView.RemoveVoice(type);
        }

        public void UnlockVoice(VoiceType type)
        {
            if (!IsNpcAllowedToBeVoice(type))
            {
                Debug.LogWarning(type + " is not register");
                return;
            }

            m_dialogueView.AddVoice(type);
        }

        #endregion

        #region Dialogues

        public override void Pause(bool pause)
        {
            base.Pause(pause);

            Debug.Log("DialogueManager is paused : " + pause.ToString());
        }

        private void Start()
        {
            StartDialogue(DialogueName.DialogueWelcome);
        }

        public void StartDialogue(DialogueName type)
        {
            if (!m_dialogueConfig.GetConfig().ContainsKey(type))
            {
                Debug.LogWarning("Doesnt find dialogue");
                return;
            }

            foreach (var t in m_dialogueConfig.GetConfig()[type])
            {
                // check that view has unlocked the icon & bubble if not main icon
                if (!ProgressionUtils.HasUnlockVoice(t.m_speaker))
                {
                    Debug.LogWarning("You need to unlock " + t.m_speaker.ToString() + " before starting this dialogue");
                    return;
                }
            }

           m_dialogueCoroutine = StartCoroutine(DialogueCoroutine(type));
        }

        public IEnumerator DialogueCoroutine(DialogueName type)
        {
            yield return new WaitForSeconds(2f);

            foreach (var part in m_dialogueConfig.GetConfig()[type])
            {
                if (part.m_isMainDialogue)
                {
                   yield return StartCoroutine(m_dialogueView.TriggerMainDialogue(part));
                }
                else
                {
                   yield return StartCoroutine(m_dialogueView.TriggerVoiceDialogue(part));
                }
            }
        }

        #endregion
    }
}
