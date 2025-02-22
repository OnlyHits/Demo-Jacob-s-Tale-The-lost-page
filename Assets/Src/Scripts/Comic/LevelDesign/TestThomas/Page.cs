using UnityEngine;
using CustomArchitecture;
using System.Collections.Generic;
using UnityEditor.Rendering.Universal;
using Sirenix.OdinInspector.Editor.Validation;
//using UnityEditor.Rendering.Universal;

namespace Comic
{
    [ExecuteAlways]
    public class Page : BaseBehaviour
    {
        [SerializeField] private Transform m_panelContainer;
        [SerializeField] private SpriteRenderer m_margin;
        [SerializeField] private GameObject m_panelPrefab;
        [SerializeField] private List<Panel> m_currentPanels;
        [SerializeField] private Transform m_spawnPoint;
        private Dictionary<PropsType, List<AProps>> m_props;

        private void Awake()
        {
            if (m_margin != null)
            {
                foreach (var panel in m_currentPanels)
                {
                    panel.Init(m_margin);
                }
            }
        }

        public void Init()
        {
            foreach (var panel in m_currentPanels)
                panel.Init(m_margin);

            m_props = new()
            {
                { PropsType.Props_Lamp, new List<AProps>() },
                { PropsType.Props_Painting, new List<AProps>() },
                { PropsType.Props_Candle, new List<AProps>() },
            };

            foreach (var panel in m_currentPanels)
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

        public bool CanAccessPanel(Vector3 position)
        {
            foreach (var panel in m_currentPanels)
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
            if (m_currentPanels == null || m_currentPanels.Count == 0)
                return null;

            List<SpriteRenderer> sprites = new();

            foreach (var panel in m_currentPanels)
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
            foreach (Panel panel in m_currentPanels)
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
            m_currentPanels.Clear();

            foreach (Transform child in m_panelContainer)
            {
                Panel component = child.GetComponent<Panel>();
                if (component != null)
                {
                    m_currentPanels.Add(component);
                    component.Init(m_margin);
                }
            }
        }

        public void InstantiatePanel()
        {
            GameObject panel_object = Instantiate(m_panelPrefab, m_panelContainer);
            Panel panel = panel_object.GetComponent<Panel>();

            panel.Init(m_margin);
            m_currentPanels.Add(panel);
        }
#endif

        #endregion
    }
}