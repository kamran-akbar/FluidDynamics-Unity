using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MassSpringSimulator : MonoBehaviour
{
    public Material mat;
    public float scale;
    public int pointNumber;
    public float distance;
    public float width;
    public float stifness;
    public Vector3 gravity;
    public float restLength;
    public float mass;
    public float dampingCoefficient;
    public float dragCoefficient;
    public Material material;

    MeshFilter meshFilter;
    GameObject[] spheres;
    Vector3[] points;
    Vector3[] velocities;
    Vector3[] forces;
    Edge[] edges;
    Constraint[] constraints;
    GameObject rope;


    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        InitializePoints();
        CreateSphere();
        DefineConstraint();
        InitializeRope();
    }

    // Update is called once per frame
    void Update()
    {
        // compute force
        ComputeForce();
        // change states
        UpdateState();
        // apply constraints
        ApplyContraints();
        rope.GetComponent<LineRenderer>().positionCount = pointNumber;
        rope.GetComponent<LineRenderer>().SetPositions(points);
    }

    void InitializePoints()
    {
        points = new Vector3[pointNumber];
        velocities = new Vector3[pointNumber];
        forces = new Vector3[pointNumber];
        edges = new Edge[pointNumber - 1];
        for (int i = 0; i < pointNumber; i++)
        {
            points[i] = transform.forward * distance * i;
            velocities[i] = Vector3.zero;
            forces[i] = Vector3.zero;
            if (i < pointNumber - 1)
            {
                edges[i] = new Edge(i, i + 1);
            }
        }
    }

    void InitializeRope()
    {
        rope = new GameObject();
        rope.transform.name = "Rope";
        rope.AddComponent<LineRenderer>();
        rope.GetComponent<LineRenderer>().material = material;
        rope.GetComponent<LineRenderer>().startWidth = 10;
        rope.GetComponent<LineRenderer>().endWidth = 10;
    }

    void DefineConstraint()
    {
        constraints = new Constraint[1];
        constraints[0] = new Constraint(0, new Vector3(-3, 0, -7), Vector3.zero);
    }

    void ComputeForce()
    {
        // gravity
        for (int i = 0; i < forces.Length; i++)
        {
            forces[i] = mass * gravity;
        }
        //spring
        for(int i = 0; i < edges.Length; i++)
        {
            int p1 = edges[i].first;
            int p2 = edges[i].second;
            float d = Vector3.Distance(points[p1], points[p2]);
            Vector3 dir = (points[p1] - points[p2]).normalized;
            Vector3 spring = -stifness * (d - restLength) * dir;
            forces[p1] += spring;
            forces[p2] -= spring;
        }
        //// damping force
        for (int i = 0; i < edges.Length; i++)
        {
            int p1 = edges[i].first;
            int p2 = edges[i].second;
            Vector3 damping = -dampingCoefficient * (velocities[p1] - velocities[p2]);
            forces[p1] += damping;
            forces[p2] -= damping;
        }
        //air drag
        for (int i = 0; i < forces.Length; i++)
        {
            forces[i] += -dragCoefficient * velocities[i];
        }
    }

    void UpdateState()
    {
        for (int i = 0; i < points.Length; i++)
        {
            Vector3 acceleration = forces[i] / mass;
            Vector3 newVel = velocities[i] + Time.deltaTime * acceleration;
            Vector3 newPos = points[i] + Time.deltaTime * newVel;
            velocities[i] = newVel;
            points[i] = newPos;
            spheres[i].transform.position = points[i];
        }
    }

    void ApplyContraints()
    {
        for (int i = 0; i < constraints.Length; i++)
        {
            points[constraints[i].index] = constraints[i].position;
            velocities[constraints[i].index] = constraints[i].velocity;
            spheres[i].transform.position = points[i];
        }
    }

    void CreateSphere()
    {
        spheres = new GameObject[points.Length];
        for (int i = 0; i < points.Length; i++)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = points[i];
            sphere.transform.parent = transform;
            sphere.transform.localScale = new Vector3(scale, scale, scale);
            sphere.GetComponent<MeshRenderer>().material = mat;
            spheres[i] = sphere;
        }
    }

    Mesh UpdateMesh()
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[2 * points.Length];
        Vector2[] uvs = new Vector2[2 * points.Length];
        int[] triangles = new int[3 * 2 * (points.Length - 1)];
        int count = 0;
        int triCount = 0;
        for (int i = 0; i < points.Length; i+=1)
        {
            Vector3 dir;
            if (i == 0)
            {
                dir = points[i + 1] - points[i];
            }
            else if (i == points.Length - 1)
            {
                dir = points[i] - points[i - 1];
            }
            else
            {
                dir = points[i + 1] - points[i - 1];
            }
            dir.Normalize();
            vertices[count] = points[i] + width / 2 * new Vector3(-dir.z, 0, dir.x);
            vertices[count + 1] = points[i] - width / 2 * new Vector3(-dir.z, 0, dir.x);
            float completion = i / (float)(points.Length - 1);
            uvs[count] = new Vector2(0, completion);
            uvs[count + 1] = new Vector2(1, completion);

            if (i < points.Length - 1)
            {
                triangles[triCount] = count;
                triangles[triCount + 1] = count + 2;
                triangles[triCount + 2] = count + 1;

                triangles[triCount + 3] = count + 1;
                triangles[triCount + 4] = count + 2;
                triangles[triCount + 5] = count + 3;
            }
            triCount += 6;
            count += 2;
        }
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

    public class Edge
    {
        public int first;
        public int second;
        public Edge(int first, int second)
        {
            this.first = first;
            this.second = second;
        }
    }
    
    public class Constraint
    {
        public int index;
        public Vector3 position;
        public Vector3 velocity;
        public Constraint(int index, Vector3 position, Vector3 velocity)
        {
            this.index = index;
            this.position = position;
            this.velocity = velocity;
        }
    }
}
