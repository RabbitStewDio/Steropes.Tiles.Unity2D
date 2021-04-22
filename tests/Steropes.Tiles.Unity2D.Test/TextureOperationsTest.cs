
using System;
using System.IO;
using FluentAssertions;
using MonoGame.Utilities.Png;
using NUnit.Framework;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.TexturePack.Operations;
using Steropes.Tiles.Unity2D.Tiles;
using UnityEngine;

namespace Steropes.Tiles.Unity2D.Test
{
    public class TextureOperationsTest 
    {
        Color32[] srcPixels;
        UnityTexture srcTexture;

        [SetUp]
        public void SetUp()
        {
            var srcData = File.ReadAllBytes("Assets/Plugins/Steropes.Tiles/src/Steropes.Tiles.Unity2D.Test/validation-sprite.png");
            var srcTextureRaw = new PngReader().Read(new MemoryStream(srcData));

            srcPixels = srcTextureRaw.GetPixels32();

            var bounds = new TextureCoordinateRect(0, 0, srcTextureRaw.width, srcTextureRaw.height);
            srcTexture = new UnityTexture("source", bounds, srcTextureRaw);
        }

        [Test]
        public void TestTextureLoading_Linear()
        {
            srcPixels[31 * 32].Should().Be(new Color32(99, 155, 255, 255)); // top left
        }

        [Test]
        public void TestPixelData_Stable_After_Clearing()
        {
            var srcTextureRaw = new Texture2D(1, 1, TextureFormat.RGBA32, false, false);
            srcTextureRaw.SetPixel(0, 0, new Color());

            srcPixels = srcTextureRaw.GetPixels32();
            srcPixels[0].Should().Be(new Color32());
        }

        [Test]
        public void TestTextureExtract()
        {
            var tgtTextureRaw = new Texture2D(32, 32, TextureFormat.ARGB32, false);
            var bounds = new TextureCoordinateRect(0, 0, tgtTextureRaw.width, tgtTextureRaw.height);
            var tgtTexture = new UnityTexture("target", bounds, tgtTextureRaw);

            UnityTextureOperations top = new UnityTextureOperations();
            var srcData = top.ExtractData(srcTexture, srcTexture.Bounds);

            top.ApplyTextureData(tgtTexture, srcData);

            // File.WriteAllText("t1.txt", string.Join(" \n", tgtTextureRaw.GetPixels32()));
            // File.WriteAllText("t2.txt", string.Join(" \n", srcPixels));

            tgtTextureRaw.GetPixels32().Should().BeEquivalentTo(srcPixels);
            
        }

        [Test]
        public void ApplyWithDifferentSizeShouldThrow()
        {
            var tgtTextureRaw = new Texture2D(32, 32, TextureFormat.ARGB32, false);
            var bounds = new TextureCoordinateRect(0, 0, tgtTextureRaw.width, tgtTextureRaw.height);
            var tgtTexture = new UnityTexture("target", bounds, tgtTextureRaw);

            UnityTextureOperations top = new UnityTextureOperations();
            Assert.Throws<ArgumentException>(() => top.ApplyTextureData(tgtTexture, top.CreateClearTexture(new IntDimension(4, 4))));
        }

        [Test]
        public void ValidateTextureCoordinateSystem()
        {
            srcTexture.Texture.width.Should().Be(32);
            srcTexture.Texture.height.Should().Be(32);

            srcPixels[0].Should().Be(new Color32()); 
            srcPixels[31].Should().Be(new Color32());
            srcPixels[31 * 32].Should().Be(new Color32(99, 155, 255, 255)); // top left
            srcPixels[31 + 31 * 32].Should().Be(new Color32());
        }
    }
}
