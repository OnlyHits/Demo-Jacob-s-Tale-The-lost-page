using UnityEngine;

namespace Comic
{
    public partial class Player : Character
    {
        [Header("Inputs")]
        [SerializeField] private PlayerInputsController m_inputsController;

        [Header("Sprite")]
        [SerializeField] private SpriteRenderer m_spriteHead;
        [SerializeField] private SpriteRenderer m_spriteBody;

        [Space]
        [SerializeField] private Color m_baseColor = Color.white;
        [SerializeField] private Color m_shadowColor = Color.black;

        [Header("Grounded")]
        [SerializeField, ReadOnly] private bool m_isGrounded = false;

        [Header("Move")]
        [SerializeField, ReadOnly] private bool m_isMoving = false;
        [SerializeField] private float m_speed = 10f;

        [Header("Jump")]
        [SerializeField, ReadOnly] private bool m_isJumping = false;
        [SerializeField] private float m_jumpForce = 10f;

        [Header("Fall")]
        [SerializeField, ReadOnly] private bool m_isFalling = false;

        public PlayerInputsController GetInputController() => m_inputsController;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        {
            base.OnFixedUpdate();

            m_isGrounded = IsGrounded();

            /*if (!m_isGrounded && !m_isFalling)
            {
                TryFall();
            }
            else if (m_isFalling && m_isGrounded)
            {
                StopFall();
            }*/
        }
        protected override void OnLateUpdate()
        {
            base.OnLateUpdate();

        }
        protected override void OnUpdate()
        {
            base.OnUpdate();
        }
        public override void LateInit(params object[] parameters)
        {
            base.LateInit(parameters);

            m_inputsController.LateInit();
        }
        public override void Init(params object[] parameters)
        {
            base.Init();

            m_inputsController.Init();

            m_inputsController.onMoveAction += OnMove;
            m_inputsController.onLookAction += OnLook;
            m_inputsController.onJumpAction += OnJump;
            m_inputsController.onSprintAction += OnSprint;
            m_inputsController.onInteractAction += OnInteract;
            m_inputsController.onPowerAction += OnPower;
            m_inputsController.onNextPowerAction += OnNextPower;
            m_inputsController.onPrevPowerAction += OnPrevPower;

            ComicGameCore.Instance.MainGameMode.SubscribeToPowerSelected(OnPowerSelected);
        }
        #endregion

        // input controller pause is handle in NavigationController
        public override void Pause(bool pause = true)
        {
            base.Pause(pause);
        }

        // private void OnCollisionEnter2D(Collision2D collision)
        // {
        //     if (collision.gameObject.layer == LayerMask.NameToLayer(caseColliderLayerName))
        //     {
        //         Debug.Log(">>> IN");
        //         m_isInWall = true;
        //     }
        // }

        // private void OnCollisionExit2D(Collision2D collision)
        // {
        //     if (collision.gameObject.layer == LayerMask.NameToLayer(caseColliderLayerName))
        //     {
        //         Debug.Log(">>> OUT");
        //         m_isInWall = false;
        //     }
        // }
        /*
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer(caseColliderLayerName))
            {
                Debug.Log(">>> IN");
                m_isInWall = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.gameObject.layer == LayerMask.NameToLayer(caseColliderLayerName))
            {
                Debug.Log(">>> OUT");
                m_isInWall = false;
            }
        }
        */

        #region VISUAL

        public void EnableVisual(bool enable)
        {
            m_spriteHead.enabled = enable;
            m_spriteBody.enabled = enable;
        }

        public void EnableShadowVisual(bool enable, bool fade = false)
        {
            if (enable)
            {
                m_spriteHead.color = m_shadowColor;
                m_spriteBody.color = m_shadowColor;
            }
            else
            {
                m_spriteHead.color = m_baseColor;
                m_spriteBody.color = m_baseColor;
            }
        }

        #endregion VISUAL

        #region GROUNDED

        private bool IsGrounded()
        {
            return true;
        }

        #endregion GROUNDED

        #region MOVE
        private void StartMove(Vector2 v)
        {
            PlayRun(true);
            m_isMoving = true;

            Vector2 newVel = new Vector2(0, m_rb.linearVelocity.y);
            m_rb.linearVelocity = newVel;
        }

        private void Move(Vector2 v)
        {
            SetSprireFaceDirection(v);

            if (!m_isGrounded)
            {
                return;
            }
            Vector2 newVel = v * m_speed;
            Vector2 currentVel = new Vector2(m_rb.linearVelocity.x, m_isJumping ? 0 : m_rb.linearVelocity.y);
            Vector2 expectedVel = (newVel - currentVel) * Time.fixedDeltaTime;
            m_rb.linearVelocityX = expectedVel.x;
            //m_rb.AddForce(expectedVel, ForceMode2D.Force);
        }
        private void StopMove(Vector2 v)
        {
            PlayRun(false);
            m_isMoving = false;

            Vector2 newVel = new Vector2(0, m_rb.linearVelocity.y);
            m_rb.linearVelocity = newVel;
        }
        #endregion MOVE

        #region JUMP
        private void TryJump()
        {
            if (!m_isGrounded)
            {
                return;
            }
            PlayJump(true);
            m_isJumping = true;

            Vector2 direction = Vector2.up;
            //Vector2 newVel = m_jumpForce * direction;
            //Vector2 currentVel = new Vector2(0, m_rb.linearVelocity.y);
            //Vector2 expectedVel = (newVel - currentVel) * Time.fixedDeltaTime;
            //m_rb.linearVelocityY = expectedVel.y;
            m_rb.AddForce(m_jumpForce * direction, ForceMode2D.Impulse);
        }
        #endregion JUMP

        #region FALL
        private void TryFall()
        {
            if (m_isGrounded || m_isJumping || m_isFalling)
            {
                return;
            }
            m_isFalling = true;
            PlayFall(true);
        }

        private void StopFall()
        {
            if (!m_isGrounded && m_isFalling)
            {
                return;
            }
            m_isFalling = false;
            PlayFall(false);
        }
        #endregion FALL

    }
}