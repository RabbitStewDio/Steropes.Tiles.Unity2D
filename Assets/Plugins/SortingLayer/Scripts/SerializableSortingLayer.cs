using System;
using System.Linq;
using UnityEngine;

namespace UnityToolbag
{
    [Serializable]
    public class SerializableSortingLayer
    {
        public int id;

        public SerializableSortingLayer()
        {
        }

        public SerializableSortingLayer(int id)
        {
            this.id = id;
        }

        public static implicit operator SortingLayer(SerializableSortingLayer s)
        {
            return SortingLayer.layers.First(l => l.id == s.id);
        }

        public static implicit operator SerializableSortingLayer(SortingLayer s)
        {
            return new SerializableSortingLayer(s.id);
        }
    }
}