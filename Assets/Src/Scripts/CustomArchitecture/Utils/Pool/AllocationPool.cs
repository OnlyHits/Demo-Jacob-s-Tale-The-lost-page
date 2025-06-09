using System;
using System.Collections.Generic;
using Comic;
using System.Linq;
using UnityEngine;

namespace CustomArchitecture
{
    // TODO : sort by z, or by callback
    // Come back after month, sort order is kinda bullshit
    // too precise and made for only one purpose
    public enum SortOrderMethod
    {
        // if enable, user need to call manually SortElements(false)
        // callback is set in constructor
        Sort_Custom,
        // use SetSiblingIndex by order of arrival
        Sort_Hierarchy,
        // Sort order is defined by instantiation order
        Sort_None
    }

    /// <summary>
    /// Pool that instantiate MonoBehaviour objects. Manage the life cycle of the object.
    /// This pool allocate if needed. Adjust initial pool size in constructor.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AllocationPool<T> where T : APoolElement
    {
        private Queue<T>            m_objectPool = new();
        private GameObject          m_prefab;
        private Transform           m_parentTransform;
        private List<T>             m_currentObjects = new();
        private SortOrderMethod     m_sortMethod;
        private Action<List<T>>     m_customSort;
        private Action<T>           m_initCallback;
        private int m_initialPoosSize;
        public bool IsCompute() => m_currentObjects.Count > 0;
        public List<T> GetComputedElements() => m_currentObjects;

        public AllocationPool(GameObject prefab, Transform parentTransform, int initialPoolSize = 10, SortOrderMethod sort_method = SortOrderMethod.Sort_None, Action<List<T>> callback = null, Action<T> on_init_callback = null)
        {
            m_sortMethod = sort_method;
            m_customSort += callback;
            m_initCallback += on_init_callback;
            m_prefab = prefab;
            m_parentTransform = parentTransform;
            m_initialPoosSize = initialPoolSize;

            for (int i = 0; i < m_initialPoosSize; i++)
            {
                GameObject obj = UnityEngine.Object.Instantiate(m_prefab, m_parentTransform);
                obj.SetActive(false);

                if (!obj.TryGetComponent<T>(out var component))
                {
                    UnityEngine.Object.Destroy(obj);
                    break;
                }

                m_objectPool.Enqueue(component);
                m_initCallback?.Invoke(component);
            }
        }
        public void Update(float elapsed_time)
        {
            for (int i = 0; i < m_currentObjects.Count; ++i)
            {
                if (!m_currentObjects[i].Compute)
                {
                    DeallocateElement(m_currentObjects[i]);

                    m_currentObjects.RemoveAt(i);
                }
            }
        }

        public T AllocateElement(params object[] parameters)
        {
            if (m_objectPool.Count > 0)
            {
                T obj = m_objectPool.Dequeue();
                m_currentObjects.Add(obj);
                obj.gameObject.SetActive(true);
                obj.OnAllocate(parameters);

                SortElements(true);

                return obj;
            }
            else
            {
                GameObject obj = UnityEngine.Object.Instantiate(m_prefab, m_parentTransform);

                if (!obj.TryGetComponent<T>(out var component))
                {
                    UnityEngine.Object.Destroy(obj);
                    return null;
                }

                m_currentObjects.Add(obj.GetComponent<T>());
                m_initCallback?.Invoke(component);

                obj.SetActive(true);
                component.OnAllocate(parameters);

                SortElements(true);

                return component;
            }
        }

        private void DeallocateElement(T obj)
        {
            obj.OnDeallocate();
            obj.gameObject.SetActive(false);
            m_objectPool.Enqueue(obj);
        }

        private void SortBySiblingIndex()
        {
            for (int i = 0, index = m_currentObjects.Count - 1; i < m_currentObjects.Count; ++i, --index)
            {
                m_currentObjects[i].transform.SetSiblingIndex(index);
            }
        }

        public void SortElements(bool on_allocate = true)
        {
            switch (m_sortMethod)
            {
                case SortOrderMethod.Sort_Custom:
                    if (!on_allocate)
                        m_customSort?.Invoke(m_currentObjects);
                    break;
                case SortOrderMethod.Sort_Hierarchy:
                    SortBySiblingIndex();
                    break;
                case SortOrderMethod.Sort_None:
                    break;
            }
        }
    }
}