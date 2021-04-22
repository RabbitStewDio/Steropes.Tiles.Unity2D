using System;
using System.Collections.Generic;
using System.Text;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.Navigation;
using Steropes.Tiles.TexturePack.Atlas;
using Steropes.Tiles.TexturePack.Operations;
using UnityEngine;

namespace Steropes.Tiles.Unity2D.Tiles
{
    public class UnityTextureOperations : ITextureOperations<UnityTexture, Color32>
    {
        readonly Dictionary<int, WeakReference<Color32[]>> clearTexturesCache;
        readonly Dictionary<int, WeakReference<Color32[]>> textureData;
        
        public UnityTextureOperations()
        {
            textureData = new Dictionary<int, WeakReference<Color32[]>>();
            clearTexturesCache = new Dictionary<int, WeakReference<Color32[]>>();
        }

        public void Dispose()
        {
            clearTexturesCache.Clear();
            textureData.Clear();
        }

        public TextureCoordinateRect ToNative(IntDimension context, IntRect src)
        {
            return new TextureCoordinateRect(src.X, context.Height - src.Y - src.Height, src.Width, src.Height);
        }

        public void MakeDebugVisible(BoundedTextureData<Color32> b)
        {
            for (var i = 0; i < b.TextureData.Length; i++)
            {
                if (b.TextureData[i].a > 0)
                {
                    return;
                }
            }

            Debug.LogWarning("Data is all transparent pixels");
        }

        public ITextureAtlasBuilder<UnityTexture> CreateAtlasBuilder()
        {
            return new MultiTextureAtlasBuilder<UnityTexture, Color32>(this);
        }

        public UnityTexture CreateTexture(string name, IntDimension tileSize, bool clearToTransparentBlack = true)
        {
            var texture = new Texture2D(tileSize.Width, tileSize.Height, TextureFormat.ARGB32, false, false);
            texture.filterMode = FilterMode.Point;
            texture.anisoLevel = 1;
            texture.wrapMode = TextureWrapMode.Clamp;

            if (clearToTransparentBlack)
            {
                texture.SetPixels32(FillClearTextureCache(tileSize));
            }

            return new UnityTexture(name, texture);
        }

        Color32[] FillClearTextureCache(IntDimension tileSize)
        {
            var l = tileSize.Width * tileSize.Height;
            if (clearTexturesCache.TryGetValue(l, out var entry) && entry.TryGetTarget(out var color))
            {
                return color;
            }
            
            var clearDataCache = new Color32[tileSize.Width * tileSize.Height];
            clearTexturesCache[l] = new WeakReference<Color32[]>(clearDataCache);
            return clearDataCache;
        }

        public TextureCoordinateRect TileAreaForCardinalDirection(IntDimension ts, CardinalIndex dir)
        {
            var w = ts.Width;
            var h = ts.Height;
            var wHalf = ts.Width / 2;
            var hHalf = ts.Height / 2;

            switch (dir)
            {
                case CardinalIndex.West:
                    return new TextureCoordinateRect(0, hHalf, wHalf, hHalf);
                case CardinalIndex.North:
                    return new TextureCoordinateRect(wHalf, hHalf, w - wHalf, hHalf);
                case CardinalIndex.East:
                    return new TextureCoordinateRect(wHalf, 0, w - wHalf, h - hHalf);
                case CardinalIndex.South:
                    return new TextureCoordinateRect(0, 0, wHalf, h - hHalf);
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        ///   Applies the texture data given in 'result' to the UnityTexture
        ///   'texture'. The texture data is copied at the bounds given in the
        ///   'result' texture data set.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public UnityTexture ApplyTextureData(UnityTexture texture, BoundedTextureData<Color32> result)
        {
            var b = result.Bounds;
            if (result.TextureData.Length != b.Width * b.Height)
            {
                throw new ArgumentException(
                    $"Texture data needs to have {b.Width} * {b.Height} = {b.Width * b.Height} pixels.");
            }

            if (texture.Bounds.Width != result.Bounds.Width)
            {
                throw new ArgumentException($"Width mismatch: src: {b.Width} target:{texture.Bounds.Width}");
            }

            if (texture.Bounds.Height != result.Bounds.Height)
            {
                throw new ArgumentException($"Width mismatch: src: {b.Width} target:{texture.Bounds.Width}");
            }

            var targetY = b.Y;
            // Debug.Log("Apply to " + targetY + " -> SrcBounds: " + b + " TargetBounds:" + texture.Bounds);
            texture.Texture.SetPixels32(b.X, targetY, b.Width, b.Height,
                result.TextureData);
            texture.Texture.Apply();
            return texture;
        }

        public UnityTexture ApplyTextureData(UnityTexture texture,
                                             BoundedTextureData<Color32> result,
                                             TextureCoordinatePoint offset)
        {
            var b = result.Bounds;
            if (result.TextureData.Length != b.Width * b.Height)
            {
                throw new ArgumentException();
            }

            if (offset.X + b.Width > texture.Texture.width)
            {
                throw new IndexOutOfRangeException();
            }

            if (offset.Y + b.Height > texture.Texture.height)
            {
                throw new IndexOutOfRangeException();
            }

            texture.Texture.SetPixels32(
                offset.X,
                offset.Y,
                b.Width,
                b.Height,
                result.TextureData);
            texture.Texture.Apply();
            return texture;
        }

        public BoundedTextureData<Color32> CreateClearTexture(IntDimension size)
        {
            var data = FillClearTextureCache(size);
            var bounds = new TextureCoordinateRect(0, 0, size.Width, size.Height);
            return new BoundedTextureData<Color32>(bounds, data);
        }

        public BoundedTextureData<Color32> CombineMask(BoundedTextureData<Color32> color,
                                                       BoundedTextureData<Color32> mask)
        {
            var colorBounds = color.Bounds;
            var maskBounds = mask.Bounds;
            if (colorBounds.Size != maskBounds.Size)
            {
                throw new ArgumentException("Masking requires equal-sized texture data: " + colorBounds.Size + " vs mask " + maskBounds.Size);
            }

            var width = colorBounds.Width;
            var height = colorBounds.Height;
            var retval = new Color32[width * height];
            for (var y = 0; y < height; y += 1)
            {
                for (var x = 0; x < width; x += 1)
                {
                    var px = colorBounds.X + x;
                    var py = colorBounds.Y + y;
                    var c = color[px, py];

                    var mx = maskBounds.X + x;
                    var my = maskBounds.Y + y;
                    var a = mask[mx, my];

                    var tidx = y * width + x;
                    var src = (Color) c;
                    var alpha = a.a / 255f;
                    //src.a = alpha;
                    c.a = a.a;
                    retval[tidx] = c;
                }
            }

            return new BoundedTextureData<Color32>(colorBounds, retval);
        }

        /// <summary>
        ///   Debug Method.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public static string GenerateImageDump(Color32[] data, int width)
        {
            StringBuilder b = new StringBuilder();
            for (var i = 0; i < data.Length; i++)
            {
                if (i > 0 && i % width == 0)
                {
                    b.Append("\n");
                }

                var px = data[i];
                b.AppendFormat($"{px.r:X2}:{px.g:X2}:{px.b:X2}:{px.a:X2} ");
            }

            return b.ToString();
        }

        public BoundedTextureData<Color32> CombineBlend(BoundedTextureData<Color32> background,
                                                        BoundedTextureData<Color32> foreground)
        {
            var backgroundBounds = background.Bounds;
            var foregroundBounds = foreground.Bounds;
            if (foreground.Bounds.Size != background.Bounds.Size)
            {
                throw new ArgumentException("Blending requires equal-sized texture data.");
            }

            var width = backgroundBounds.Width;
            var height = backgroundBounds.Height;

            var retval = new Color32[width * height];
            for (var y = 0; y < height; y += 1)
            {
                for (var x = 0; x < width; x += 1)
                {
                    var px = backgroundBounds.X + x;
                    var py = backgroundBounds.Y + y;
                    Color src = background[px, py];

                    var tx = foregroundBounds.X + x;
                    var ty = foregroundBounds.Y + y;
                    Color tgt = foreground[tx, ty];

                    var tidx = y * width + x;
                    retval[tidx] = src * (1 - tgt.a) + tgt * tgt.a;
                }
            }

            return new BoundedTextureData<Color32>(backgroundBounds, retval);
        }

        public BoundedTextureData<Color32> ExtractData(UnityTexture srcTexture,
                                                       TextureCoordinateRect localCoordinates)
        {
            var texture = srcTexture.Texture;
            if (texture == null)
            {
                return CreateClearTexture(localCoordinates.Size);
            }

            var rawData = ExtractRawData(srcTexture);
            var rectWithinTextureArea = localCoordinates.Translate(srcTexture.Bounds.Origin);
            var clippedSrcBounds = srcTexture.Bounds.Clip(rectWithinTextureArea);
            // Debug.Log("Extracting data: area=" + rectWithinTextureArea + "; src=" + srcTexture.Bounds);
            var textureWidth = texture.width;
            var clippedPixelData = new Color32[clippedSrcBounds.Width * clippedSrcBounds.Height];

            for (var y = 0; y < clippedSrcBounds.Height; y += 1)
            {
                for (var x = 0; x < clippedSrcBounds.Width; x += 1)
                {
                    var srcPos = (x + clippedSrcBounds.X) + (y + clippedSrcBounds.Y) * textureWidth;
                    var tgtPos = y * clippedSrcBounds.Width + x;
                    clippedPixelData[tgtPos] = rawData[srcPos];
                }
            }

            return new BoundedTextureData<Color32>(clippedSrcBounds, clippedPixelData);
        }

        Color32[] ExtractRawData(UnityTexture srcTexture)
        {
            Texture2D texture = srcTexture.Texture;
            Color32[] rawData = null;
            if (textureData.TryGetValue(texture.GetInstanceID(), out var entry) &&
                entry.TryGetTarget(out var data))
            {
                return data;
            }

            rawData = texture.GetPixels32();
            textureData[texture.GetInstanceID()] = new WeakReference<Color32[]>(rawData);
            return rawData;
        }

        public UnityTexture Clip(string name, UnityTexture texture, TextureCoordinateRect clipRegion)
        {
            return texture.Clip(name, clipRegion);
        }
    }
}