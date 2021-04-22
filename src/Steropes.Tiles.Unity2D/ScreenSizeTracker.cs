using System;
using UnityEngine;
using UnityEngine.U2D;
using Rect = Steropes.Tiles.DataStructures.Rect;

namespace Steropes.Tiles.Unity2D
{
    public class ScreenSizeTracker: MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField] WindowSizeChangeGuardian windowSizeChangeGuardian;
        [SerializeField] new UnityGameRenderingBase renderer;
        [SerializeField] RectOffset padding;
        [SerializeField] new PixelPerfectCamera camera;
        [SerializeField] int tileWidth;

        /// <summary>
        ///   Defines whether screen resolution changes alter the scale to keep a constant number of items visible or
        ///   whether screen resolution changes make more map items visible.
        /// </summary>
        [SerializeField] bool scaleToResolution;
#pragma warning restore 649

        void OnEnable()
        {
            windowSizeChangeGuardian.WindowSizeChanged += OnWindowSizeChanged;
            renderer.OnRendererStarted += OnWindowSizeChanged;
            if (renderer.RendererStarted)
            {
                OnWindowSizeChanged(this, EventArgs.Empty);
            }
        }

        void OnDisable()
        {
            windowSizeChangeGuardian.WindowSizeChanged -= OnWindowSizeChanged;
        }

        void OnWindowSizeChanged(object sender, EventArgs e)
        {
            var sw = Screen.width;
            var sh = Screen.height;

            var x = padding.left;
            var y = padding.top;
            var w = Mathf.Max(1, sw - padding.horizontal);
            var h = Mathf.Max(1, sh - padding.vertical);
            if (renderer.RendererStarted)
            {
                renderer.Bounds = new Rect(x, y, w, h);
            }

            if (camera && !scaleToResolution)
            {
                camera.assetsPPU = tileWidth;
                camera.refResolutionX = sw;
                camera.refResolutionY = sh;
            }
        }
    }
}