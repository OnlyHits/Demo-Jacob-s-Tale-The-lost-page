using UnityEngine;

namespace CustomArchitecture
{
    public static class GameObjectBoundsExtensions
    {
        /// <summary>
        /// Attempts to get the world-space bounds of a GameObject from known components.
        /// Returns null if no valid bounds source is found.
        /// </summary>
#nullable enable
        public static Bounds? TryGetBounds(this GameObject? go)
        {
            if (go == null) return null;

            // SpriteRenderer
            var spriteRenderer = go.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                return spriteRenderer.bounds;

            // Collider2D (Box, Circle, Polygon, etc.)
            var collider2D = go.GetComponent<Collider2D>();
            if (collider2D != null)
                return collider2D.bounds;

            // RectTransform (for UI like Image, Panel, etc.)
            var rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform != null)
                return rectTransform.GetWorldBounds();

            // Renderer (generic fallback)
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                return renderer.bounds;

            return null;
        }
#nullable disable

        /// <summary>
        /// Attempts to get the world-space bounds of a GameObject from known components.
        /// Returns new Bounds() if no valid bounds source is found.
        /// </summary>
        public static Bounds GetBounds(this GameObject go)
        {
            if (go == null) return new Bounds();

            // SpriteRenderer
            var spriteRenderer = go.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                return spriteRenderer.bounds;

            // Collider2D (Box, Circle, Polygon, etc.)
            var collider2D = go.GetComponent<Collider2D>();
            if (collider2D != null)
                return collider2D.bounds;

            // RectTransform (for UI like Image, Panel, etc.)
            var rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform != null)
                return rectTransform.GetWorldBounds();

            // Renderer (generic fallback)
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null)
                return renderer.bounds;

            return new Bounds();
        }
    }
}