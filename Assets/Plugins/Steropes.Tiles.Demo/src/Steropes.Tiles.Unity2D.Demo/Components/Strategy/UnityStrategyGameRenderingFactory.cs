using Steropes.Tiles.DataStructures;
using Steropes.Tiles.Renderer;
using Steropes.Tiles.Sample.Shared;
using Steropes.Tiles.Sample.Shared.Strategy;
using Steropes.Tiles.Sample.Shared.Strategy.Model;
using Steropes.Tiles.Sample.Shared.Strategy.Rendering;
using Steropes.Tiles.Unity2D.Tiles;
using System;
using UnityEngine;
using UnityToolbag;

namespace Steropes.Tiles.Unity2D.Demo.Components.Strategy
{
    public class UnityCityBarRenderingFactory : CityBarRenderingFactoryBase
    {
        readonly RectTransform guiParent;

        public UnityCityBarRenderingFactory(GameRenderingConfig renderingConfig, StrategyGameData gameData, IntDimension tileSize, RectTransform guiParent) : base(renderingConfig, gameData, tileSize)
        {
            this.guiParent = guiParent;
        }

        protected override IRenderCallback<ISettlement, Nothing> CreateRenderer()
        {
            return new CityBarRenderer(guiParent, RenderControl);
        }
    }

    public class UnityRenderCallbackFactory<TCreationContext> : IRenderCallbackFactory<TCreationContext, UnityTile>
    {
        readonly TileRendererData template;
        readonly GameObject parent;
        int sortOrder;

        public UnityRenderCallbackFactory(TileRendererData template, GameObject parent)
        {
            this.template = template;
            this.parent = parent;
        }

        public IRenderCallback<UnityTile, TContext> CreateRenderer<TContext>(IRenderingFactoryConfig<UnityTile> tileSetSource, TCreationContext p)
        {
            var layers = SortingLayer.layers;
            var retval = new TileRenderer<TContext>(template, tileSetSource.RenderControl, tileSetSource.RenderingConfig.Viewport, parent, layers[Math.Min(sortOrder, layers.Length - 1)]);
            sortOrder += 1;
            Debug.Log("Layers: " + sortOrder + " -- " + SortingLayer.layers.Length);
            return retval;
        }
    }
    
    public class SortingLayerUnityRenderCallbackFactory : IRenderCallbackFactory<SerializableSortingLayer, UnityTile>
    {
        readonly TileRendererData template;
        readonly GameObject parent;

        public SortingLayerUnityRenderCallbackFactory(TileRendererData template, GameObject parent)
        {
            this.template = template;
            this.parent = parent;
        }

        public IRenderCallback<UnityTile, TContext> CreateRenderer<TContext>(IRenderingFactoryConfig<UnityTile> tileSetSource, SerializableSortingLayer p)
        {
            return new TileRenderer<TContext>(template, tileSetSource.RenderControl, tileSetSource.RenderingConfig.Viewport, parent, p);
        }
    }
}