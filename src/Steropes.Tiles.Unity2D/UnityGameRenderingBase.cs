using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Steropes.Tiles.DataStructures;
using Steropes.Tiles.Navigation;
using Steropes.Tiles.Plotter;
using Steropes.Tiles.Plotter.Operations;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Profiling;
using Rect = Steropes.Tiles.DataStructures.Rect;

namespace Steropes.Tiles.Unity2D
{
    public abstract class UnityGameRenderingBase : MonoBehaviour, IViewportControl, IReadOnlyList<IPlotOperation>
    {
        IPlotter gridPlotter;
        readonly List<IPlotOperation> plotters;
        readonly List<bool> plotterStatus;

        public event EventHandler OnRendererStarted;

        public UnityGameRenderingBase()
        {
            plotters = new List<IPlotOperation>();
            plotterStatus = new List<bool>();
        }

        public GameRenderingConfig RenderConfig { get; set; }

        protected abstract IEnumerable<UnityCoroutineResult<(RendererControl renderControl, GameRenderingConfig config)>> CreateRenderer();
        
        IEnumerator Start()
        {
            foreach (var x in CreateRenderer())
            {
                if (x.TryGetResult(out var result))
                {
                    RenderConfig = result.config;
                    RenderControl = result.renderControl;
                }

                yield return null;
            }

            RenderControl.PropertyChanged += OnRenderPropertyChanged;
            RenderConfig.PropertyChanged += OnRenderingConfigPropertyChanged;

            if (RenderConfig.RenderType != RenderControl.ActiveRenderType)
            {
                throw new ArgumentException("RenderConfig render type and renderControl render type must match.");
            }

            RotationSteps = 0;
            CenterPointInMapCoordinates = new MapCoordinate(0, 0);

            gridPlotter = RenderConfig.CreatePlotter();
            RendererStarted = true;
            OnRendererStarted?.Invoke(this, EventArgs.Empty);
            AfterRendererStarted();
        }

        protected virtual void AfterRendererStarted()
        {

        }

        public bool RendererStarted { get; set; }

        public RendererControl RenderControl { get; private set; }

        public IntDimension ViewPortSize
        {
            get { return RenderConfig?.Size ?? default; }
        }

        public Rect Bounds
        {
            get { return RenderControl?.Bounds ?? default; }
            set
            {
                RenderControl.Bounds = value;
                InvalidateAll();
            }
        }

        public RenderType ActiveRenderType
        {
            get { return RenderConfig?.RenderType ?? default; }
        }

        public IntInsets Overdraw
        {
            get { return RenderConfig?.Overdraw ?? default; }
            set
            {
                RenderConfig.Overdraw = value; 
                InvalidateAll();
            }
        }

        public IntDimension TileSize
        {
            get { return RenderControl?.TileSize ?? default; }
        }

        public int RotationSteps
        {
            get { return RenderConfig?.RotationSteps ?? default; }
            set
            {
                RenderConfig.RotationSteps = value; 
                InvalidateAll();
            }
        }

        public ContinuousViewportCoordinates CenterPoint
        {
            get { return RenderConfig?.CenterPoint ?? default; }
            set
            {
                RenderConfig.CenterPoint = value; 
                InvalidateAll();
            }
        }

        public MapCoordinate CenterPointInMapCoordinates
        {
            get { return RenderConfig?.CenterPointInMapCoordinates ?? default; }
            set { RenderConfig.CenterPointInMapCoordinates = value; }
        }

        public ContinuousViewportCoordinates MapPositionToScreenPosition(DoublePoint mapPosition)
        {
            return RenderConfig.Viewport.MapPositionToScreenPosition(mapPosition);
        }

        public ViewportCoordinates MapPositionToScreenPosition(MapCoordinate mapPosition)
        {
            return RenderConfig.Viewport.MapPositionToScreenPosition(mapPosition);
        }

        public DoublePoint ScreenPositionToMapPosition(ContinuousViewportCoordinates screenPosition)
        {
            return RenderConfig.Viewport.ScreenPositionToMapPosition(screenPosition);
        }

        public MapCoordinate ScreenPositionToMapCoordinate(ViewportCoordinates screenPosition)
        {
            return RenderConfig.Viewport.ScreenPositionToMapCoordinate(screenPosition);
        }

        public IMapNavigator<GridDirection> MapNavigator
        {
            get { return RenderConfig.MatcherNavigator; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnRenderingConfigPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        void OnRenderPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IRendererControl.Bounds))
            {
                RenderConfig.Size = CalculateViewportSize(RenderControl.Bounds);
                OnPropertyChanged(nameof(Bounds));
            }
        }

        protected void LateUpdate()
        {
            if (!RendererStarted)
            {
                return;
            }
            
            for (var index = 0; index < plotters.Count; index++)
            {
                var plotter = plotters[index];
                if (plotterStatus[index])
                {
                    gridPlotter.Draw(plotter);
                    plotterStatus[index] = false;
                }
            }

            LateUpdateOverride();
        }

        protected virtual void LateUpdateOverride()
        {
        }

        public void AddLayers(IEnumerable<IPlotOperation> ops)
        {
            foreach (var op in ops)
            {
                plotters.Add(op);
                plotterStatus.Add(true);
            }
        }

        public void AddLayer(IPlotOperation op)
        {
            plotters.Add(op);
            plotterStatus.Add(true);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<IPlotOperation> IEnumerable<IPlotOperation>.GetEnumerator()
        {
            return plotters.GetEnumerator();
        }

        public List<IPlotOperation>.Enumerator GetEnumerator()
        {
            return plotters.GetEnumerator();
        }

        public IPlotOperation this[int idx]
        {
            get { return plotters[idx]; }
        }

        public void MarkDirty(IPlotOperation op)
        {
            var idx = plotters.IndexOf(op);
            if (idx != -1)
            {
                plotterStatus[idx] = true;
            }
        }

        public void MarkDirty(int idx)
        {
            if (idx >= 0 && idx < plotterStatus.Count)
            {
                plotterStatus[idx] = true;
            }
        }

        public void InvalidateAll()
        {
            for (var i = 0; i < plotterStatus.Count; i++)
            {
                plotterStatus[i] = true;
                plotters[i].InvalidateAll();
            }
        }

        public int Count => plotters.Count;

        IntDimension CalculateViewportSize(Rect r)
        {
            var widthInFullTiles = (int) Math.Ceiling(r.Width / TileSize.Width);
            var heightInFullTiles = (int) Math.Ceiling(r.Height / TileSize.Height);
            return new IntDimension(widthInFullTiles, heightInFullTiles);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ContinuousViewportCoordinates FromUnityScreenToAbsoluteViewCoordinates(Vector2 mousePos)
        {
            // Get mouse position in normalized coordinate format ..
            // (y-axis downwards; relative to viewport center point)
            var c = Bounds.Center;
            var x = mousePos.x - c.X;
            var y = Screen.height - mousePos.y - 1 - c.Y;

            // convert to normalized screen coordinates (where 0,0 is still middle of viewport)
            var mouseInScreenScale = ContinuousViewportCoordinates.FromPixels(TileSize, x, y);
            var mouseInAbsoluteViewCoordinates = mouseInScreenScale + CenterPoint;

            return mouseInAbsoluteViewCoordinates;
        }
    }
}