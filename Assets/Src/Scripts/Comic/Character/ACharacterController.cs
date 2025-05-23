using UnityEngine;
using UnityEngine.InputSystem;
using CustomArchitecture;

namespace Comic
{
    public abstract class ACharacterController : BaseBehaviour
    {
        [Header("Configuration")]
        [SerializeField] protected CharacterConfiguration m_configuration;

        [Header("Components")]
        [SerializeField] protected Rigidbody2D        m_rb;
        [SerializeField] protected Collider2D         m_collider;

        [Header("Ground Check")]
        [SerializeField] protected LayerMask          m_groundLayer;
        [SerializeField] protected float              m_groundCheckDistance = 0.1f;

        [Header("Jump Settings")]
        [SerializeField] protected float              m_coyoteTime = 0.15f;
        [SerializeField] protected float              m_jumpBufferTime = 0.15f;

        private bool m_hasDoubleJumped = false;
        private float m_coyoteTimeCounter;
        private float m_jumpBufferCounter;
        private bool m_isGrounded;

        public Collider2D GetCollider() => m_collider;
        public Rigidbody2D GetRigidbody() => m_rb;
        public bool IsGrounded() => m_isGrounded;
        public bool IsJumpingUp() => !m_isGrounded && m_rb.linearVelocity.y > 0.01f;
        public bool IsFalling() => !m_isGrounded && m_rb.linearVelocity.y < -0.01f;
        public bool IsRunning() => IsGrounded() && Mathf.Abs(m_rb.linearVelocity.x) > 0.01f;

//        protected abstract void PlayJumpAnimation();

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
                else if (m_configuration.allowDoubleJump && !m_hasDoubleJumped)
                {
                    Jump();
                    m_jumpBufferCounter = 0f;
                    m_hasDoubleJumped = true;
                }
            }
        }

        #region Jump
        public void TryJumpInternal()
        {
            m_jumpBufferCounter = m_jumpBufferTime;
        }

        private void Jump()
        {
            Vector2 direction = Vector2.up;
            m_rb.AddForce(m_configuration.jumpSpeed * direction, ForceMode2D.Impulse);
        }
        #endregion Jump

        #region Ground
        private void UpdateGroundedState()
        {
            RaycastHit2D hit = Physics2D.BoxCast(
                m_collider.bounds.center,
                m_collider.bounds.size,
                0f,
                Vector2.down,
                m_groundCheckDistance,
                m_groundLayer
            );

            m_isGrounded = hit.collider != null;
        }
        #endregion Ground
    }
}
