using UnityEngine;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using static PageHole;

namespace Comic
{
    public class NavigableView : AView
    {
        [Header("Debug")]
        public bool m_debug;

        [Header("Navigable View")]
        [SerializeField] protected List<PanelData> m_panelsData = new List<PanelData>();

        [Header("Colors")]
        [SerializeField] private Color m_baseColorElement = Color.black;
        [SerializeField] private Color m_selectedColor = Color.red;

        [Header("Cooldowns")]
        [SerializeField, ReadOnly] protected bool isCd = false;
        [SerializeField] protected float cd = 0.5f;
        protected float timer = 0.1f;

        [Header("Panels Datas")]
        [SerializeField, ReadOnly] protected PanelData m_currentPanelData;
        [SerializeField, ReadOnly] protected int m_currentElementIdx = 0;
        [SerializeField, ReadOnly] protected int m_lastPanelElementIdx = 0;
        [SerializeField, ReadOnly] protected UIBehaviour m_currentElement;

        [Space]
        [SerializeField, ReadOnly] protected int m_basePanelIndex = 0;
        [SerializeField, ReadOnly] protected int m_currentPanelIndex = -1;
        [SerializeField, ReadOnly] protected int m_lastPanelIndex = -1;


        [Serializable]
        public class PanelData
        {
            public int panelIndex;
            public GameObject panelObj;

            public UIBehaviour startElement;
            public List<UIBehaviour> selectableElements;

            public void SetStartingElement(UIBehaviour element)
            {
                startElement = element;
            }
        }

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        {
            if (isCd)
            {
                timer += Time.deltaTime;
                if (timer >= cd)
                {
                    isCd = false;
                    timer = 0f;
                }
            }
        }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        { }
        #endregion

        #region INTERNAL

        public override void ActiveGraphic(bool active)
        {
            var startElement = m_panelsData[m_basePanelIndex].startElement;
            var selectableElements = m_panelsData[m_basePanelIndex].selectableElements;

            m_lastPanelElementIdx = selectableElements.IndexOf(startElement);
            ShowPanelByIndex(m_basePanelIndex);
        }

        #endregion INTERNAL 


        #region PANELS

        protected virtual void ShowPanelByIndex(int panelIndex)
        {
            m_lastPanelIndex = m_currentPanelIndex;
            m_currentPanelIndex = panelIndex;

            foreach (PanelData data in m_panelsData)
            {
                if (data.panelIndex == panelIndex)
                {
                    m_currentPanelData = data;
                    data.panelObj.SetActive(true);
                }
                else
                {
                    data.panelObj.SetActive(false);
                }
            }
            TrySetStartingElement(out m_currentElement);
        }

        protected void ShowBasePanel()
        {
            ShowPanelByIndex(m_basePanelIndex);
        }

        protected void ShowPrevPanel()
        {
            if (m_lastPanelIndex <= -1)
            {
                return;
            }
            ShowPanelByIndex(m_lastPanelIndex);
        }

        #endregion PANELS


        #region UI ELEMENTS

        protected bool TrySetStartingElement(out UIBehaviour element)
        {
            UIBehaviour startElement = m_currentPanelData.startElement;

            if (startElement == null)
            {
                element = startElement;
                Debug.LogWarning("Starting element of Paneldata index : " + m_currentPanelData.panelIndex.ToString() + ", is null .");
                return false;
            }

            if (m_currentPanelIndex == m_basePanelIndex)
            {
                // @note: set last base panel element index on swithing to it
                startElement = m_currentPanelData.selectableElements[m_lastPanelElementIdx];
            }
            else
            {
                // @note: save base panel element index on swithing to another panel
                m_lastPanelElementIdx = m_currentElementIdx;
            }

            if (TrySetElement(out element, startElement))
            {
                SelectElement(element, m_currentPanelData.selectableElements);
                return true;
            }
            Debug.LogWarning("Starting element [" + m_currentPanelData.startElement.name + "] of Navigation panels is not in panel elements list.");
            return false;
        }
        protected bool TrySetElementByIndex(out UIBehaviour destElement, int newIndex)
        {
            UIBehaviour newElement = m_currentPanelData.selectableElements.ElementAt(newIndex);

            if (newElement == null)
            {
                destElement = m_currentElement;
                Debug.LogWarning("Failed to select element at index " + newIndex.ToString() + ".");
                return false;
            }
            if (TrySetElement(out destElement, newElement))
            {
                SelectElement(destElement, m_currentPanelData.selectableElements);
                return true;
            }
            Debug.LogWarning("Failed to select element by index");
            return false;
        }
        protected bool TrySetElement(out UIBehaviour destElement, UIBehaviour element)
        {
            List<UIBehaviour> elements = m_currentPanelData.selectableElements;

            m_currentElementIdx = elements.IndexOf(element);
            if (m_currentElementIdx < 0)
            {
                m_currentElementIdx = 0;
                destElement = m_currentElement;
                return false;
            }
            destElement = elements[m_currentElementIdx];
            return true;
        }

        protected void SelectElement(UIBehaviour selectedElement, List<UIBehaviour> others)
        {
            foreach (UIBehaviour element in others)
            {
                Color destColor = default;

                if (element == selectedElement)
                    destColor = m_selectedColor;
                else
                    destColor = m_baseColorElement;

                if (element is Button button)
                {
                    button.transform.GetComponentInChildren<TextMeshProUGUI>().color = destColor;
                    button.image.color = destColor;
                }
                else if (element is Slider slider)
                {
                    slider.handleRect.GetComponentInChildren<Image>().color = destColor;
                    slider.fillRect.GetComponentInChildren<Image>().color = destColor;
                }
                else if (element is TextMeshProUGUI textMesh)
                {
                    textMesh.color = destColor;
                }
                else if (element is Image image)
                {
                    image.color = destColor;
                }
            }
        }

        #endregion
    }
}
