using System.Linq;
using CustomArchitecture;
using UnityEngine;
using System.Collections.Generic;
using static CustomArchitecture.CustomArchitecture;
using static Comic.Comic;
using System.Security.Cryptography.X509Certificates;

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

    public class NewCharacter : ACharacterController
    {
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

        // Reference to CharacterManager which contain vfx
        // Todo : make a vfx manager singleton or GameCore dependency
        private NewCharacterManager m_manager;

        // Animations
        private readonly string ANIM_IDLE = "Idle";
        private readonly string ANIM_RUN = "Run";
        private readonly string ANIM_JUMP = "Jump";
        private readonly string ANIM_FALL = "Fall";

        public Animator GetAnimator() => m_animator;
        public CharacterConfiguration GetConfiguration() => m_configuration;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (IsJumpingUp() && !IsPlaying(ANIM_JUMP))
            {
                PlayAnimation(ANIM_JUMP);
            }
            else if (IsFalling() && !IsPlaying(ANIM_FALL))
            {
                PlayAnimation(ANIM_FALL);
            }
            else if (IsRunning() && !IsPlaying(ANIM_RUN))
            {
                PlayAnimation(ANIM_RUN);
            }
            else if (!IsPlaying(ANIM_IDLE))
            {
                PlayAnimation(ANIM_IDLE);
            }
        }
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
        // Is it use somewhere? Remove if not
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
                TryJumpInternal();
            }
        }
        #endregion

        #region Animation

        private void PlayAnimation(string animation)
        {
            m_animator.Play(animation);
        }
        private bool IsPlaying(string state)
        {
            return m_animator.GetCurrentAnimatorStateInfo(0).IsName(state);
        }
        //private void PlayRun(bool play = true)
        //{
        //    m_animator.SetBool(ANIM_RUN, play);
        //}

        //protected override void PlayJumpAnimation()
        //{
        //    m_animator.SetTrigger(ANIM_JUMP);
        //}

        //private void PlayFall(bool play = true)
        //{
        //    m_animator.SetBool(ANIM_FALL, play);
        //}
        #endregion

        #region Physics
        public virtual void StartMove(Vector2 v)
        {
            Vector2 newVel = new Vector2(0, m_rb.linearVelocity.y);
            m_rb.linearVelocity = newVel;

            SetSpriteFaceDirection(v);

            SpawnFootStepVfx(v);
        }

        public virtual void Move(Vector2 v)
        {
            if (!IsGrounded())
            {
                return;
            }

            SetSpriteFaceDirection(v);
            Vector2 newVel = v * m_configuration.speed;
            Vector2 currentVel = new Vector2(m_rb.linearVelocity.x, IsJumpingUp() || IsFalling() ? 0 : m_rb.linearVelocity.y);
            Vector2 expectedVel = (newVel - currentVel) * Time.fixedDeltaTime;
            m_rb.linearVelocityX = expectedVel.x;
        }

        private void SpawnFootStepVfx(Vector2 v)
        {
            Bounds bounds = m_collider.bounds;

            Vector2 pos = new(bounds.center.x, bounds.min.y);

            m_manager.AllocateFootStep(pos, m_faceRight, Mathf.Abs(v.x));
        }

        public virtual void StopMove(Vector2 v)
        {
            //PlayRun(false);

            Vector2 newVel = new(0, m_rb.linearVelocity.y);
            m_rb.linearVelocity = newVel;
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
