using FluentAssertions;
using NUnit.Framework;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.Matcher.Registry;
using Steropes.Tiles.Navigation;
using Steropes.Tiles.TexturePack.Blending;
using Steropes.Tiles.TexturePack.Operations;
using Steropes.Tiles.Unity2D.Tiles;
using UnityEngine;

namespace Steropes.Tiles.Unity2D.Test
{
    public class BlendingTest
    {
        static readonly Color South = Color.red;
        static readonly Color North = Color.blue;
        static readonly Color East = Color.green;
        static readonly Color West = Color.yellow;

        static readonly IntDimension tileSize = new IntDimension(2,2);
        readonly UnityTileProducer tileProd;

        public BlendingTest()
        {
            tileProd = new UnityTileProducer(
                new UnityTextureOperations(), 
                new NoOpTextureAtlasBuilder<UnityTexture>());
        }

        UnityTile CreateTile(string name, UnityTexture mask)
        {
            var anchor = new IntPoint(1, 1);
            return tileProd.Produce(tileSize, anchor, "mask", mask);
        }

        [Test]
        public void TestBlending_North()
        {
            var gen = SetupGenerator();

            gen.TryGenerate("tile", CardinalIndex.North, out var tileGen);
            tileGen.Texture.Texture.GetPixel(0, 0).Should().Be(new Color());
            tileGen.Texture.Texture.GetPixel(0, 1).Should().Be(new Color());
            tileGen.Texture.Texture.GetPixel(1, 0).Should().Be(new Color());
            tileGen.Texture.Texture.GetPixel(1, 1).Should().Be(North);
        }

        [Test]
        public void TestBlending_East()
        {
            var gen = SetupGenerator();

            gen.TryGenerate("tile", CardinalIndex.East, out var tileGen);
            tileGen.Texture.Texture.GetPixel(0, 0).Should().Be(new Color());
            tileGen.Texture.Texture.GetPixel(0, 1).Should().Be(new Color());
            tileGen.Texture.Texture.GetPixel(1, 0).Should().Be(East);
            tileGen.Texture.Texture.GetPixel(1, 1).Should().Be(new Color());
        }

        [Test]
        public void TestBlending_South()
        {
            var gen = SetupGenerator();

            gen.TryGenerate("tile", CardinalIndex.South, out var tileGen);
            tileGen.Texture.Texture.GetPixel(0, 0).Should().Be(South);
            tileGen.Texture.Texture.GetPixel(0, 1).Should().Be(new Color());
            tileGen.Texture.Texture.GetPixel(1, 0).Should().Be(new Color());
            tileGen.Texture.Texture.GetPixel(1, 1).Should().Be(new Color());
        }

        [Test]
        public void TestBlending_West()
        {
            var gen = SetupGenerator();

            gen.TryGenerate("tile", CardinalIndex.West, out var tileGen);
            tileGen.Texture.Texture.GetPixel(0, 0).Should().Be(new Color());
            tileGen.Texture.Texture.GetPixel(0, 1).Should().Be(West);
            tileGen.Texture.Texture.GetPixel(1, 0).Should().Be(new Color());
            tileGen.Texture.Texture.GetPixel(1, 1).Should().Be(new Color());
        }

        BlendingTileGenerator<UnityTile, UnityTexture, Color32> SetupGenerator()
        {
            var top = new UnityTextureOperations();
            var maskTile = CreateTile("mask", CreateMask(top));
            var terrainTile = CreateTile("tile", CreateTile(top));

            var tileRegistry = new BasicTileRegistry<UnityTile>();
            tileRegistry.Add("tile", terrainTile);
            var gen = CreateGenerator(top, tileRegistry, maskTile);
            return gen;
        }

        BlendingTileGenerator<UnityTile, UnityTexture, Color32> 
            CreateGenerator(UnityTextureOperations top,
                            ITileRegistry<UnityTile> reg,
                            UnityTile mask)
        {
            return new BlendingTileGenerator<UnityTile, UnityTexture, Color32>(reg, top, tileProd, 
                RenderType.IsoDiamond, tileSize, mask);
        }

        static UnityTexture CreateMask(UnityTextureOperations top)
        {
            var mask = top.CreateTexture("mask", tileSize);
            mask.Texture.SetPixel(0, 0, Color.white);
            mask.Texture.SetPixel(1, 0, Color.white);
            mask.Texture.SetPixel(0, 1, Color.white);
            mask.Texture.SetPixel(1, 1, Color.white);
            return mask;
        }

        static UnityTexture CreateTile(UnityTextureOperations top)
        {
            var mask = top.CreateTexture("mask", tileSize);
            mask.Texture.SetPixel(0, 0, South);
            mask.Texture.SetPixel(1, 0, East);
            mask.Texture.SetPixel(0, 1, West);
            mask.Texture.SetPixel(1, 1, North);
            return mask;
        }
    }
}