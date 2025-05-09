using UnityEngine;

namespace CustomArchitecture
{
    public static class SpriteExtension
    {
        /// <summary>
        /// Returns the PPU divided by sprite pixel size as a Vector2.
        /// That is: (ppu / widthInPixels, ppu / heightInPixels)
        /// </summary>
        /// <param name="sprite">The sprite to evaluate.</param>
        /// <returns>A Vector2 of (ppu / width, ppu / height).</returns>
        public static Vector2 GetPPUPerPixelSize(this Sprite sprite)
        {
            if (sprite == null)
                return Vector2.zero;

            float ppu = sprite.pixelsPerUnit;
            Vector2 pixelSize = sprite.rect.size; // width and height in pixels

            return new Vector2(ppu / pixelSize.x, ppu / pixelSize.y);
        }
    }
}