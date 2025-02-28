using UnityEngine;
using static Comic.Comic;
using CustomArchitecture;
using static PageHole;

namespace Comic
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class BoxBully : BaseBehaviour
    {
        [SerializeField] private EventOnCollide2D m_eventOnCol;
        [SerializeField] private SpriteRenderer m_outlineSprite;
        [SerializeField] private Color m_colorInteractible = Color.white;
        [SerializeField] private Color m_colorPushed = Color.yellow;
        [SerializeField, ReadOnly] private Rigidbody2D m_rigidbody;
        [SerializeField, ReadOnly] private Collider2D m_collider;
        [SerializeField, ReadOnly] private bool m_isMovable = false;
        private bool m_isPlayerTriggerZone = false;
        private Player m_playerInstance = null;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        {
            if (!m_isMovable)
            {
                return;
            }

            m_playerInstance = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();

            if (!m_playerInstance.IsPushingBox())
            {
                m_rigidbody.linearVelocity = Vector2.zero;
                m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
                m_outlineSprite.color = m_colorInteractible;
                return;
            }

            Rigidbody2D playerRb = m_playerInstance.GetComponent<Rigidbody2D>();

            m_rigidbody.linearVelocity = playerRb.linearVelocity;

            m_rigidbody.bodyType = RigidbodyType2D.Dynamic;
            m_outlineSprite.color = m_colorPushed;
        }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        { }
        #endregion

        // could make some problem, should be replace by init
        private void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody2D>();
            m_collider = GetComponent<Collider2D>();

            m_eventOnCol.onTriggerEnter += OnZoneTriggerEnter;
            m_eventOnCol.onTriggerExit += OnZoneTriggerExit;

            m_rigidbody.linearVelocity = Vector2.zero;
            m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
            m_outlineSprite.color = m_colorInteractible;
            m_outlineSprite.enabled = false;
            m_isMovable = false;
        }

        // could make some problem, should be replace by lateinit and register on gamemode
        private void Start()
        {
            m_playerInstance = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();

            ComicGameCore.Instance.MainGameMode.SubscribeToPowerSelected(OnPowerSelected);
        }

        private void EnableCubeInteraction(bool enable)
        {
            if (!enable)
            {
                m_rigidbody.linearVelocity = Vector2.zero;
                m_rigidbody.bodyType = RigidbodyType2D.Kinematic;
                m_outlineSprite.color = m_colorInteractible;
                m_outlineSprite.enabled = false;
                m_isMovable = false;
            }
            else
            {
                m_outlineSprite.color = m_colorInteractible;
                m_outlineSprite.enabled = true;
                m_isMovable = true;
            }
        }

        private void OnPowerSelected(PowerType newPowerType)
        {
            if (newPowerType != PowerType.Power_Telekinesis)
            {
                EnableCubeInteraction(false);
            }

            if (!m_isPlayerTriggerZone)
            {
                return;
            }

            if (newPowerType == PowerType.Power_Telekinesis)
            {
                EnableCubeInteraction(true);
            }

            /*Player player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();

            if (!player.CanPushBoxes())
            {
                EnableCubeInteraction(false);
                return;
            }

            EnableCubeInteraction(true);
            */
        }

        #region TRIGGER

        private void OnZoneTriggerEnter(Collider2D collider)
        {
            if (collider.gameObject.layer != LayerMask.NameToLayer(playerLayerName))
            {
                return;
            }

            m_isPlayerTriggerZone = true;

            Player player = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetPlayer();

            if (!player.CanPushBoxes())
            {
                EnableCubeInteraction(false);
                return;
            }

            EnableCubeInteraction(true);
        }

        private void OnZoneTriggerExit(Collider2D collider2D)
        {
            m_isPlayerTriggerZone = false;

            EnableCubeInteraction(false);
        }

        #endregion TRIGGER
    }
}
