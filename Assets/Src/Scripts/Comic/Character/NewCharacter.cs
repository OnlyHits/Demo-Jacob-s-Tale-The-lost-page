using System.Linq;
using CustomArchitecture;
using UnityEngine;
using System.Collections.Generic;
using static CustomArchitecture.CustomArchitecture;
using static Comic.Comic;

namespace Comic
{
    [System.Serializable]
    public struct CharacterConfiguration
    {
        public float speed;
        public float jumpSpeed;
        public bool allowDoubleJump;
        public CharacterType type;
    }

    public class NewCharacter : BaseBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CharacterConfiguration m_configuration;

        [Header("Facing")]
        [SerializeField, ReadOnly] protected bool m_faceRight = true;

        [Header("Animations")]
        [SerializeField] protected Animator m_animator;

        [Header("Sprites")]
        [SerializeField] protected Transform m_headPositionGoRight;
        [SerializeField] protected Transform m_headPositionGoLeft;
        [SerializeField] protected Transform m_head;
        protected Vector2 m_baseHeadLocalPos;
        [HideInInspector] protected List<SpriteRenderer> m_sprites;

        [Header("Others")]
        [SerializeField] protected Rigidbody2D m_rb;
        [SerializeField] protected Collider2D m_collider;

        // Reference to CharacterManager which contain vfx
        // Todo : make a vfx manager singleton or GameCore dependency
        private NewCharacterManager m_manager;

        // States
        private bool m_isMoving = false;
        private bool m_isJumping = false;
        private bool m_isGrounded = true;

        // Animations
        private readonly string ANIM_IDLE = "Idle";
        private readonly string ANIM_RUN = "Run";
        private readonly string ANIM_JUMP = "Jump";
        private readonly string ANIM_FALL = "Fall";

        public Collider2D GetCollider() => m_collider;
        public Rigidbody2D GetRigidbody() => m_rb;
        public Animator GetAnimator() => m_animator;
        public CharacterConfiguration GetConfiguration() => m_configuration;
        public bool IsGrounded() => m_isGrounded;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        {
            m_isGrounded = CheckGround();
        }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            if (parameters.Count() < 1 || parameters[0] is not NewCharacterManager)
            {
                Debug.Log("Wrong parameters");
            }

            if (parameters.Count() < 2 || parameters[1] is not PlayerInputsController)
            {
                Debug.LogWarning("Wrong parameters");
                return;
            }

            m_manager = (NewCharacterManager)parameters[0];
            var input_controller = (PlayerInputsController)parameters[1];

            input_controller.onMoveAction += OnMove;
            input_controller.onLookAction += OnLook;
            input_controller.onJumpAction += OnJump;

            m_sprites = new();
            var sprites = GetComponentsInChildren<SpriteRenderer>();
            m_sprites.AddRange(sprites);

            m_baseHeadLocalPos = m_head.localPosition;
        }
        #endregion

        #region Input callbacks
        private void OnMove(InputType input, Vector2 v)
        {
            if (!gameObject.activeSelf)
                return;

            if (input == InputType.PRESSED)
            {
                StartMove(v);
            }
            else if (input == InputType.COMPUTED)
            {
                Move(v);
            }
            else if (input == InputType.RELEASED)
            {
                StopMove(v);
            }
        }
        private void OnLook(InputType input, Vector2 v)
        {
            if (!gameObject.activeSelf)
                return;

            if (input == InputType.PRESSED)
            {
            }
        }
        private void OnJump(InputType input, bool b)
        {
            if (!gameObject.activeSelf)
                return;

            if (input == InputType.PRESSED)
            {
                TryJump();
            }
        }
        #endregion

        #region Animation
        private void PlayRun(bool play = true)
        {
            m_animator.SetBool(ANIM_RUN, play);
        }

        private void PlayJump(bool play = true)
        {
            m_animator.SetTrigger(ANIM_JUMP);
        }

        private void PlayFall(bool play = true)
        {
            m_animator.SetBool(ANIM_FALL, play);
        }

        private void TryResetIdle()
        {
            m_animator.SetTrigger(ANIM_IDLE);
        }
        #endregion

        #region Physics
        public bool CheckGround() // cost efficient way to check ground
        {
            RaycastHit2D hit = Physics2D.BoxCast(m_collider.bounds.center, m_collider.bounds.size, 0f, Vector2.down, 0.1f, LayerMask.GetMask("Ground"));
            return hit.collider != null;
        }
    
        public virtual void StartMove(Vector2 v)
        {
            //            PlayRun(true);
            m_isMoving = true;

            Vector2 newVel = new Vector2(0, m_rb.linearVelocity.y);
            m_rb.linearVelocity = newVel;

            SetSpriteFaceDirection(v);

            SpawnFootStepVfx();
        }

        public virtual void Move(Vector2 v)
        {
            if (!m_isGrounded)
            {
                return;
            }

            SetSpriteFaceDirection(v);
            Vector2 newVel = v * m_configuration.speed;
            Vector2 currentVel = new Vector2(m_rb.linearVelocity.x, m_isJumping ? 0 : m_rb.linearVelocity.y);
            Vector2 expectedVel = (newVel - currentVel) * Time.fixedDeltaTime;
            m_rb.linearVelocityX = expectedVel.x;
        }

        private void SpawnFootStepVfx()
        {
            Bounds bounds = m_collider.bounds;

            Vector2 pos = new Vector2(bounds.center.x, bounds.min.y);

            m_manager.AllocateFootStep(pos, m_faceRight, Mathf.Abs(v.x));
        }

        public virtual void StopMove(Vector2 v)
        {
            PlayRun(false);
            m_isMoving = false;

            Vector2 newVel = new Vector2(0, m_rb.linearVelocity.y);
            m_rb.linearVelocity = newVel;
        }
        public virtual void TryJump()
        {
            if (!m_isGrounded)
            {
                return;
            }

            PlayJump(true);
            m_isJumping = true;

            Vector2 direction = Vector2.up;
            m_rb.AddForce(m_configuration.jumpSpeed * direction, ForceMode2D.Impulse);

        }
        #endregion

        #region Sprites
        protected void SetSpriteFaceDirection(Vector2 direction)
        {
            bool wasFacingRight = m_faceRight;

            m_faceRight = direction.x > 0;

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
        #endregion

        public override void Pause(bool pause = true)
        {
            base.Pause(pause);

            m_animator.speed = pause ? 0 : 1f;

            m_rb.constraints = pause ? RigidbodyConstraints2D.FreezeAll : RigidbodyConstraints2D.FreezeRotation;

            //m_rb.simulated = !pause;
            //m_collider.enabled = !pause;
        }
    }
}
