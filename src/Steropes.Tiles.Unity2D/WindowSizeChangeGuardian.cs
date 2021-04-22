using System;
using UnityEngine;

namespace Steropes.Tiles.Unity2D
{
    /// <summary>
    ///  Monogame does not report Window size changes if a window is minimized or
    ///  maximized.  This component works around this problem by simply polling
    ///  the size before each draw. If needed, it then generates an event for all
    ///  interested listeners. 
    /// </summary>
    public class WindowSizeChangeGuardian : MonoBehaviour, IWindowSizeChangeGuardian
    {
        public Vector2Int CurrentSize { get; private set; }
        
        int lastFrame;
        /// <summary>
        ///  Unity fails badly if the editor has a window open on more than one screen.
        ///  Is anyone surprised that they fuck it up again?
        /// </summary>
        bool unityBuggy;

        public event EventHandler WindowSizeChanged;
        
        void Update()
        {
            if (unityBuggy)
            {
                return;
            }

            var newSize = new Vector2Int(Screen.width, Screen.height);
            if (CurrentSize == newSize)
            {
                return;
            }

            if (lastFrame > 1 && Time.frameCount - lastFrame < 10)
            {
                Debug.Log("Unity cannot decide a proper screen resolution: Was " + CurrentSize + "; wants " + newSize + " ");
                unityBuggy = true;
            }

            lastFrame = Time.frameCount;
            if (!unityBuggy)
            {
                Debug.LogFormat(this, "Screen size change detected [{2}]. Was {0}, now {1}", CurrentSize, newSize, lastFrame);
                CurrentSize = newSize;
                WindowSizeChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}