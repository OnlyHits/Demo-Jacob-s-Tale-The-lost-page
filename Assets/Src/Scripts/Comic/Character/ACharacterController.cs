using UnityEngine;
using UnityEngine.InputSystem;
using CustomArchitecture;
using static Comic.Comic;
using TMPro;
using static UnityEngine.Rendering.STP;

namespace Comic
{
    [System.Serializable]
    public class CharacterConfiguration
    {
        [Range(0, 1)]
        [SerializeField] private float factor = 0.5f;

        [Header("Speed")]
        [SerializeField, HideInInspector] private Vector2 speedRange = new(0f, 600f);
        [SerializeField, ReadOnly] private float speed;

        [Header("Jump force")]
        [SerializeField, HideInInspector] private Vector2 jumpForceRange = new(0f, 12f);
        [SerializeField, ReadOnly] private float jumpForce;

        [Header("Double jump")]
        [SerializeField] private bool allowDoubleJump = true;

        public float GetSpeed() => speed;
        public float GetJumpForce() => jumpForce;
        public bool AllowDoubleJump() => allowDoubleJump;

        public void Recalculate()
        {
            float clampedFactor = Mathf.Clamp01(factor);

            speed = Mathf.Lerp(speedRange.y, speedRange.x, clampedFactor);
            jumpForce = Mathf.Lerp(jumpForceRange.y, jumpForceRange.x, clampedFactor);
        }

        public void OnValidate()
        {
            Recalculate();
        }
    }

    public abstract class ACharacterController : BaseBehaviour
    {
        [Header("Configuration")]
        [SerializeField] protected CharacterConfiguration m_configuration;

        [Header("Components")]
        [SerializeField] protected Rigidbody2D        m_rb;
        [SerializeField] protected Collider2D         m_collider;

        [Header("Ground Check")]
        [SerializeField] protected LayerMask          m_groundLayer;
        [SerializeField] protected float              m_groundCheckDistance = 0.01f;

        [Header("Jump Settings")]
        [SerializeField] protected float              m_coyoteTime = 0.15f;
        [SerializeField] protected float              m_jumpBufferTime = 0.15f;

        private bool m_hasDoubleJumped = false;
        private float m_coyoteTimeCounter;
        private float m_jumpBufferCounter;
        private bool m_isGrounded;
        private bool m_wasGrounded;
        private bool m_wasJumping;
        private bool m_wasFalling;
        private bool m_wasIdle;
        private bool m_wasRunning;

        public Collider2D GetCollider() => m_collider;
        public Rigidbody2D GetRigidbody() => m_rb;
        public bool IsGrounded() => m_isGrounded;
        public bool IsJumpingUp() => !m_isGrounded && m_rb.linearVelocity.y > 0.01f;
        public bool IsFalling() => !m_isGrounded && m_rb.linearVelocity.y < -0.01f;
        public bool IsRunning() => IsGrounded() && Mathf.Abs(m_rb.linearVelocity.x) > 0.01f;
        public CharacterConfiguration GetConfiguration() => m_configuration;

        protected abstract void OnJumpStarted();
        protected abstract void OnFallStarted();
        protected abstract void OnGroundedStarted();
        protected abstract void OnIdleStarted();
        protected abstract void OnRunStarted();

        #region BaseBehaviour
        public override void Init(params object[] parameters)
        {
            if (m_rb == null)
            {
                m_rb = GetComponent<Rigidbody2D>();
            }
        }
        protected override void OnUpdate()
        {
            UpdateGroundedState();

            if (m_isGrounded)
            {
                m_coyoteTimeCounter = m_coyoteTime;
                m_hasDoubleJumped = false;
            }
            else
            {
                m_coyoteTimeCounter -= Time.deltaTime;
            }

            if (m_jumpBufferCounter > 0)
                m_jumpBufferCounter -= Time.deltaTime;

            if (m_jumpBufferCounter > 0)
            {
                if (m_coyoteTimeCounter > 0)
                {
                    Jump();
                    m_jumpBufferCounter = 0f;
                    m_coyoteTimeCounter = 0f;
                }
                else if (m_configuration.AllowDoubleJump() && !m_hasDoubleJumped)
                {
                    Jump();
                    m_jumpBufferCounter = 0f;
                    m_hasDoubleJumped = true;
                }
            }
        }

        protected override void OnLateUpdate()
        {
            bool isGroundedNow = m_isGrounded;
            bool isJumpingNow = IsJumpingUp();
            bool isFallingNow = IsFalling();
            bool isIdleNow = isGroundedNow && Mathf.Abs(m_rb.linearVelocity.x) <= 0.01f;
            bool isRunningNow = isGroundedNow && Mathf.Abs(m_rb.linearVelocity.x) > 0.01f;

            if (!m_wasJumping && isJumpingNow)
                OnJumpStarted();
            if (!m_wasFalling && isFallingNow)
                OnFallStarted();
            if (!m_wasIdle && isIdleNow)
                OnIdleStarted();

            // that make logic not accurate
            // nevertheless it fit well with the animation logic
            if (!m_wasRunning && isRunningNow)
                OnRunStarted();
            else if (!m_wasGrounded && isGroundedNow)
                OnGroundedStarted();

            m_wasGrounded = isGroundedNow;
            m_wasJumping = isJumpingNow;
            m_wasFalling = isFallingNow;
            m_wasIdle = isIdleNow;
            m_wasRunning = isRunningNow;
        }
        #endregion BaseBehaviour

        #region Move
        public virtual void StartMove(Vector2 v)
        {
            //Vector2 newVel = new Vector2(0, m_rb.linearVelocity.y);
            //m_rb.linearVelocity = newVel;
        }

        public virtual void Move(Vector2 v)
        {
            Vector2 newVel = v * m_configuration.GetSpeed();
            Vector2 currentVel = new(m_rb.linearVelocity.x, IsJumpingUp() || IsFalling() ? 0 : m_rb.linearVelocity.y);
            Vector2 expectedVel = (newVel - currentVel) * Time.fixedDeltaTime;
            m_rb.linearVelocityX = expectedVel.x;
        }

        public virtual void StopMove(Vector2 v)
        {
            Vector2 newVel = new(0, m_rb.linearVelocity.y);
            m_rb.linearVelocity = newVel;
        }

        #endregion Move

        #region Jump
        public void TryJumpInternal()
        {
            m_jumpBufferCounter = m_jumpBufferTime;
        }

        private void Jump()
        {
            Vector2 direction = Vector2.up;
            m_rb.linearVelocityY = 0f;
            m_rb.AddForce(m_configuration.GetJumpForce() * direction, ForceMode2D.Impulse);
        }
        #endregion Jump

        #region Ground
        private void UpdateGroundedState()
        {
            m_isGrounded = Physics2D.BoxCast(
                m_collider.bounds.center,
                m_collider.bounds.size,
                0f,
                Vector2.down,
                m_groundCheckDistance,
                m_groundLayer
            ).collider != null;
        }
        #endregion Ground

        #region Editor
        private void OnDrawGizmosSelected()
        {
            if (m_collider == null) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(m_collider.bounds.center + Vector3.down * m_groundCheckDistance, m_collider.bounds.size);
        }

        private void OnValidate()
        {
            if (m_configuration != null)
            {
                m_configuration.OnValidate();
            }
        }
        #endregion
    }
}
