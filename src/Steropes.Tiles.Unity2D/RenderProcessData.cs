using Steropes.Tiles.Plotter;
using UnityEngine;

namespace Steropes.Tiles.Unity2D
{
    [CreateAssetMenu]
    public class RenderProcessData : ScriptableObject
    {
        public IRendererControl Viewport { get; set; }

        public GameRenderingConfig RenderConfig { get; } 
        public RendererControl RenderControl { get; }
        public GridPlotter GridPlotter => RenderConfig.CreatePlotter();

    }
}