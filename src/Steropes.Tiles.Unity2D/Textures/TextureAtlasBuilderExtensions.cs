using System.IO;
using Steropes.Tiles.TexturePack.Operations;
using Steropes.Tiles.Unity2D.Tiles;
using UnityEngine;

namespace Steropes.Tiles.Unity2D.Textures
{
    public static class TextureAtlasBuilderExtensions
    {
        public static void WriteAll<TTexture>(this ITextureAtlasBuilder<TTexture> t)
            where TTexture: UnityTexture
        {
            foreach (var b in t.GetTextures())
            {
                var data = b.Texture.EncodeToPNG();
                File.WriteAllBytes(b.Name + ".png", data);
            }
        }
    }
}