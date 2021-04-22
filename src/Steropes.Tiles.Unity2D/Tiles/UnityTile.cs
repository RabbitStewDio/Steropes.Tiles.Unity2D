using System;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.TexturePack;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Steropes.Tiles.Unity2D.Tiles
{
    public class UnityTile : ITexturedTile<UnityTexture>
    {
        public static readonly UnityTile Empty = new UnityTile();

        UnityTile()
        {
            Tag = "*";
            HasTexture = false;
        }

        public UnityTile(string tag, UnityTexture texture, IntDimension tileSize, IntPoint anchor)
        {
            HasTexture = true;
            Tag = tag ?? throw new ArgumentNullException(nameof(tag));
            Texture = texture ?? throw new ArgumentNullException(nameof(texture));
            Anchor = anchor;

            try
            {
                var textureBounds = texture.Bounds;
                var bounds = new Rect(textureBounds.X, textureBounds.Y,
                    textureBounds.Width, textureBounds.Height);
                var anchorX = textureBounds.Width <= 1 ? 0 : anchor.X / (float) (textureBounds.Width - 1);
                var anchorY = textureBounds.Height <= 1 ? 0 : 1 - (anchor.Y / (float) (textureBounds.Height - 1));
                var relativeAnchor = new Vector2(anchorX, anchorY);
                var ppu = tileSize.Width;
                Sprite = Sprite.Create(texture.Texture, bounds, relativeAnchor, ppu, 0, SpriteMeshType.FullRect);
                Sprite.name = $"{tag}";
            }
            catch (ArgumentException e)
            {
                throw new ArgumentException($"Unable to create tile {Tag} from texture {texture.Texture}", e);
            }
        }

        public IntPoint Anchor { get; }
        public bool HasTexture { get; }
        public Sprite Sprite { get; }
        public UnityTexture Texture { get; }
        public string Tag { get; }
    }
}