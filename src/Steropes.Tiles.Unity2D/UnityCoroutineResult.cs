using JetBrains.Annotations;
using UnityEngine;

namespace Steropes.Tiles.Unity2D
{
    public readonly struct UnityCoroutineResultEmptyPlaceholder {}

    public static class UnityCoroutineResult
    {
        public static UnityCoroutineResult<T> Empty<T>()
        {
            return new UnityCoroutineResult<T>();
        }

        public static UnityCoroutineResultEmptyPlaceholder Empty()
        {
            return new UnityCoroutineResultEmptyPlaceholder();
        }

        public static UnityCoroutineResult<T> Of<T>(T t)
        {
            return new UnityCoroutineResult<T>(t);
        }
    }
    
    public readonly struct UnityCoroutineResult<T>
    {
        readonly bool hasResult;

        [CanBeNull]
        readonly YieldInstruction yieldInstruction;

        [CanBeNull]
        readonly CustomYieldInstruction customYieldInstruction;

        [CanBeNull]
        readonly T result;

        public UnityCoroutineResult([CanBeNull] T result)
        {
            this.hasResult = true;
            this.result = result;
            this.yieldInstruction = default;
            this.customYieldInstruction = default;
        }

        public UnityCoroutineResult([CanBeNull] CustomYieldInstruction customYieldInstruction)
        {
            this.hasResult = false;
            this.customYieldInstruction = customYieldInstruction;
            this.yieldInstruction = default;
            this.result = default;
        }

        public UnityCoroutineResult([CanBeNull] YieldInstruction yieldInstruction)
        {
            this.hasResult = false;
            this.yieldInstruction = yieldInstruction;
            this.customYieldInstruction = default;
            this.result = default;
        }

        public static implicit operator UnityCoroutineResult<T>(YieldInstruction p)
        {
            return new UnityCoroutineResult<T>(p);
        }

        public static implicit operator UnityCoroutineResult<T>(CustomYieldInstruction p)
        {
            return new UnityCoroutineResult<T>(p);
        }

        public static implicit operator UnityCoroutineResult<T>(UnityCoroutineResultEmptyPlaceholder p)
        {
            return default;
        }

        public bool TryGetResult(out T result)
        {
            if (hasResult)
            {
                result = this.result;
                return true;
            }

            result = default;
            return false;
        }

        public object GetYield()
        {
            if (yieldInstruction != null)
            {
                return yieldInstruction;
            }

            return customYieldInstruction;
        }
    }
}
