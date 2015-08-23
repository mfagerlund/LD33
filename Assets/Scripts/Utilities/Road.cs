using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class Road : MonoBehaviour
{
    private Mesh _mesh;
    private float weldDistance = 0.05f;
    private List<Edge> _edges;
    private List<Edge> _outline;
    
    public void Start()
    {
        _mesh = GetComponent<MeshFilter>().sharedMesh;
        List<Vertex> vertices = GetVertices();
        _edges = GetEdges(vertices);
        _outline = _edges.Where(e => e.Count == 1).ToList();
    }

    //public void Update()
    //{
    //    Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

    //    if (IsPointOnRoad(mousePos))
    //    {
    //        Debug.Log("On the road again!");
    //    }
    //}
    
    public void OnDrawGizmos()
    {
        if (_edges == null)
        {
            Start();
        }

        Gizmos.color = Color.magenta;
        foreach (Edge edge in _outline)
        {
            Gizmos.DrawLine(edge.FromVertex.Position, edge.ToVertex.Position);
        }
    }

    // See http://geomalgorithms.com/a03-_inclusion.html
    // wn_PnPoly(): winding number test for a point in a polygon
    //      Input:   P = a point,
    //               V[] = vertex points of a polygon V[n+1] with V[n]=V[0]
    //      Return:  wn = the winding number (=0 only when P is outside)
    public bool IsPointOnRoad(Vector2 p)
    {
        if (_outline == null)
        {
            Start();
        }
        int wn = 0;    // the  winding number counter

        // loop through all edges of the polygon
        foreach (Edge edge in _outline)
        {
            Vector2 vi = edge.FromVertex.Position;
            Vector2 vi1 = edge.ToVertex.Position;
            // edge from V[i] to  V[i+1]
            if (vi.y <= p.y)
            {          // start y <= P.y
                if (vi1.y > p.y)      // an upward crossing
                    if (GetIsLeft(vi, vi1, p))  // P left of  edge
                        ++wn;            // have  a valid up intersect
            }
            else
            {                        // start y > P.y (no test needed)
                if (vi1.y <= p.y)     // a downward crossing
                    if (!GetIsLeft(vi, vi1, p)) // P right of  edge
                    {
                        --wn; // have  a valid down intersect
                    }
            }
        }

        return wn != 0;
    }

    private static bool GetIsLeft(Vector2 end, Vector2 start, Vector2 p)
    {
        var cross =
            (end.x - start.x) * (p.y - start.y)
            - (end.y - start.y) * (p.x - start.x);
        return cross < 0;
    }

    private List<Edge> GetEdges(List<Vertex> vertices)
    {
        List<Edge> edges = new List<Edge>();
        for (int index = 0; index < _mesh.triangles.Length; index += 3)
        {
            Vertex v0 = vertices[_mesh.triangles[index]];
            Vertex v1 = vertices[_mesh.triangles[index + 1]];
            Vertex v2 = vertices[_mesh.triangles[index + 2]];
            AddEdge(v0, v1, edges);
            AddEdge(v1, v2, edges);
            AddEdge(v2, v0, edges);
        }
        return edges;
    }

    private void AddEdge(Vertex v0, Vertex v1, List<Edge> edges)
    {
        foreach (Edge edge in edges)
        {
            if (edge.Matches(v0, v1))
            {
                edge.Count++;
                return;
            }
        }

        edges.Add(new Edge { Count = 1, FromVertex = v0, ToVertex = v1 });
    }

    private List<Vertex> GetVertices()
    {
        float weldDistanceSqr = weldDistance * weldDistance;
        List<Vertex> vertices = new List<Vertex>();
        foreach (Vector2 rawPos in _mesh.vertices)
        {
            Vector2 pos = transform.TransformPoint(rawPos);
            Vertex newVertex = null;
            foreach (Vertex vertex in vertices)
            {
                if ((vertex.Position - pos).sqrMagnitude <= weldDistanceSqr)
                {
                    newVertex = vertex;
                    break;
                }
            }

            if (newVertex == null)
            {
                newVertex = new Vertex(pos);
            }

            vertices.Add(newVertex);
        }
        return vertices;
    }

    public class Vertex
    {
        public Vertex(Vector2 position)
        {
            Position = position;
        }

        public Vector2 Position { get; set; }
    }

    public class Edge
    {
        public bool Matches(Vertex from, Vertex to)
        {
            return
                from == FromVertex && to == ToVertex
                || to == FromVertex && from == ToVertex;
        }
        public Vertex FromVertex { get; set; }
        public Vertex ToVertex { get; set; }
        public int Count { get; set; }
    }
}
