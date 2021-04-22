using Steropes.Tiles.TexturePack;
using UnityEngine;

namespace Steropes.Tiles.Unity2D.Tiles
{
    public class UnityAssetContentLoader : IContentLoader<UnityRawTexture>
    {
        readonly string basePath;

        public UnityAssetContentLoader(string basePath = null)
        {
            this.basePath = basePath;
        }

        public UnityRawTexture LoadTexture(string name)
        {
            string path;
            if (!string.IsNullOrEmpty(basePath))
            {
                path = $"{basePath}/{name}";
            }
            else
            {
                path = name;
            }

            var texture = Resources.Load<Texture2D>(path);
            return new UnityRawTexture(path, texture);
        }
    }
}