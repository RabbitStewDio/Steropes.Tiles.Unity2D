using System;
using System.Collections.Generic;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.Matcher.Sprites;
using Steropes.Tiles.Renderer;
using Steropes.Tiles.Unity2D.Tiles;
using UnityEngine;
using UnityEngine.Profiling;
using Object = UnityEngine.Object;

namespace Steropes.Tiles.Unity2D
{
    public class TileRenderer<TContext> : IRenderCallback<UnityTile, TContext>
    {
        readonly TileRendererData rendererTemplate;
        readonly IRendererControl renderControl;
        readonly SortingLayer sortOrder;
        readonly GameObject poolHolder;
        readonly GameObject renderHolder;

        readonly List<TileRendererData> activeRenderer;
        readonly List<TileRendererData> inactiveRenderer;
        readonly List<TileRendererData> pendingRenderer;

        readonly ViewportCoordinates[] offsetsBySpritePosition;
        readonly Vector3 tileAspectRatio;
        int tileCount;
        Vector3 globalOffset;

        public TileRenderer(TileRendererData rendererTemplate,
                            IRendererControl renderControl,
                            IMapViewport viewport,
                            GameObject parent,
                            SortingLayer sortOrder)
        {
            this.rendererTemplate = rendererTemplate
                ? rendererTemplate
                : throw new ArgumentNullException(nameof(rendererTemplate));
            this.renderControl = renderControl ?? throw new ArgumentNullException(nameof(renderControl));
            this.sortOrder = sortOrder;

            activeRenderer = new List<TileRendererData>();
            inactiveRenderer = new List<TileRendererData>();
            pendingRenderer = new List<TileRendererData>();

            var sortOrderName = sortOrder.name;
            poolHolder = new GameObject("Tile-Pool-" + sortOrderName);
            poolHolder.SetActive(false);
            poolHolder.transform.SetParent(parent.transform);

            renderHolder = new GameObject("Render-Pool-" + sortOrderName);
            renderHolder.SetActive(true);
            renderHolder.transform.SetParent(parent.transform);

            offsetsBySpritePosition = SpritePositionExtensions.OffsetsFor(renderControl.ActiveRenderType);

            tileAspectRatio = Vector3.one;
            if (renderControl.TileSize.Width > 0)
            {
                tileAspectRatio.y = (float) renderControl.TileSize.Height / renderControl.TileSize.Width;
            }
        }

        public void StartDrawing()
        {
            tileCount = short.MinValue;

            var boundsInPixels = renderControl.Bounds;
            var tileSize = renderControl.TileSize;

            var globalOffsetInPx = renderControl.CalculateTileOffset();
            globalOffset = new Vector3(
                tileAspectRatio.x * globalOffsetInPx.X / (float) tileSize.Width, 
                tileAspectRatio.y * globalOffsetInPx.Y / (float) tileSize.Height ,
                0
                );

            var boundsCenterX = boundsInPixels.X + boundsInPixels.Width / 2;
            var boundsCenterY = boundsInPixels.Y + boundsInPixels.Height / 2;

            renderHolder.transform.localPosition = new Vector3(
                (float) (-boundsCenterX / tileSize.Width) * tileAspectRatio.x,
                (float) (boundsCenterY / tileSize.Height)* tileAspectRatio.y, 
                0);

            pendingRenderer.Clear();
            pendingRenderer.Capacity = activeRenderer.Count;
            pendingRenderer.AddRange(activeRenderer);

            activeRenderer.Clear();
        }

        public void StartLine(int logicalLine, in ContinuousViewportCoordinates screen)
        {
        }

        /// <summary>
        ///  Draw a given sprite at the provided viewport coordinates. View
        ///  coordinates start at (0,0) in the upper left corner.
        ///
        ///  Screen coordinates are given in multiples of 4, so that the
        ///  int-range of these values covers all predefined positions without
        ///  introducing rounding errors.
        /// </summary>
        /// <param name="tile"></param>
        /// <param name="context"></param>
        /// <param name="pos"></param>
        /// <param name="c"></param>
        public void Draw(UnityTile tile, TContext context, SpritePosition pos, in ContinuousViewportCoordinates c)
        {
            Profiler.BeginSample("TileRenderer::Draw");

            if (tile.Sprite == null)
            {
                // This must be a dummy tile. Tile matchers never return null after all.
                return;
            }

            var offset = offsetsBySpritePosition[(int) pos];

            var screenPos = new ContinuousViewportCoordinates(offset.X + c.X, offset.Y + c.Y);

            var renderPos = new Vector3((float) screenPos.X / 4, (float) screenPos.Y / -4, 0);
            renderPos = Vector3.Scale(renderPos, tileAspectRatio) + globalOffset;

            var renderer = ReserveRenderer();
            renderer.UpdateTile(tile, sortOrder, tileCount);
            renderer.transform.localPosition = renderPos;

            tileCount += 1;

            Profiler.EndSample();
        }

        TileRendererData ReserveRenderer()
        {
            if (pendingRenderer.Count > 0)
            {
                var retval = pendingRenderer[pendingRenderer.Count - 1];
                pendingRenderer.RemoveAt(pendingRenderer.Count - 1);
                retval.RenderRun = Time.frameCount;
                activeRenderer.Add(retval);
                return retval;
            }

            if (inactiveRenderer.Count > 0)
            {
                var retval = inactiveRenderer[inactiveRenderer.Count - 1];
                inactiveRenderer.RemoveAt(inactiveRenderer.Count - 1);
                retval.RenderRun = Time.frameCount;
                activeRenderer.Add(retval);
                retval.transform.SetParent(renderHolder.transform);
                return retval;
            }

            var next = Object.Instantiate(rendererTemplate, renderHolder.transform, false);
            next.gameObject.name = "Tile-" + sortOrder + " @ " + Time.frameCount;
            next.RenderRun = Time.frameCount;
            next.gameObject.SetActive(true);
            activeRenderer.Add(next);
            return next;
        }

        public void EndLine(int logicalLine, in ContinuousViewportCoordinates screen)
        {
        }

        public void FinishedDrawing()
        {
            foreach (var r in pendingRenderer)
            {
                inactiveRenderer.Add(r);
                r.transform.SetParent(poolHolder.transform);
            }

            pendingRenderer.Clear();

            // Debug.Log("Rendered: "  + Time.frameCount + " / " + sortOrder.name + " -> " + activeRenderer.Count + " tiles active, " + inactiveRenderer.Count + " tiles reserved.");
        }
    }
}