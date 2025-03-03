using System.Collections.Generic;
using UnityEngine;
using CustomArchitecture;
using static PageHole;

namespace Comic
{
    public class ViewManager : BaseBehaviour
    {
        [SerializeField] private AView m_startingView;
        [SerializeField] private AView[] m_views;
        private AView m_currentView;
        private readonly Stack<AView> m_history = new Stack<AView>();

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        {
            for (int i = 0; i < m_views.Length; ++i)
            {
                m_views[i].LateInit();
            }

            //// remove this and replace by ShowStartingView function
            //if (m_startingView != null)
            //    Show(m_startingView, true);
        }
        public override void Init(params object[] parameters)
        {
            for (int i = 0; i < m_views.Length; ++i)
            {
                // set manager first, view could need this reference
                m_views[i].Manager = this;
                m_views[i].Init();
                m_views[i].Hide();
            }
        }
        #endregion

        public AView GetCurrentView() => m_currentView;

        public T GetView<T>() where T : AView
        {
            for (int i = 0; i < m_views.Length; i++)
            {
                if (m_views[i] is T tView)
                {
                    return tView;
                }
            }
            return null;
        }

        public void Show<T>(bool remember = false) where T : AView
        {
            for (int i = 0; i < m_views.Length; i++)
            {
                if (m_views[i] is T)
                {
                    if (m_currentView != null)
                    {
                        if (remember)
                        {
                            m_history.Push(m_currentView);
                        }
                        m_currentView.Hide();
                    }
                    m_views[i].Show();
                    // added
                    m_views[i].ActiveGraphic(true);
                    m_currentView = m_views[i];
                }
            }

            HideAndShowPartialViews();
        }

        public void Show(AView view, bool remember = false)
        {
            if (m_currentView != null)
            {
                if (remember)
                {
                    m_history.Push(m_currentView);
                }
                m_currentView.Hide();
            }
            view.Show();
            // added
            view.ActiveGraphic(true);
            m_currentView = view;

            HideAndShowPartialViews();
        }

        public void HideAndShowPartialViews()
        {
            // Quand on show une view, il faut showpartial une autre view
            // showPartial : Show(instant: true), Hide(partial: true, istant: true)

            foreach (AView ite_view in m_views)
            {
                if (ite_view == m_currentView) continue;
                if (m_currentView is PauseView) ite_view.Hide(false, true);
                if (m_currentView is BookCoverView) ite_view.Hide(false, true);
                if (m_currentView is ProgressionView)
                {
                    if (ite_view is DialogueView)
                    {
                        ite_view.ShowPartial();
                    }
                }
                if (m_currentView is not PauseView && m_currentView is not ProgressionView
                    && m_currentView is not BookCoverView)
                {
                    ite_view.Hide(true, false);
                }
            }
        }

        public void ShowLast()
        {
            if (m_history.Count != 0)
            {
                Show(m_history.Pop(), false);
            }
        }

        public override void Pause(bool pause)
        {
            base.Pause(pause);

            if (m_currentView != null)
                m_currentView.Pause(pause);
        }
    }
}