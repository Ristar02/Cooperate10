using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Map
{
    public class MapView : MonoBehaviour
    {
        public enum MapOrientation
        {
            BottomToTop,
            TopToBottom,
            RightToLeft,
            LeftToRight
        }

        public MapManager mapManager;
        public MapOrientation orientation;
        public List<MapConfig> allMapConfigs;
        public GameObject nodePrefab;
        public float orientationOffset;
        public Sprite background;
        public Color32 backgroundColor = Color.white;
        public float xSize;
        public float yOffset;
        [Header("Line Settings")]
        public GameObject linePrefab;
        [Range(3, 10)]
        public int linePointsCount = 10;
        public float offsetFromNodes = 0.5f;
        [Header("Colors")]
        public Color32 visitedColor = Color.white;
        public Color32 lockedColor = Color.gray;
        public Color32 lineVisitedColor = Color.white;
        public Color32 lineLockedColor = Color.gray;

        protected GameObject firstParent;
        protected GameObject mapParent;
        private List<List<Vector2Int>> paths;
        private Camera cam;
        // ALL nodes:
        public readonly List<MapNode> MapNodes = new List<MapNode>();
        protected readonly List<LineConnection> lineConnections = new List<LineConnection>();

        public static MapView Instance;

        public Map Map { get; protected set; }

        private void Awake()
        {
            Instance = this;
            cam = Camera.main;
        }

        protected virtual void ClearMap()
        {
         
            if (mapParent != null)
                DG.Tweening.DOTween.Kill(mapParent.transform);
    
            // 리스트 정리
            MapNodes.Clear();
            lineConnections.Clear();
            
            if (firstParent != null)
            {
                Destroy(firstParent);
                firstParent = null;
            }
    
            mapParent = null;
        }

        public virtual void ShowMap(Map m)
        {
            if (m == null)
            {
                return;
            }

            Map = m;

            ClearMap();

            CreateMapParent();

            CreateNodes(m.nodes);

            DrawLines();

            SetOrientation();

            ResetNodesRotation();

            SetAttainableNodes();

            SetLineColors();

            CreateMapBackground(m);
            
            Debug.Log($"Total nodes: {MapNodes.Count}");
            Debug.Log($"Boss nodes: {MapNodes.Count(n => n.Node.nodeType == NodeType.Boss)}");
            foreach(var node in MapNodes)
            {
                Debug.Log($"Node: {node.Node.blueprintName} - Type: {node.Node.nodeType}");
            }
        }

        protected virtual void CreateMapBackground(Map m)
        {
            if (background == null) return;

            GameObject backgroundObject = new GameObject("Background");
            backgroundObject.transform.SetParent(mapParent.transform);
            MapNode bossNode = MapNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.Boss);
            float span = m.DistanceBetweenFirstAndLastLayers();
            backgroundObject.transform.localPosition = new Vector3(bossNode.transform.localPosition.x, span / 2f, 0f);
            backgroundObject.transform.localRotation = Quaternion.identity;
            SpriteRenderer sr = backgroundObject.AddComponent<SpriteRenderer>();
            sr.color = backgroundColor;
            sr.drawMode = SpriteDrawMode.Sliced;
            sr.sprite = background;
            sr.size = new Vector2(xSize, span + yOffset * 2f);
        }

        protected virtual void CreateMapParent()
        {
            firstParent = new GameObject("OuterMapParent");
            mapParent = new GameObject("MapParentWithAScroll");
            mapParent.transform.SetParent(firstParent.transform);
            ScrollNonUI scrollNonUi = mapParent.AddComponent<ScrollNonUI>();
            scrollNonUi.freezeX = orientation == MapOrientation.BottomToTop || orientation == MapOrientation.TopToBottom;
            scrollNonUi.freezeY = orientation == MapOrientation.LeftToRight || orientation == MapOrientation.RightToLeft;
            BoxCollider boxCollider = mapParent.AddComponent<BoxCollider>();
            boxCollider.size = new Vector3(100, 100, 1);
        }

        protected void CreateNodes(IEnumerable<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                MapNode mapNode = CreateMapNode(node);
                MapNodes.Add(mapNode);
            }
        }

        protected virtual MapNode CreateMapNode(Node node)
        {
            GameObject mapNodeObject = Instantiate(nodePrefab, mapParent.transform);
            MapNode mapNode = mapNodeObject.GetComponent<MapNode>();
            NodeBlueprint blueprint = GetBlueprint(node.blueprintName);
            mapNode.SetUp(node, blueprint);
            mapNode.transform.localPosition = node.position;
            return mapNode;
        }

        public void SetAttainableNodes()
        {
            foreach (MapNode node in MapNodes)
                node.SetState(NodeStates.Locked);

            if (mapManager.CurrentMap.path.Count == 0)
            {
                foreach (MapNode node in MapNodes.Where(n => n.Node.point.y == 0))
                    node.SetState(NodeStates.Attainable);
            }
            else
            {
                foreach (Vector2Int point in mapManager.CurrentMap.path)
                {
                    MapNode mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Visited);
                }

                Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);
                
                foreach (Vector2Int point in currentNode.outgoing)
                {
                    MapNode mapNode = GetNode(point);
                    if (mapNode != null)
                        mapNode.SetState(NodeStates.Attainable);
                }
            }
        }

        public virtual void SetLineColors()
        {
            foreach (LineConnection connection in lineConnections)
                connection.SetColor(lineLockedColor);
            
            if (mapManager.CurrentMap.path.Count == 0)
                return;
            
            Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
            Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);

            foreach (Vector2Int point in currentNode.outgoing)
            {
                LineConnection lineConnection = lineConnections.FirstOrDefault(conn => conn.from.Node == currentNode &&
                                                                            conn.to.Node.point.Equals(point));
                lineConnection?.SetColor(lineVisitedColor);
            }

            if (mapManager.CurrentMap.path.Count <= 1) return;

            for (int i = 0; i < mapManager.CurrentMap.path.Count - 1; i++)
            {
                Vector2Int current = mapManager.CurrentMap.path[i];
                Vector2Int next = mapManager.CurrentMap.path[i + 1];
                LineConnection lineConnection = lineConnections.FirstOrDefault(conn => conn.@from.Node.point.Equals(current) &&
                                                                            conn.to.Node.point.Equals(next));
                lineConnection?.SetColor(lineVisitedColor);
            }
        }

        protected virtual void SetOrientation()
        {
            ScrollNonUI scrollNonUi = mapParent.GetComponent<ScrollNonUI>();
            float span = mapManager.CurrentMap.DistanceBetweenFirstAndLastLayers();
            MapNode bossNode = MapNodes.FirstOrDefault(node => node.Node.nodeType == NodeType.Boss);
            
            firstParent.transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, 0f);
            float offset = orientationOffset;
            switch (orientation)
            {
                case MapOrientation.BottomToTop:
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.yConstraints.max = 0;
                        scrollNonUi.yConstraints.min = -(span + 2f * offset);
                    }
                    firstParent.transform.localPosition += new Vector3(0, offset, 0);
                    break;
                case MapOrientation.TopToBottom:
                    mapParent.transform.eulerAngles = new Vector3(0, 0, 180);
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.yConstraints.min = 0;
                        scrollNonUi.yConstraints.max = span + 2f * offset;
                    }
                    firstParent.transform.localPosition += new Vector3(0, -offset, 0);
                    break;
                case MapOrientation.RightToLeft:
                    offset *= cam.aspect;
                    mapParent.transform.eulerAngles = new Vector3(0, 0, 90);
                    firstParent.transform.localPosition -= new Vector3(offset, bossNode.transform.position.y, 0);
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.xConstraints.max = span + 2f * offset;
                        scrollNonUi.xConstraints.min = 0;
                    }
                    break;
                case MapOrientation.LeftToRight:
                    offset *= cam.aspect;
                    mapParent.transform.eulerAngles = new Vector3(0, 0, -90);
                    firstParent.transform.localPosition += new Vector3(offset, -bossNode.transform.position.y, 0);
                    if (scrollNonUi != null)
                    {
                        scrollNonUi.xConstraints.max = 0;
                        scrollNonUi.xConstraints.min = -(span + 2f * offset);
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawLines()
        {
            foreach (MapNode node in MapNodes)
            {
                foreach (Vector2Int connection in node.Node.outgoing)
                    AddLineConnection(node, GetNode(connection));
            }
        }

        private void ResetNodesRotation()
        {
            foreach (MapNode node in MapNodes)
                node.transform.rotation = Quaternion.identity;
        }

        protected virtual void AddLineConnection(MapNode from, MapNode to)
        {
            if (linePrefab == null) return;

            GameObject lineObject = Instantiate(linePrefab, mapParent.transform);
            LineRenderer lineRenderer = lineObject.GetComponent<LineRenderer>();
            Vector3 fromPoint = from.transform.position +
                                (to.transform.position - from.transform.position).normalized * offsetFromNodes;

            Vector3 toPoint = to.transform.position +
                              (from.transform.position - to.transform.position).normalized * offsetFromNodes;
            
            lineObject.transform.position = fromPoint;
            lineRenderer.useWorldSpace = false;
            
            lineRenderer.positionCount = linePointsCount;
            for (int i = 0; i < linePointsCount; i++)
            {
                lineRenderer.SetPosition(i,
                    Vector3.Lerp(Vector3.zero, toPoint - fromPoint, (float)i / (linePointsCount - 1)));
            }

            DottedLineRenderer dottedLine = lineObject.GetComponent<DottedLineRenderer>();
            if (dottedLine != null) dottedLine.ScaleMaterial();

            lineConnections.Add(new LineConnection(lineRenderer, null, from, to));
        }

        protected MapNode GetNode(Vector2Int p)
        {
            return MapNodes.FirstOrDefault(n => n.Node.point.Equals(p));
        }

        protected MapConfig GetConfig(string configName)
        {
            return allMapConfigs.FirstOrDefault(c => c.name == configName);
        }

        protected NodeBlueprint GetBlueprint(NodeType type)
        {
            MapConfig config = GetConfig(mapManager.CurrentMap.configName);
            return config.nodeBlueprints.FirstOrDefault(n => n.nodeType == type);
        }

        protected NodeBlueprint GetBlueprint(string blueprintName)
        {
            MapConfig config = GetConfig(mapManager.CurrentMap.configName);
            return config.nodeBlueprints.FirstOrDefault(n => n.name == blueprintName);
        }
    }
}
