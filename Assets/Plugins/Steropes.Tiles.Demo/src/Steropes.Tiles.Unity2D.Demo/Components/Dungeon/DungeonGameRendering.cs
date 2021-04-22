using System.Diagnostics;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.Navigation;
using Steropes.Tiles.Sample.Shared.Dungeon;
using Steropes.Tiles.TexturePack.Atlas;
using Steropes.Tiles.Unity2D.Demo.Components.Strategy;
using Steropes.Tiles.Unity2D.Tiles;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityToolbag;

namespace Steropes.Tiles.Unity2D.Demo.Components.Dungeon
{
    public class DungeonGameRendering : UnityGameRenderingBase
    {
        public DungeonGameData Data { get; private set; }

#pragma warning disable 649
        [SerializeField]
        TileRendererData renderTemplate;

        [SerializeField]
        SerializableSortingLayer floorLayer;

        [SerializeField]
        SerializableSortingLayer itemLayer;
#pragma warning restore 649


        protected override IEnumerable<UnityCoroutineResult<(RendererControl renderControl, GameRenderingConfig config)>> CreateRenderer()
        {
            Data = new DungeonGameData();
            Profiler.BeginSample("DungeonGameRendering:CreateTileSet");
            var sw = Stopwatch.StartNew();
            var textureOperations = new UnityTextureOperations();
            var unityTileProducer = new UnityTileProducer(textureOperations, 
                                                          new MultiTextureAtlasBuilder<UnityTexture, Color32>(textureOperations));
            var tileSet = new DungeonTileSet<UnityTile, UnityTexture, UnityRawTexture>(new UnityAssetContentLoader(), unityTileProducer);
            UnityEngine.Debug.Log("Creating Tileset: " + sw.Elapsed.TotalSeconds);
            Profiler.EndSample();

            var config = new GameRenderingConfig(RenderType.IsoDiamond,
                                                 new Range(0, Data.Map.Width),
                                                 new Range(0, Data.Map.Height));
            config.Overdraw = new IntInsets();

            var fa = new DungeonGameRenderingFactory<UnityTile>(config, Data, tileSet);
            foreach (var l in fa.Create(new SortingLayerUnityRenderCallbackFactory(renderTemplate, gameObject), floorLayer, itemLayer))
            {
                AddLayer(l);
                yield return UnityCoroutineResult.Empty();
            }
            
            yield return UnityCoroutineResult.Of((fa.RenderControl, config));
        }

        protected override void AfterRendererStarted()
        {
            base.AfterRendererStarted();
            this.CenterPointInMapCoordinates = new MapCoordinate(1, 1);
        }

        void Update()
        {
            if (!RendererStarted)
            {
                return;
            }

            Data.Update(Time.time);
            MarkDirty(1);

            if (Input.GetMouseButtonUp(0))
            {
                var mousePosition = Input.mousePosition;
                var sc = FromUnityScreenToAbsoluteViewCoordinates(mousePosition);
                UnityEngine.Debug.Log("MousePosition: " + mousePosition + " => " + sc);
                CenterPoint = sc;
            }
        }
    }
}
