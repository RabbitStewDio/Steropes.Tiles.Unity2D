using System;
using Steropes.Tiles.DataStructures;
using UnityEngine;

namespace Steropes.Tiles.Unity2D
{
    public static class XnaExtensions
    {

        public static RectInt Union(this RectInt r1, RectInt r2)
        {
            int x = Math.Min(r1.xMin, r2.xMin);
            int y = Math.Min(r1.yMin, r2.yMin);
            return new RectInt(x, y, Math.Max(r1.xMax, r2.xMax) - x, Math.Max(r1.yMax, r2.yMax) - y);
        }

        public static RectInt ToXna(this IntRect rect)
        {
            return new RectInt(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static IntRect ToTileRect(this RectInt rect)
        {
            return new IntRect(rect.x, rect.y, rect.width, rect.height);
        }
    }
}