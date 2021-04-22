using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.Unity2D.Textures;
using Steropes.Tiles.Unity2D.Tiles;
using UnityEngine;

namespace Steropes.Tiles.Unity2D.Test
{
    public class TextureAtlasTest
    {
        UnityTextureOperations textureOperations;

        public TextureAtlasTest()
        {
            textureOperations = new UnityTextureOperations();
        }
        
        [Test]
        public void Test()
        {
            var texture = textureOperations.CreateTexture("atlas", new IntDimension(512, 512));
            var ta = new TextureAtlasBuilder<UnityTexture, Color32>(textureOperations, texture);

            ta.Insert(CreateTexture("red", Color.red), out var red);
            ta.Insert(CreateTexture("greed", Color.green), out var green);
            ta.Insert(CreateTexture("blue", Color.blue), out var blue);

            var png = ta.Texture.Texture.EncodeToPNG();
            File.WriteAllBytes("test.png", png);

            AssertTextureColor(red, Color.red);
        }

        void AssertTextureColor(UnityTexture t, Color32 c)
        {
            var bounds = t.Bounds;
            var px = textureOperations.ExtractData(t, t.Bounds);
            for (var i = 0; i < px.TextureData.Length; i++)
            {
                px.TextureData[i].Should().Be(c);
            }
        }

        public UnityTexture CreateTexture(string name, Color32 color)
        {
            var ta = textureOperations.CreateTexture(name, new IntDimension(2, 3));
            var px = new Color32[ta.Bounds.Width * ta.Bounds.Height];
            for (var i = 0; i < px.Length; i++)
            {
                px[i] = color;
            }
            ta.Texture.SetPixels32(px);
            ta.Texture.Apply();
            return ta;
        }
    }
}