using System;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.Unity2D.Tiles;
using UnityEngine;
using Rect = UnityEngine.Rect;

namespace Steropes.Tiles.Unity2D
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class TileRendererData : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] SpriteRenderer spriteRenderer;
#pragma warning restore 649
        [NonSerialized] int renderRun;
        
        UnityTile tile;
        int sortOrder;
        SortingLayer sortingLayer;

        public int RenderRun
        {
            get => renderRun;
            set => renderRun = value;
        }

        public SpriteRenderer SpriteRenderer => spriteRenderer;

        public void UpdateTile(UnityTile tile, SortingLayer sortingLayer, int sortOrder)
        {
            if (tile != this.tile || sortOrder != this.sortOrder || sortingLayer.id != this.sortingLayer.id)
            {
                SpriteRenderer.sprite = tile.Sprite;
                SpriteRenderer.color = Color.white;
                SpriteRenderer.sortingOrder = sortOrder;
                SpriteRenderer.sortingLayerID = sortingLayer.id;

                this.tile = tile;
                this.sortOrder = sortOrder;
                this.sortingLayer = sortingLayer;
            }
        }

        public string Tag => tile?.Tag;
        public IntPoint Anchor => tile?.Anchor ?? new IntPoint();
        public Vector2 Pivot => tile?.Sprite?.pivot ?? default;
        public Bounds Bounds => tile?.Sprite?.bounds ?? default;
        public Rect TextureBounds => tile?.Sprite?.rect ?? default;
    }
}