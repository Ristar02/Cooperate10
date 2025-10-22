using System.Collections.Generic;
using UnityEngine;

namespace Map
{
    [System.Serializable]
    public class NodeWeight
    {
        public NodeType _nodeType;
        [Range (0, 100)]
        public int weight;
    }
    
    
    [CreateAssetMenu]
    public class MapConfig : ScriptableObject
    {
        public List<NodeBlueprint> nodeBlueprints;

        public List<NodeWeight> nodeWeights = new List<NodeWeight>
        {
            new NodeWeight { _nodeType = NodeType.MinorEnemy, weight = 43 },
            new NodeWeight { _nodeType = NodeType.EliteEnemy, weight = 22 },
            new NodeWeight { _nodeType = NodeType.Store, weight = 10 },
            new NodeWeight { _nodeType = NodeType.Event, weight = 25 },
        };
        
        public int GridWidth => Mathf.Max(numOfPreBossNodes.max, numOfStartingNodes.max);

        
        public IntMinMax numOfPreBossNodes;
        public IntMinMax numOfStartingNodes;
        
        public int extraPaths;
        public List<MapLayer> layers;
        
        public NodeType GetRandomNodeType()
        {
            if (nodeWeights.Count == 0)
            {
                return NodeType.MinorEnemy;
            }
            
            int totalWeight = 0;
            foreach (var weight in nodeWeights)
            {
                totalWeight += weight.weight;
            }
            
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;
            
            foreach (var weight in nodeWeights)
            {
                currentWeight += weight.weight;
                if (randomValue < currentWeight)
                {
                    return weight._nodeType;
                }
            }
            
            return NodeType.MinorEnemy;
        }
        
    }
    
   
}