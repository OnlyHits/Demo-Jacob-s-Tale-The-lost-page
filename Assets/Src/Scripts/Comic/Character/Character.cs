using System.Collections.Generic;
using CustomArchitecture;
using UnityEngine;
using static PageHole;

namespace Comic
{
    public partial class Character : BaseBehaviour
    {
        [Header("Facing")]
        [SerializeField, ReadOnly] protected bool m_faceRight = true;

        [Header("Animations")]
        [SerializeField] protected Animator m_animator;

        [Space]
        [SerializeField] protected Transform m_headPositionGoRight;
        [SerializeField] protected Transform m_headPositionGoLeft;
        [SerializeField] protected Transform m_head;
        protected Vector2 m_baseHeadLocalPos;

        [Header("Others")]
        [SerializeField] protected Rigidbody2D m_rb;
        [SerializeField] protected Collider2D m_collider;
        [HideInInspector] protected List<SpriteRenderer> m_sprites = new List<SpriteRenderer>();

        public Collider2D GetCollider() => m_collider;

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
        {
            // Get spawn pos & set pos
            //ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();
        }
        #endregion


        protected virtual void Awake()
        {
            m_baseHeadLocalPos = m_head.localPosition;

            var sprites = GetComponentsInChildren<SpriteRenderer>();
            m_sprites.AddRange(sprites);
        }

        public override void Pause(bool pause = true)
        {
            base.Pause(pause);
            m_animator.speed = pause ? 0 : 1f;

            m_rb.constraints = pause ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;

            //m_rb.simulated = !pause;
            //m_collider.enabled = !pause;
        }

        #region SPRITES
        protected void SetSprireFaceDirection(Vector2 direction)
        {
            bool wasFacingRight = m_faceRight;

            if (direction.x > 0)
            {
                m_faceRight = true;
            }
            else if (direction.x < 0)
            {
                m_faceRight = false;
            }

            if (wasFacingRight == m_faceRight)
            {
                return;
            }

            foreach (var sprite in m_sprites)
            {
                sprite.flipX = !m_faceRight;
                Transform parentHead = m_faceRight ? m_headPositionGoRight : m_headPositionGoLeft;
                m_head.parent = parentHead;
                m_head.localPosition = m_baseHeadLocalPos;
            }
        }
        #endregion SPRITES
    }
}
