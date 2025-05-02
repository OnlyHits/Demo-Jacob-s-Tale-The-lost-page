using UnityEngine;

namespace CustomArchitecture
{
    public static class Texture2DExtension
    {
        public static Sprite ConvertTextureToSprite(this Texture2D texture)
        {
            if (texture == null)
                return null;

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
    }
}