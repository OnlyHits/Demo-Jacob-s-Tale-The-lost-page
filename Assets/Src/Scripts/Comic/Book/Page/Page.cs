using UnityEngine;
using CustomArchitecture;
using System.Collections.Generic;
using System.Linq;
using static CustomArchitecture.CustomArchitecture;
using Comc;

namespace Comic
{
    [System.Serializable]
    public struct PageConfiguration
    {
        public List<Vector3> m_panelPositions;
    }

    public class Page : AutomaticNavigationSystem<Panel>
    {
        // Page content
        [ReadOnly, SerializeField] protected List<PageConfiguration>    m_pageConfigurations;
        [SerializeField] private SpriteRenderer                         m_pageSprite;
        [SerializeField] private Transform                              m_panelContainer;
        [SerializeField] private GameObject                             m_panelPrefab;
        private Dictionary<PropsType, List<AProps>>                     m_props;
        private PanelShuffleSequence                                    m_shuffleSequence;
        private int                                                     m_configurationIndex;

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
                var next_config = GetNextPageConfiguration();
                if (next_config != null)
                {
                    var panels = m_navigables.Select(n => n.transform).ToList();
                    m_shuffleSequence.Shuffle(panels, next_config.Value, m_pageSprite.bounds.center);
                }
            }
        }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            foreach (var panel in m_navigables)
                panel.LateInit();
        }
        public override void Init(params object[] parameters)
        {
            base.Init();

            ComicGameCore.Instance.MainGameMode.GetNavigationManager()
                .GetPanelInput().SubscribeToNavigate(OnNavigate);
            ComicGameCore.Instance.MainGameMode.GetNavigationManager()
                .GetPanelInput().SubscribeToInteract(OnInteract);

            m_shuffleSequence = GetComponent<PanelShuffleSequence>();
            m_shuffleSequence.Init(gameObject.transform);

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

            m_onChangeFocus += OnFocusPanel;
        }
        #endregion

        #region Page configuration
        public PageConfiguration? GetCurrentPageConfiguration()
        {
            if (m_pageConfigurations == null || m_configurationIndex >= m_pageConfigurations.Count)
            {
                Debug.LogWarning("Page configuration doesn't exist");
                return null;
            }

            return m_pageConfigurations[m_configurationIndex];
        }

        // kinda weird cause it set index.
        // Maybe split set index from get configuration logic
        public PageConfiguration? GetNextPageConfiguration()
        {
            if (m_pageConfigurations == null || m_pageConfigurations.Count() == 0)
            {
                Debug.LogWarning("Page configuration doesn't exist");
                return null;
            }

            m_configurationIndex = m_configurationIndex + 1 >= m_pageConfigurations.Count() ? 0 : m_configurationIndex + 1;
            return m_pageConfigurations[m_configurationIndex];
        }

        public PageConfiguration? GetPageConfigurationAt(int index)
        {
            if (m_pageConfigurations == null || index >= m_pageConfigurations.Count())
            {
                Debug.LogWarning("Page configuration doesn't exist");
                return null;
            }

            return m_pageConfigurations[index];

        }
        #endregion Page configuration

        private void OnFocusPanel(Panel panel)
        {
            StartCoroutine(ComicCinemachineMgr.Instance.FocusCamera(
                panel.GetCinemachineCamera().Camera,
                ComicCinemachineMgr.CameraList.Managed_Cameras,
                false,
                ComicCinemachineMgr.Instance.SmoothBlend));
        }

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

        public bool IsPlayerInFocusedPanel()
        {
            var character = ComicGameCore.Instance.MainGameMode.GetCharacterManager().GetCurrentCharacter();

            if (character == null || !m_isRunning || m_focusedNavigable == null)
                return false;

            return m_focusedNavigable.ContainPosition(character.transform.position);
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

        public List<Bounds> GetPanelsInnerBound()
        {
            if (m_navigables == null || m_navigables.Count == 0)
                return null;

            List<Bounds> bounds = new();

            foreach (var panel in m_navigables)
                bounds.Add(panel.GetInnerPanelBounds());

            return bounds;
        }

        public Panel GetCurrentPanel()
        {
            foreach (Panel panel in m_navigables)
            {
                if (panel.IsPlayerInPanel())
                {
                    return panel;
                }
            }

            Debug.Log("Player is in no case from the page " + gameObject.name);
            return null;
        }

        #region SPAWN POINT

        public Transform TryGetSpawnPoint()
        {
            return m_spawnPoint;
        }

        #endregion SPAWN POINT

        #region PageEdition
#if UNITY_EDITOR
        public void AddConfiguration()
        {
            var config = new PageConfiguration();
            config.m_panelPositions = new List<Vector3>();

            foreach (var panel in m_navigables)
            {
                config.m_panelPositions.Add(panel.transform.position);
            }

            m_pageConfigurations.Add(config);
        }
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
                }
            }

            var nav_list = m_navigables.Cast<Navigable>().ToList();

            foreach (var panel in m_navigables)
                panel.Init(nav_list, m_margin);
        }

        public void InstantiatePanel()
        {
            GameObject panel_object = Instantiate(m_panelPrefab, m_panelContainer);
            Panel panel = panel_object.GetComponent<Panel>();

            panel.Init(m_navigables, m_margin);
            m_navigables.Add(panel);
        }
#endif
        #endregion
    }
}