using System.Diagnostics;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.Sample.Shared.Strategy;
using Steropes.Tiles.Sample.Shared.Strategy.Rendering;
using Steropes.Tiles.TexturePack;
using Steropes.Tiles.TexturePack.Grids;
using Steropes.Tiles.TexturePack.Operations;
using Steropes.Tiles.Unity2D.Tiles;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;

namespace Steropes.Tiles.Unity2D.Demo.Components.Strategy
{
    public class StrategyGameRendering : UnityGameRenderingBase
    {
#pragma warning disable 649
        [SerializeField] TileRendererData renderTemplate;
        [SerializeField] GameObject renderParent;
        [SerializeField] RectTransform guiParent;
#pragma warning restore 649
        readonly UnityTextureOperations textureOperations;

        public StrategyGameData Data { get; set; }

        public StrategyGameRendering()
        {
            textureOperations = new UnityTextureOperations();
        }

        protected void Awake()
        {
            if (!renderParent)
            {
                renderParent = gameObject;
            }
        }

        StrategyGameTileSet<UnityTile> CreateTileSet(ITextureAtlasBuilder<UnityTexture> textureAtlasBuilder)
        {
            var basePath = "";
            if (Application.isEditor)
            {
                basePath = "Assets/Plugins/Steropes.Tiles.Demo/Resources";
            }

            Debug.Log("Using base path " + basePath);

            var loader = new TexturePackLoader<UnityTile, UnityTexture, UnityRawTexture>(
                new UnityFileContentLoader(),
                new UnityTileProducer(textureOperations, textureAtlasBuilder),
                new DefaultFileSystemAdapter(basePath));

            var tp = loader.Read("Tiles/Civ/tiles.xml");
            var rt = tp.TextureType == TextureType.Grid ? RenderType.Grid : RenderType.IsoDiamond;
            var tileSet = new StrategyGameTileSet<UnityTile>(tp, rt);
            tileSet.InitializeBlendingRules(Data.Rules);
            return tileSet;
        }

        protected override IEnumerable<UnityCoroutineResult<(RendererControl renderControl, GameRenderingConfig config)>> CreateRenderer()
        {
            Data = new StrategyGameData();
            Data.Fog.MapDataChanged += OnFogDataChanged;
            
            var textureAtlasBuilder = textureOperations.CreateAtlasBuilder();

            var sw = Stopwatch.StartNew();
            Profiler.BeginSample("DungeonGameRendering:CreateTileSet");
            var tileSet = CreateTileSet(textureAtlasBuilder);
            Profiler.EndSample();
            
            Debug.Log("Creating Tileset took: " + sw.Elapsed.TotalSeconds);
            yield return UnityCoroutineResult.Empty();
            
            var config = new GameRenderingConfig(tileSet.RenderType,
                                                 new Range(0, Data.TerrainWidth),
                                                 new Range(0, Data.TerrainHeight));

            sw = Stopwatch.StartNew();
            var unityTileProducer = new UnityTileProducer(textureOperations, textureAtlasBuilder);
            var fact = new StrategyGameRenderingFactory<UnityTile, UnityTexture, Color32>(config, Data, tileSet, 
                                                                                          unityTileProducer, textureOperations);

            foreach (var layer in fact.Create(new UnityRenderCallbackFactory<Nothing>(renderTemplate, renderParent)))
            {
                AddLayer(layer);
                sw.Stop();
                yield return UnityCoroutineResult.Empty();
                sw.Start();
            }
            
            Debug.Log("Creating Rendering System took: " + sw.Elapsed.TotalSeconds);
            yield return UnityCoroutineResult.Of((fact.RenderControl, fact.RenderingConfig));
        }
        
        
        void OnFogDataChanged(object sender, MapDataChangedEventArgs e)
        {
            // Fog layer is the last layer generated.
            MarkDirty(Count - 1);
        }

        void Update()
        {
            if (!RendererStarted)
            {
                return;
            }
            
            var mousePosition = Input.mousePosition;
            var sc = FromUnityScreenToAbsoluteViewCoordinates(mousePosition);
            Data.MousePosition = this.ScreenPositionToMapCoordinate(sc.ToViewCoordinate());
            
            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("MousePosition: " + mousePosition + " => " + sc);
                CenterPoint = sc;
            }
        }
    }
}