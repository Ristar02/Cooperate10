
using UnityEngine;

namespace Map
{
    [System.Serializable]
    public class MapLayer
    {
        public NodeType nodeType;
        public FloatMinMax distanceFromPreviousLayer;
        public float nodesApartDistance;
        [Range(0f, 1f)] public float randomizePosition;
        [Range(0f, 1f)] public float randomizeNodes;
    }
}