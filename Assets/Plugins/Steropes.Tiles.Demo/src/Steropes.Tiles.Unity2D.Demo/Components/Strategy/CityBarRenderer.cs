using System;
using System.Collections.Generic;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.Matcher.Sprites;
using Steropes.Tiles.Navigation;
using Steropes.Tiles.Renderer;
using Steropes.Tiles.Sample.Shared.Strategy.Model;
using UnityEngine;
using UnityEngine.UI;

namespace Steropes.Tiles.Unity2D.Demo.Components.Strategy
{
    /// <summary>
    ///  A specialised renderer that draws the city bar using a UI framework. Some
    ///  tasks are simply ill suited as tile based rendering.
    /// </summary>
    public class CityBarRenderer : IRenderCallback<ISettlement, Nothing>
    {
        readonly CityBarWidget template;
        readonly IRendererControl viewport;
        readonly RectTransform parent;
        readonly Stack<CityBarWidget> pool;
        readonly Dictionary<MapCoordinate, CityBarWidget> widgetsByMapPosition;
        readonly LayoutGroup layoutGroup;

        int epoch;
        IntPoint offset;

        public CityBarRenderer(RectTransform parent,
                               IRendererControl viewport)
        {
            this.viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
            // this.parent = parent ? parent : throw new ArgumentNullException(nameof(parent));
            this.pool = new Stack<CityBarWidget>();
            this.widgetsByMapPosition = new Dictionary<MapCoordinate, CityBarWidget>();
        }

        public void StartDrawing()
        {
            unchecked
            {
                epoch += 1;
            }

            offset = viewport.CalculateTileOffset();
        }

        public void StartLine(int logicalLine, in ContinuousViewportCoordinates screen)
        {
        }

        public void Draw(ISettlement tile,
                         Nothing context,
                         SpritePosition pos,
                         in ContinuousViewportCoordinates screenLocation)
        {
            if (widgetsByMapPosition.TryGetValue(tile.Location, out CityBarWidget w))
            {
                w.Epoch = epoch;
                w.UpdateSettlementData();
                Reposition(w, screenLocation);
            }
            else
            {
                w = pool.Count != 0 ? pool.Pop() : InstantiateWidget();
                w.Epoch = epoch;
                w.Settlement = tile;
                w.UpdateSettlementData();

                Reposition(w, screenLocation);
                widgetsByMapPosition[tile.Location] = w;
            }
        }

        CityBarWidget InstantiateWidget()
        {
            return UnityEngine.Object.Instantiate(template, parent, false);
        }

        void Reposition(CityBarWidget w, in ContinuousViewportCoordinates screenPos)
        {
            var renderPos = screenPos.ToPixels(viewport.TileSize); // + offset;

            w.RectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, (float) renderPos.X, w.RectTransform.rect.width);
            w.RectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, (float) renderPos.Y, w.RectTransform.rect.height);

            // var ds = w.DesiredSize;
            // return AnchoredRect.CreateTopLeftAnchored((int) (renderPos.X - ds.Width / 2),
            //     (int) (renderPos.Y - ds.Height * 2));
        }

        public void EndLine(int logicalLine, in ContinuousViewportCoordinates screen)
        {
        }

        public void FinishedDrawing()
        {
            foreach (var pair in widgetsByMapPosition)
            {
                if (pair.Value.Epoch != epoch)
                {
                    pair.Value.gameObject.SetActive(false);
                    pool.Push(pair.Value);
                }
            }

            foreach (var widget in pool)
            {
                widgetsByMapPosition.Remove(widget.Settlement.Location);
            }
        }
    }
}