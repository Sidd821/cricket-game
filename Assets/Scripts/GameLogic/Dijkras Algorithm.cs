using System.Collections.Generic;
using UnityEngine;

public class DijkrasAlgorithm : MonoBehaviour
{
    public Transform outfield;  // This is the outfield
    public int fieldRadius;     // outfield radius (units)
    public float groundY = 1f;  // treat Y=1 so it doesnt go through the floor


    public int cellSize = 1;    // spacing between nodes

    [Header("Debug gizmos")]
    public bool drawNodes = true;
    public bool drawBlocked = true;
    public Color nodeColor = new Color(0f, 1f, 0.3f, 0.9f);
    public Color blockedColor = new Color(1f, 0.2f, 0.2f, 0.9f);
    public float nodeGizmoSize = 0.15f;

    public Node[,] cellArray;
    public List<Node> debugPath;
    public bool chased = false;

    // Optional adjacency mirror (not required since node.edges already holds neighbors)
    private List<List<Edge>> adjacency = new List<List<Edge>>();

    void Awake()
    {
        BuildNodes();
        BuildAdjacency();
    }
   
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            BuildNodes();
            BuildAdjacency();
        }
    }

    public void BuildNodes()
    {
        if (outfield == null)
        {
            return;
        }

        // Note: assumes base mesh diameter ≈ 1 unit; otherwise assign radius manually
        fieldRadius = Mathf.Max(1, Mathf.RoundToInt(outfield.localScale.x * 0.5f));
        int maxNodes = Mathf.Max(1, fieldRadius / Mathf.Max(1, cellSize));

        cellArray = new Node[maxNodes * 2, maxNodes * 2];

        for (int i = -maxNodes; i < maxNodes; i++)
        {
            for (int j = -maxNodes; j < maxNodes; j++)
            {
                Vector3 newCell = new Vector3(outfield.position.x + i * cellSize, groundY, outfield.position.z + j * cellSize);

                float distanceX = newCell.x - outfield.position.x;
                float distanceZ = newCell.z - outfield.position.z;
                float distance2 = distanceX * distanceX + distanceZ * distanceZ;
                float radius2 = fieldRadius * fieldRadius;
                if (distance2 > radius2)
                {
                    continue;
                }


                cellArray[i + maxNodes, j + maxNodes] = new Node
                {
                    position = newCell,
                    id = (i + maxNodes).ToString() + (j + maxNodes).ToString()
                };
            }
        }
    }

    public void BuildAdjacency()
    {
        if (cellArray == null)
        {
            return;
        }

        adjacency.Clear();

        int width = cellArray.GetLength(0);
        int height = cellArray.GetLength(1);

        // For each node, clear old edges (important on rebuild) and connect to up to 8 neighbors
        //...
        //.N.
        //...
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Node currentNode = cellArray[x, z];
                if (currentNode == null)
                {
                    continue;
                }
                currentNode.edges.Clear();

                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i == 0 && j == 0)
                        {
                            continue;
                        }

                        int nx = x + i;
                        int nz = z + j;
                        if (nx < 0 || nz < 0 || nx >= width || nz >= height)
                        {
                            continue;
                        }
                        Node neighbour = cellArray[nx, nz];
                        if (neighbour == null)
                        {
                            continue;
                        }
                        float weight;

                        if (i != 0 && j != 0)
                        {
                            weight = (cellSize * 1.41421356f);
                        }
                        else
                        {
                            weight = cellSize;
                        }

                        currentNode.AddEdge(neighbour, weight);
                    }
                }
            }
        }

        // Optional: mirror node.edges into adjacency list (not used elsewhere)
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Node n = cellArray[x, z];
                if (n != null)
                {
                    adjacency.Add(n.edges);
                }
            }
        }
    }

    public Node ObjectToNode(Vector3 pos)
    {
        if (cellArray == null)
        {
            return null;
        }

        Node closestNode = null;
        float closestDistance = Mathf.Infinity;

        int w = cellArray.GetLength(0);
        int h = cellArray.GetLength(1);

        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                Node n = cellArray[i, j];
                if (n == null)
                {
                    continue;
                }

                float d = Vector3.SqrMagnitude(n.position - pos); // faster (squared)
                if (d < closestDistance)
                {
                    closestDistance = d;
                    closestNode = n;
                }
            }
        }

        if (closestNode != null)
        {
            //Debug.Log("Closest node found: " + closestNode.id);
        }
            

        return closestNode;
    }

    public List<Node> PathFinding(Vector3 startPos, Vector3 endPos)
    {
        if (cellArray == null)
        {
            return new List<Node>();
        }

        ResetNode();

        Node startNode = ObjectToNode(startPos);
        Node endNode = ObjectToNode(endPos);
        if (startNode == null || endNode == null)
        {
            return new List<Node>();
        }

        // Gather all nodes once
        List<Node> all = new List<Node>();
        int w = cellArray.GetLength(0);
        int h = cellArray.GetLength(1);
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                if (cellArray[i, j] != null)
                {
                    all.Add(cellArray[i, j]);
                }
            }
                
        }

        for (int i = 0; i < all.Count; i++)
        {
            Node n = all[i];
            n.distance = Mathf.Infinity;
            n.previous = null;
            n.visited = false;
        }
        startNode.distance = 0f;

        while (true)
        {
            Node u = null;
            float best = Mathf.Infinity;

            for (int i = 0; i < all.Count; i++)
            {
                Node n = all[i];
                if (!n.visited && n.distance < best)
                {
                    best = n.distance;
                    u = n;
                }
            }

            if (u == null || best == Mathf.Infinity)
            {
                break; // no more reachable
            }
            if (u == endNode)
            {
                break; // reached goal
            }

            u.visited = true;

            // Relax neighbors
            for (int i = 0; i < u.edges.Count; i++)
            {
                Edge e = u.edges[i];
                Node v = e.to;
                if (v == null || v.visited)
                {
                    continue;
                }

                float alt = u.distance + e.weight;
                if (alt < v.distance)
                {
                    v.distance = alt;
                    v.previous = u;
                }
            }
        }

        // Reconstruct path
        List<Node> path = new List<Node>();
        for (Node n = endNode; n != null; n = n.previous)
        { 
            path.Add(n);
        }
        path.Reverse();

        // If unreachable, path will not start at startNode
        if (path.Count == 0 || path[0] != startNode)
        {
            path.Clear();
            return path;
        }

        debugPath = path;
        return path;
    }

    public void ResetNode()
    {
        if (cellArray == null)
        {
            return;
        }

        int w = cellArray.GetLength(0);
        int h = cellArray.GetLength(1);
        for (int i = 0; i < w; i++)
        {
            for (int j = 0; j < h; j++)
            {
                Node currentNode = cellArray[i, j];
                if (currentNode != null)
                {
                    currentNode.distance = Mathf.Infinity;
                    currentNode.previous = null;
                    currentNode.visited = false;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!drawNodes || outfield == null)
        {
            return;
        }

        // Field boundary
        Gizmos.color = nodeColor;
        DrawCircle(outfield.position, fieldRadius, 50);

        if (cellArray == null)
        {
            return;
        }

        int width = cellArray.GetLength(0);
        int height = cellArray.GetLength(1);

        // Draw nodes + their edges
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Node node = cellArray[x, z];
                if (node == null)
                {
                    continue;
                }

                Gizmos.color = nodeColor;
                Gizmos.DrawSphere(node.position, nodeGizmoSize);

                Gizmos.color = Color.blue;
                for (int i = 0; i < node.edges.Count; i++)
                {
                    Edge edge = node.edges[i];
                    if (edge.to != null)
                    {
                        Gizmos.DrawLine(node.position, edge.to.position);
                    }
                }
            }
        }

        // Draw debug path
        if (debugPath != null && debugPath.Count > 1)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(debugPath[0].position, nodeGizmoSize * 2f); // Start
            Gizmos.DrawSphere(debugPath[^1].position, nodeGizmoSize * 2f); // End

            for (int i = 0; i < debugPath.Count - 1; i++)
                Gizmos.DrawLine(debugPath[i].position, debugPath[i + 1].position);
        }
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float t = (i / (float)segments) * Mathf.PI * 2f;
            Vector3 next = center + new Vector3(Mathf.Cos(t) * radius, 0f, Mathf.Sin(t) * radius);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}

public class Node
{
    public string id;
    public Vector3 position;
    public List<Edge> edges = new List<Edge>();
    public float distance = Mathf.Infinity; // Distance from start node
    public Node previous = null;            // For path reconstruction
    public bool visited = false;

    public void AddEdge(Node toNode, float weight)
    {
        edges.Add(new Edge(this, toNode, weight));
    }
}

public class Edge
{
    public Node from;
    public Node to;
    public float weight;

    public Edge(Node f, Node t, float w) { from = f; to = t; weight = w; }
}
