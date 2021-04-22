using System;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.TexturePack;
using Steropes.Tiles.TexturePack.Operations;
using UnityEngine;

namespace Steropes.Tiles.Unity2D.Tiles
{
    public class UnityTexture: ITexture
    {
        public UnityTexture(string name, Texture2D texture)
        {
            if (!texture)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            Name = name;
            Bounds = new TextureCoordinateRect(0, 0, texture.width, texture.height);
            Texture = texture;
        }

        public UnityTexture(string name, TextureCoordinateRect bounds, Texture2D texture)
        {
            if (!texture)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            Name = name;
            Bounds = bounds;
            Texture = texture;
        }

        public bool Valid => Texture;

        public string Name { get; }
        public TextureCoordinateRect Bounds { get; }
        public Texture2D Texture { get; }

        public UnityTexture Clip(string name, TextureCoordinateRect bounds)
        {
            return new UnityTexture(name ?? Name, bounds, Texture);
        }
    }

    
    public class UnityRawTexture: IRawTexture<UnityTexture>
    {
        public UnityRawTexture(string name, Texture2D texture)
        {
            if (!texture)
            {
                throw new ArgumentNullException(nameof(texture));
            }

            Name = name;
            Bounds = new IntRect(0, 0, texture.width, texture.height);
            Texture = texture;
        }

        public bool Valid => Texture;

        public string Name { get; }
        public IntRect Bounds { get; }
        public Texture2D Texture { get; }

        public UnityTexture CreateSubTexture(string name, TextureCoordinateRect bounds)
        {
            return new UnityTexture(name ?? Name, bounds, Texture);
        }

    }
}