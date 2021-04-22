using UnityEngine;

namespace Steropes.Tiles.Unity2D.Tiles
{
    public static class TextureLoader
    {
        public static Texture2D Load(byte[] data, bool linear = false)
        {
            // return new PngReader().Read(new MemoryStream(data));

            return LoadWithUnity(data, linear);
        }

        static Texture2D LoadWithUnity(byte[] data, bool linear)
        {
            var t = new Texture2D(1, 1, TextureFormat.ARGB32, false, linear);
            t.filterMode = FilterMode.Point;
            t.anisoLevel = 1;
            t.wrapMode = TextureWrapMode.Clamp;
            t.LoadImage(data, false);
            // Debug.Log("After loading: " + t.mipmapCount + " " + t.wrapMode + " " + t.filterMode);
            return t;
        }
    }
}