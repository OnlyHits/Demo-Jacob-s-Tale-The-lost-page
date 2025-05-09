using UnityEngine;
using CustomArchitecture;
using System.Collections.Generic;
using System.Linq;
using static CustomArchitecture.CustomArchitecture;
namespace Comic
{
    [ExecuteAlways]
    public class Page : AutomaticNavigationSystem<Panel>
    {
        // Page content
        [SerializeField] private SpriteRenderer     m_pageSprite;
        [SerializeField] private Transform          m_panelContainer;
        [SerializeField] private GameObject         m_panelPrefab;
        private Dictionary<PropsType, List<AProps>> m_props;
        private PanelShuffleSequence                m_shuffleSequence;

        // Debug visual
        [SerializeField] private SpriteRenderer m_margin;

        // Game logic
        [SerializeField] private Transform m_spawnPoint;

        public SpriteRenderer GetPageSpriteRenderer() => m_pageSprite;

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.K))
            {
                var panels = m_navigables.Select(n => n.transform).ToList();
                m_shuffleSequence?.Shuffle(panels, m_pageSprite.bounds.center);
            }
        }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            ComicGameCore.Instance.MainGameMode.GetNavigationManager()
                .GetPanelInput().SubscribeToNavigate(OnNavigate);
            ComicGameCore.Instance.MainGameMode.GetNavigationManager()
                .GetPanelInput().SubscribeToInteract(OnInteract);

            m_shuffleSequence = new PanelShuffleSequence();

            var nav_list = m_navigables.Cast<Navigable>().ToList();

            foreach (var panel in m_navigables)
                panel.Init(nav_list, m_margin);

            m_props = new()
            {
                { PropsType.Props_Lamp, new List<AProps>() },
                { PropsType.Props_Painting, new List<AProps>() },
                { PropsType.Props_Candle, new List<AProps>() },
            };

            foreach (var panel in m_navigables)
            {
                foreach (var props in panel.GetProps())
                {
                    if (m_props.ContainsKey(props.GetPropsType()))
                    {
                        m_props[props.GetPropsType()].Add(props);
                    }
                }
            }
        }
        #endregion

        private void OnInteract(InputType input, bool b)
        {
            if (!IsRunning())
                return;

            m_focusedNavigable.Flip(Direction.Up);
        }

        public bool CanAccessPanel(Vector3 position)
        {
            foreach (var panel in m_navigables)
            {
                if (panel.ContainPosition(position))
                {
                    if (panel.IsLock())
                        return false;
                }
            }

            return true;
        }

        public override void Pause(bool pause = true)
        {
            base.Pause(pause);

            foreach (var p in m_props)
            {
                foreach (var props in p.Value)
                    props.Pause(pause);
            }
        }

        public void EnablePage()
        {
            gameObject.SetActive(true);

            foreach (var p in m_props)
            {
                foreach (var props in p.Value)
                {
                    props.StartBehaviour();
                }
            }
        }

        public void DisablePage()
        {
            foreach (var p in m_props)
            {
                foreach (var props in p.Value)
                    props.StopBehaviour();
            }

            gameObject.SetActive(false);
        }

        public void Enable(bool enable)
        {
            gameObject.SetActive(enable);
        }

        public List<SpriteRenderer> GetPanelsSpriteRenderer()
        {
            if (m_navigables == null || m_navigables.Count == 0)
                return null;

            //if (m_currentPanels == null || m_currentPanels.Count == 0)
            //        return null;

            List<SpriteRenderer> sprites = new();

            foreach (var panel in m_navigables)
                sprites.Add(panel.GetPanelVisual().PanelReference());

            return sprites;
        }

        #region SPAWN POINT

        public Transform TryGetSpawnPoint()
        {
            return m_spawnPoint;
        }

        #endregion SPAWN POINT

        public Panel GetCurrentPanel()
        {
            foreach (Panel panel in m_navigables)
            {
                if (panel.IsPlayerInCase())
                {
                    return panel;
                }
            }

            Debug.Log("Player is in no case from the page " + gameObject.name);
            return null;
        }

        #region PageEdition

#if UNITY_EDITOR

        // TODO : generalize and made static in utils
        public void RefreshList()
        {
            m_navigables.Clear();

            foreach (Transform child in m_panelContainer)
            {
                Panel component = child.GetComponent<Panel>();
                if (component != null)
                {
                    m_navigables.Add(component);
                    component.Init(m_margin);
                }
            }
        }

        public void InstantiatePanel()
        {
            GameObject panel_object = Instantiate(m_panelPrefab, m_panelContainer);
            Panel panel = panel_object.GetComponent<Panel>();

            panel.Init(m_margin);
            m_navigables.Add(panel);
        }
#endif

        #endregion
    }
}