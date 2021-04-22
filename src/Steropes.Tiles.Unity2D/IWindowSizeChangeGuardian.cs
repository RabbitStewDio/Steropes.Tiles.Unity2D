using System;
using UnityEngine;

namespace Steropes.Tiles.Unity2D
{
    public interface IWindowSizeChangeGuardian
    {
        event EventHandler WindowSizeChanged;
        Vector2Int CurrentSize { get; }
    }
}