using System.Linq;
using CustomArchitecture;
using UnityEngine;
using System.Collections.Generic;
using static CustomArchitecture.CustomArchitecture;
using static Comic.Comic;
using System.Security.Cryptography.X509Certificates;

namespace Comic
{
    public class NewCharacter : ACharacterController
    {
        [Header("Type")]
        [SerializeField] private CharacterType m_type;

        [Header("Facing")]
        [SerializeField, ReadOnly] protected bool m_faceRight = true;

        [Header("Animations")]
        [SerializeField] protected Animator m_animator;

        [Header("Sprites")]
        [SerializeField] protected Transform    m_headPositionGoRight;
        [SerializeField] protected Transform    m_headPositionGoLeft;
        [SerializeField] protected Transform    m_head;
        protected Vector2                       m_baseHeadLocalPos;
        [HideInInspector] protected List<SpriteRenderer> m_sprites;

        [Header("Vfx")]
        [SerializeField] private GameObject         m_footStepParticlePrefab;
        [SerializeField] private Transform          m_footStepParticleContainer;
        private AllocationPool<FootStepParticle>    m_footStepParticlePool;

        private Vector2 m_moveInputStrength;

        // Reference to CharacterManager which contain vfx
        // Todo : make a vfx manager singleton or GameCore dependency
        private NewCharacterManager m_manager;

        // Animations
        private readonly string ANIM_IDLE = "Idle";
        private readonly string ANIM_RUN = "Run";
        private readonly string ANIM_JUMP = "Jump";
        private readonly string ANIM_FALL = "Fall";

        public CharacterType GetCharacterType() => m_type;
        public Animator GetAnimator() => m_animator;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();
        }
        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (m_footStepParticlePool != null)
                m_footStepParticlePool.Update(Time.deltaTime);
        }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            base.Init();

            if (parameters.Count() < 1 || parameters[0] is not NewCharacterManager)
            {
                Debug.Log("Wrong parameters");
                return;
            }

            if (parameters.Count() < 2 || parameters[1] is not PlayerInputsController)
            {
                Debug.LogWarning("Wrong parameters");
                return;
            }

            m_manager = (NewCharacterManager)parameters[0];
            var input_controller = (PlayerInputsController)parameters[1];

            input_controller.onMoveAction += OnMove;
            input_controller.onJumpAction += OnJump;

            m_sprites = new();
            var sprites = GetComponentsInChildren<SpriteRenderer>();
            m_sprites.AddRange(sprites);

            m_baseHeadLocalPos = m_head.localPosition;

            m_footStepParticlePool = new AllocationPool<FootStepParticle>(m_footStepParticlePrefab, m_footStepParticleContainer, 2);
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
        protected override void OnFallStarted()
        {
            PlayFootStepParticle(false);
            PlayAnimation(ANIM_FALL);
        }
        protected override void OnJumpStarted()
        {
            PlayFootStepParticle(false);
            PlayAnimation(ANIM_JUMP);
        }
        protected override void OnGroundedStarted()
        {
            PlayFootStepParticle(false);
            SpawnFootStepVfx(true, true);
            SpawnFootStepVfx(false, true);
            PlayAnimation(ANIM_IDLE);
        }

        protected override void OnIdleStarted()
        {
            PlayFootStepParticle(false);
            PlayAnimation(ANIM_IDLE);
        }
        protected override void OnRunStarted()
        {
            PlayFootStepParticle(true);
            SpawnFootStepVfx(m_faceRight, false);
            PlayAnimation(ANIM_RUN);
        }
        private void PlayAnimation(string animation)
        {
            m_animator.Play(animation);
        }
        private bool IsPlaying(string state)
        {
            return m_animator.GetCurrentAnimatorStateInfo(0).IsName(state);
        }
        #endregion

        #region Move override
        public override void StartMove(Vector2 v)
        {
            base.StartMove(v);

            SetSpriteFaceDirection(v);

            m_moveInputStrength = v;
        }

        public override void Move(Vector2 v)
        {
            base.Move(v);

            SetSpriteFaceDirection(v);

            m_moveInputStrength = v;
        }

        public override void StopMove(Vector2 v)
        {
            base.StopMove(v);

            m_moveInputStrength = v;
        }
        #endregion Move override

        #region Vfx
        private void SpawnFootStepVfx(bool faceRight, bool ignoreSpeed)
        {
            Bounds bounds = m_collider.bounds;

            Vector2 pos = new(bounds.center.x, bounds.min.y);

            m_manager.AllocateFootStep(pos, faceRight, Mathf.Abs(m_moveInputStrength.x), ignoreSpeed);
        }
        private void PlayFootStepParticle(bool play)
        {
            if (play)
            {
                m_footStepParticlePool.AllocateElement();
            }
            else if (!play)
            {
                foreach (var particle in m_footStepParticlePool.GetComputedElements())
                {
                    particle.StopParticleSystem();
                }
            }
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

            m_rb.simulated = !pause;
        }
    }
}
