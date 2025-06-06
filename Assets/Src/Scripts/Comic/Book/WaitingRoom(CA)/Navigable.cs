using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static CustomArchitecture.CustomArchitecture;

namespace CustomArchitecture
{
    public abstract class Navigable : BaseBehaviour
    {
        private Dictionary<Direction, Navigable> m_links = null;
        [SerializeField] private bool m_isNavigable = true;
        protected bool m_isFocus = false;
        public bool IsNavigable() => m_isNavigable;

        // override this function to specify the bounds of your
        // navigable object.
        // This bound determine which navigable object
        // is going to be linked to this object
        // On a SpriteRenderer call sr.bounds()
        // On a RectTransform construct bounds by calling GetWorldCorners()
        // Todo : make an extension for rectTransform, or a wrapper class to make it generic
        public abstract Bounds GetGlobalBounds();
        public virtual void Focus() => m_isFocus = true;
        public virtual void Unfocus() => m_isFocus = false;

        // /!\ null check this function
        public Navigable GetLinkedNavigable(Direction direction) => m_links?[direction];

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
                { Direction.Left, null },
                { Direction.Right, null },
                { Direction.Up, null },
                { Direction.Down, null }
            };

            SetLinks((List<Navigable>)parameters[0]);
        }

        // Todo : make a generalization of this method
        // First thought is that there is no ideal way to generalize
        private void SetLinks(List<Navigable> navigables)
        {
            if (!IsNavigable())
                return;

            Bounds self_bounds = GetGlobalBounds();
            Vector3 self_center = self_bounds.center;

            Dictionary<Direction, (Navigable navigable, float distance)> closest_navigables = new()
            {
                { Direction.Left,  (null, float.MaxValue) },
                { Direction.Right, (null, float.MaxValue) },
                { Direction.Up,    (null, float.MaxValue) },
                { Direction.Down,  (null, float.MaxValue) }
            };

            Dictionary<Direction, (Navigable navigable, float distance)> furthest_navigables = new()
            {
                { Direction.Left,  (null, float.MinValue) },
                { Direction.Right, (null, float.MinValue) },
                { Direction.Up,    (null, float.MinValue) },
                { Direction.Down,  (null, float.MinValue) }
            };

            foreach (var nav in navigables)
            {
                if (nav == this || !nav.IsNavigable())
                    continue;

                Bounds target_bounds = nav.GetGlobalBounds();
                Vector3 target_center = target_bounds.center;

                float delta_x = target_center.x - self_center.x;
                float delta_y = target_center.y - self_center.y;

                // Check Left (target is to the left and vertically overlaps)
                if (delta_x < 0 && OverlapsVertically(self_bounds, target_bounds))
                {
                    float absDeltaX = Mathf.Abs(delta_x);
                    if (absDeltaX < closest_navigables[Direction.Left].distance)
                    {
                        closest_navigables[Direction.Left] = (nav, absDeltaX);
                    }
                    if (absDeltaX > furthest_navigables[Direction.Left].distance)
                    {
                        furthest_navigables[Direction.Left] = (nav, absDeltaX);
                    }
                }

                // Check Right (target is to the right and vertically overlaps)
                if (delta_x > 0 && OverlapsVertically(self_bounds, target_bounds))
                {
                    float absDeltaX = Mathf.Abs(delta_x);
                    if (absDeltaX < closest_navigables[Direction.Right].distance)
                    {
                        closest_navigables[Direction.Right] = (nav, absDeltaX);
                    }
                    if (absDeltaX > furthest_navigables[Direction.Right].distance)
                    {
                        furthest_navigables[Direction.Right] = (nav, absDeltaX);
                    }
                }

                // Check Up (target is above and horizontally overlaps)
                if (delta_y > 0 && OverlapsHorizontally(self_bounds, target_bounds))
                {
                    float absDeltaY = Mathf.Abs(delta_y);
                    if (absDeltaY < closest_navigables[Direction.Up].distance)
                    {
                        closest_navigables[Direction.Up] = (nav, absDeltaY);
                    }
                    if (absDeltaY > furthest_navigables[Direction.Up].distance)
                    {
                        furthest_navigables[Direction.Up] = (nav, absDeltaY);
                    }
                }

                // Check Down (target is below and horizontally overlaps)
                if (delta_y < 0 && OverlapsHorizontally(self_bounds, target_bounds))
                {
                    float absDeltaY = Mathf.Abs(delta_y);
                    if (absDeltaY < closest_navigables[Direction.Down].distance)
                    {
                        closest_navigables[Direction.Down] = (nav, absDeltaY);
                    }
                    if (absDeltaY > furthest_navigables[Direction.Down].distance)
                    {
                        furthest_navigables[Direction.Down] = (nav, absDeltaY);
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
                    Direction inverseDirection = GetInverseDirection(kvp.Key);
                    m_links[kvp.Key] = furthest_navigables[inverseDirection].navigable;
                }
            }
        }

        private Direction GetInverseDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Left: return Direction.Right;
                case Direction.Right: return Direction.Left;
                case Direction.Up: return Direction.Down;
                case Direction.Down: return Direction.Up;
                default: return Direction.None;
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