using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CustomArchitecture
{
    public abstract class Navigable : BaseBehaviour
    {
        private Dictionary<NavigationDirection, Navigable>  m_links = null;
        [SerializeField] private bool                       m_isNavigable = false;

        // override this function to specify the bounds of your
        // navigable object (Have to be in world space).
        // This bound determine which navigable object
        // is going to be linked to this object
        public abstract Bounds GetGlobalBounds();
        public abstract void Focus();
        public abstract void Unfocus();

        public bool isNavigable() => m_isNavigable;
        public Navigable GetLinkedNavigable(NavigationDirection direction) => m_links?[direction];

        #region BaseBehaviour
        protected override void OnFixedUpdate()
        { }
        protected override void OnLateUpdate()
        { }
        protected override void OnUpdate()
        { }
        public override void LateInit(params object[] parameters)
        { }
        public override void Init(params object[] parameters)
        {
            if (parameters.Count() != 1
                || parameters[0] is not List<Navigable>)
            {
                Debug.LogWarning("Bad parameters");
                return;
            }

            m_links = new()
            {
                { NavigationDirection.Left, null },
                { NavigationDirection.Right, null },
                { NavigationDirection.Up, null },
                { NavigationDirection.Down, null }
            };

            SetLinks((List<Navigable>)parameters[0]);
        }
        #endregion

        // Todo : make a generalization of this method
        // First thought is that there is no ideal way to generalize
        private void SetLinks(List<Navigable> navigables)
        {
            Bounds self_bounds = GetGlobalBounds();
            Vector3 self_center = self_bounds.center;

            Dictionary<NavigationDirection, (Navigable navigable, float distance)> closest_navigables = new()
            {
                { NavigationDirection.Left,  (null, float.MaxValue) },
                { NavigationDirection.Right, (null, float.MaxValue) },
                { NavigationDirection.Up,    (null, float.MaxValue) },
                { NavigationDirection.Down,  (null, float.MaxValue) }
            };

            Dictionary<NavigationDirection, (Navigable navigable, float distance)> furthest_navigables = new()
            {
                { NavigationDirection.Left,  (null, float.MinValue) },
                { NavigationDirection.Right, (null, float.MinValue) },
                { NavigationDirection.Up,    (null, float.MinValue) },
                { NavigationDirection.Down,  (null, float.MinValue) }
            };

            foreach (var nav in navigables)
            {
                // Skip if this object or the target is not navigable, or if the target is the same object
                if (!nav.isNavigable() || nav == this)
                    continue;

                Bounds target_bounds = nav.GetGlobalBounds();
                Vector3 target_center = target_bounds.center;

                float delta_x = target_center.x - self_center.x;
                float delta_y = target_center.y - self_center.y;

                // Check Left (target is to the left and vertically overlaps)
                if (delta_x < 0 && OverlapsVertically(self_bounds, target_bounds))
                {
                    float absDeltaX = Mathf.Abs(delta_x);
                    if (absDeltaX < closest_navigables[NavigationDirection.Left].distance)
                    {
                        closest_navigables[NavigationDirection.Left] = (nav, absDeltaX);
                    }
                    if (absDeltaX > furthest_navigables[NavigationDirection.Left].distance)
                    {
                        furthest_navigables[NavigationDirection.Left] = (nav, absDeltaX);
                    }
                }

                // Check Right (target is to the right and vertically overlaps)
                if (delta_x > 0 && OverlapsVertically(self_bounds, target_bounds))
                {
                    float absDeltaX = Mathf.Abs(delta_x);
                    if (absDeltaX < closest_navigables[NavigationDirection.Right].distance)
                    {
                        closest_navigables[NavigationDirection.Right] = (nav, absDeltaX);
                    }
                    if (absDeltaX > furthest_navigables[NavigationDirection.Right].distance)
                    {
                        furthest_navigables[NavigationDirection.Right] = (nav, absDeltaX);
                    }
                }

                // Check Up (target is above and horizontally overlaps)
                if (delta_y > 0 && OverlapsHorizontally(self_bounds, target_bounds))
                {
                    float absDeltaY = Mathf.Abs(delta_y);
                    if (absDeltaY < closest_navigables[NavigationDirection.Up].distance)
                    {
                        closest_navigables[NavigationDirection.Up] = (nav, absDeltaY);
                    }
                    if (absDeltaY > furthest_navigables[NavigationDirection.Up].distance)
                    {
                        furthest_navigables[NavigationDirection.Up] = (nav, absDeltaY);
                    }
                }

                // Check Down (target is below and horizontally overlaps)
                if (delta_y < 0 && OverlapsHorizontally(self_bounds, target_bounds))
                {
                    float absDeltaY = Mathf.Abs(delta_y);
                    if (absDeltaY < closest_navigables[NavigationDirection.Down].distance)
                    {
                        closest_navigables[NavigationDirection.Down] = (nav, absDeltaY);
                    }
                    if (absDeltaY > furthest_navigables[NavigationDirection.Down].distance)
                    {
                        furthest_navigables[NavigationDirection.Down] = (nav, absDeltaY);
                    }
                }
            }

            // Assign found links or fallback to the furthest in the inverse direction
            foreach (var kvp in closest_navigables)
            {
                if (kvp.Value.navigable != null)
                {
                    m_links[kvp.Key] = kvp.Value.navigable;
                }
                else
                {
                    // If no closest navigable, fallback to the inverse direction
                    NavigationDirection inverseDirection = GetInverseDirection(kvp.Key);
                    m_links[kvp.Key] = furthest_navigables[inverseDirection].navigable;
                }
            }
        }

        private NavigationDirection GetInverseDirection(NavigationDirection direction)
        {
            switch (direction)
            {
                case NavigationDirection.Left: return NavigationDirection.Right;
                case NavigationDirection.Right: return NavigationDirection.Left;
                case NavigationDirection.Up: return NavigationDirection.Down;
                case NavigationDirection.Down: return NavigationDirection.Up;
                default: return NavigationDirection.None;
            }
        }

        // Helper to check if two bounds overlap horizontally
        private bool OverlapsHorizontally(Bounds a, Bounds b)
        {
            return a.max.x > b.min.x && a.min.x < b.max.x;
        }

        // Helper to check if two bounds overlap vertically
        private bool OverlapsVertically(Bounds a, Bounds b)
        {
            return a.max.y > b.min.y && a.min.y < b.max.y;
        }
    }
}