using CustomArchitecture;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using TMPro;
using static PageHole;

namespace Comic
{
    public class CreditView : AView
    {
        [SerializeField] protected RectTransform m_bubbleAnchor;
        [SerializeField] protected Transform m_bubbleContainer;
        [SerializeField] protected Transform m_mainIconContainer;
        
        private GameObject          m_bubble;
        private NpcIcon             m_mainIcon;

        private Dictionary<NpcIconType, Sprite> m_iconSprites;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            Bubble bubble = m_bubble.GetComponent<Bubble>();

            if (bubble != null)
                bubble.LateInit(parameters);

            if (m_mainIcon != null)
                m_mainIcon.LateInit(parameters);
        }
        public override void Init(params object[] parameters)
        {
            // will be replace by texture library in gamecore

            m_iconSprites = new()
            {
                { NpcIconType.Icon_Beloved, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-Beloved-1") },
                { NpcIconType.Icon_BestFriend, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-BestFriend-1") },
                { NpcIconType.Icon_Boss_1, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-Boss-1") },
                { NpcIconType.Icon_Boss_2, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-Boss-2") },
                { NpcIconType.Icon_Bully, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-Bully-1") },
                { NpcIconType.Icon_Jacob_0, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-Jacob-0") },
                { NpcIconType.Icon_Jacob_1, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-Jacob-1") },
                { NpcIconType.Icon_Jacob_2, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-Jacob-2") },
                { NpcIconType.Icon_Jacob_3, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-Jacob-3") },
                { NpcIconType.Icon_Jacob_4, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-Jacob-4") },
                { NpcIconType.Icon_Mom, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-Mom") },
                { NpcIconType.Icon_The_Lost_Page, Resources.Load<Sprite>("GUI/Icon/Sprites/Face-LostPage") },
            };

            m_bubble = InstantiatePrefab("GUI/Bubble/Bubble_Speech_Regular", m_bubbleContainer);
            GameObject main_icon = InstantiatePrefab("GUI/Icon/IconFrame_Speaker", m_mainIconContainer);

            if (m_bubble != null)
                InitBubble();

            if (main_icon != null)
                InitMainIcon(main_icon.GetComponent<NpcIcon>());

            if (main_icon != null && m_bubble != null)
            {
                m_bubble.GetComponent<Bubble>().SubscribeToAppearCallback(AppearIcon);
                m_bubble.GetComponent<Bubble>().SubscribeToDisappearCallback(DisappearIcon);
            }
        }
        #endregion

        private void InitBubble()
        {
            m_bubble.SetActive(false);

            m_bubble.GetComponent<Bubble>().Init(m_bubbleContainer.GetComponent<RectTransform>());
        }

        private void InitMainIcon(NpcIcon main_icon)
        {
            m_mainIcon = main_icon;

            RectTransform container_rect = m_bubbleContainer.GetComponent<RectTransform>();
            m_mainIcon.Init(VoiceType.Voice_None, m_iconSprites[NpcIconType.Icon_Jacob_0]);

            m_mainIcon.SetBubbleAnchor(m_bubbleAnchor);

            m_mainIcon.gameObject.SetActive(false);
        }

        private GameObject InstantiatePrefab(string prefab_path, Transform parent = null)
        {
            if (parent != null)
                return Instantiate(Resources.Load<GameObject>(prefab_path), parent);
            else
                return Instantiate(Resources.Load<GameObject>(prefab_path));
        }

        public override void ActiveGraphic(bool active)
        {
            Image[] images = gameObject.GetComponentsInChildren<Image>(true);
            TMP_Text[] texts = gameObject.GetComponentsInChildren<TMP_Text>(true);

            foreach (Image image in images)
            {
                image.enabled = active;
            }

            foreach (TMP_Text text in texts)
            {
                text.enabled = active;
            }
        }

        public void AppearIcon(float intensity)
        {
            m_mainIcon.gameObject.SetActive(true);
            m_mainIcon.Appear(intensity);
        }

        public void DisappearIcon(float intensity)
        {
            m_mainIcon.Disappear(intensity);
        }

        public override void Pause(bool pause)
        {
            // m_mainIcon.Pause(pause);
            // m_bubble.GetComponent<Bubble>().Pause(pause);
        }

        private IEnumerator SetupAndStartDialogue(PartOfDialogueConfig config, bool target_main)
        {
            m_bubble.GetComponent<Bubble>().SetupDialogue(config.m_associatedDialogue);

            m_bubble.GetComponent<Bubble>().SetTarget(m_mainIcon);
 
            yield return StartCoroutine(m_bubble.GetComponent<Bubble>().DialogueCoroutine(config.m_intensity, false, target_main));
        }

        public IEnumerator TriggerMainDialogue(PartOfDialogueConfig config)
        {
            m_mainIcon.SetIconSprite(m_iconSprites[config.m_iconType]);

            m_bubble.SetActive(true);

            yield return StartCoroutine(SetupAndStartDialogue(config, true));
        }
    }
}
