using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;

namespace Comic
{
    public class Npc : Character
    {
        [Header("Others")]
        [SerializeField] private Transform m_lookTarget;
        [SerializeField] private DialogueName m_dialogueType;

        protected override void Awake()
        {
            base.Awake();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            GameObject otherObject = collision.gameObject;

            // /!\ BE AWARE OF THIS BUG /!\ :
            // If the player is spawing on npc, and trigger this dialogue, he might be paused and could not move
            // The dialogue UI may not trigger also because of execution order or whatever
            if (otherObject.GetComponent<Player>() != null)
            {
                ComicGameCore.Instance.MainGameMode.TriggerDialogue(m_dialogueType);
            }
        }

        public override void Init()
        {
            base.Init();

            Player player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();

            m_lookTarget = player.transform;
        }

        public override void Pause(bool pause = true)
        {
            base.Pause(pause);
        }

        protected override void OnUpdate(float elapsed_time)
        {
            base.OnUpdate(elapsed_time);

            if (m_lookTarget == null)
            {
                return;
            }

            Vector2 directionTarget = (Vector2)m_lookTarget.position - m_rb.position;
            SetSprireFaceDirection(directionTarget);
        }
    }
}
