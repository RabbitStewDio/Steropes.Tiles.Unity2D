using UnityEngine;

namespace Steropes.Tiles.Unit2D.Demo.Components
{
    public class TraceLogEnabler: MonoBehaviour
    {
        class UnityLogTraceListener : LogAdapterBase
        {
            readonly TraceLogEnabler t;
            readonly string traceName;

            public UnityLogTraceListener(TraceLogEnabler t, string traceName)
            {
                this.t = t;
                this.traceName = traceName;
            }

            public override bool IsTraceEnabled => Debug.unityLogger?.logEnabled == true && Debug.unityLogger?.IsLogTypeAllowed(LogType.Log) == true;

            public override void Trace(string message)
            {
                if (t.enableLogging)
                {
                    Debug.unityLogger?.LogFormat(LogType.Log, "[{0}] {1}", traceName, message);
                }
            }
        }

        [SerializeField]
        bool enableLogging;

        void OnEnable()
        {
            LogProvider.Provider = CreateLogger;
        }

        void OnDisable()
        {
            LogProvider.Provider = null;
        }

        public ILogAdapter CreateLogger(string traceName)
        {
            return new UnityLogTraceListener(this, traceName);
        }
    }
}