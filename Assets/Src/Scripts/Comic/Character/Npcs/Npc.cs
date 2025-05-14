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

            if (m_lookTarget == null)
            {
                return;
            }

            Vector2 directionTarget = (Vector2)m_lookTarget.position - m_rb.position;
            SetSprireFaceDirection(directionTarget);
        }
        public override void LateInit(params object[] parameters)
        {
            base.LateInit(parameters);
        }
        public override void Init(params object[] parameters)
        {
            base.Init(parameters);

            // should be done in lateinit

            NewCharacter player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter();

            m_lookTarget = player.transform;
        }
        #endregion



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

        public override void Pause(bool pause = true)
        {
            base.Pause(pause);
        }
    }
}
