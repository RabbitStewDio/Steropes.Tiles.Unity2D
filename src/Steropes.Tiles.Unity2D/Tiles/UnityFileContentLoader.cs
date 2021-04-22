using System.IO;
using Steropes.Tiles.TexturePack;
using UnityEngine;

namespace Steropes.Tiles.Unity2D.Tiles
{
    public class UnityFileContentLoader : IContentLoader<UnityRawTexture>
    {
        readonly string basePath;

        public UnityFileContentLoader(string basePath = null)
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

            var data = Load(path);

            Debug.Log("Loading ... " + path);
            var texture = TextureLoader.Load(data);
            return new UnityRawTexture(path, texture);
        }

        static byte[] Load(string path)
        {
            if (File.Exists(path))
            {
                return System.IO.File.ReadAllBytes(path);
            }

            if (File.Exists(path + ".png"))
            {
                return System.IO.File.ReadAllBytes(path + ".png");
            }

            if (File.Exists(path + ".jpg"))
            {
                return System.IO.File.ReadAllBytes(path + ".jpg");
            }

            if (File.Exists(path + ".jpeg"))
            {
                return System.IO.File.ReadAllBytes(path + ".jpeg");
            }

            throw new FileNotFoundException(null, path);
        }
    }
}