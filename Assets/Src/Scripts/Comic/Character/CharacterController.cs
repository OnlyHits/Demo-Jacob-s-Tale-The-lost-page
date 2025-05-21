using UnityEngine;
using UnityEngine.InputSystem;
using CustomArchitecture;

namespace Comic
{
    public abstract class CharacterController : BaseBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private CharacterConfiguration m_configuration;

        [Header("Components")]
        [SerializeField] private Rigidbody2D        m_rb;
        [SerializeField] private BoxCollider2D      m_collider;

        [Header("Ground Check")]
        [SerializeField] private LayerMask          m_groundLayer;
        [SerializeField] private float              m_groundCheckDistance = 0.1f;

        [Header("Jump Settings")]
        [SerializeField] private float              m_coyoteTime = 0.15f;
        [SerializeField] private float              m_jumpBufferTime = 0.15f;

        private bool m_hasDoubleJumped = false;
        private float m_coyoteTimeCounter;
        private float m_jumpBufferCounter;
        private bool m_isGrounded;
        public bool IsGrounded() => m_isGrounded;
        public bool IsJumpingUp() => !m_isGrounded && m_rb.velocity.y > 0.01f;
        public bool IsFalling() => !m_isGrounded && m_rb.velocity.y < -0.01f;
        public bool IsMoving() => Mathf.Abs(m_rb.velocity.x) > 0.01f;


        private void Update()
        {
            UpdateGroundedState();

            // Update coyote time
            if (isGrounded)
            {
                coyoteTimeCounter = coyoteTime;
                hasDoubleJumped = false; // Reset double jump when grounded
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // Update jump buffer
            if (jumpBufferCounter > 0)
                jumpBufferCounter -= Time.deltaTime;

            // Try to jump
            if (jumpBufferCounter > 0)
            {
                if (coyoteTimeCounter > 0)
                {
                    Jump();
                    jumpBufferCounter = 0f;
                    coyoteTimeCounter = 0f;
                }
                else if (allowDoubleJump && !hasDoubleJumped)
                {
                    Jump();
                    jumpBufferCounter = 0f;
                    hasDoubleJumped = true;
                }
            }
        }

        private void Jump()
        {
            Vector2 velocity = rb.velocity;
            velocity.y = jumpForce;
            rb.velocity = velocity;
        }

        private void UpdateGroundedState()
        {
            RaycastHit2D hit = Physics2D.BoxCast(
                boxCollider.bounds.center,
                boxCollider.bounds.size,
                0f,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );

            isGrounded = hit.collider != null;
        }

        // Input System: jump button
        public void OnJump(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                jumpBufferCounter = jumpBufferTime;
            }
        }

        // Optional horizontal input
        public void OnMove(InputAction.CallbackContext context)
        {
            moveInput = context.ReadValue<Vector2>();
        }

    }
}
