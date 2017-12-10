using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControl : MonoBehaviour {

    public float lightRadius = 10;
    [Range(0, 4)]
    public float lightFalloff = 0.2f;
    public ContactFilter2D layerfilter;
    public Color LightColor; 

    private Vector3 center;
    private CircleCollider2D colliderCircle;

	// Use this for initialization
	void Start () {
        center = transform.position;
        colliderCircle = gameObject.GetComponent<CircleCollider2D>();
	}

	// Update is called once per frame
	void Update () {
        center = transform.position;

        // Detect objects within the light radius
        Collider2D[] results = new Collider2D[10];
        int collisions = Physics2D.OverlapCircle(center, lightRadius, layerfilter, results);
        // Create list of points to send ray casts to
        List<Vector2> rayCastPoints = new List<Vector2>();
        // Add points at edge of light radius
        rayCastPoints.Add(new Vector2(center.x + lightRadius, center.y + lightRadius));
        rayCastPoints.Add(new Vector2(center.x - lightRadius, center.y + lightRadius));
        rayCastPoints.Add(new Vector2(center.x + lightRadius, center.y - lightRadius));
        rayCastPoints.Add(new Vector2(center.x - lightRadius, center.y - lightRadius));
        
        // For every object hit, push the corners into raycast points
        foreach (Collider2D collision in results)
        {
            if (collision != null)
            {
                // BoxCollider2D
                if (collision.GetType() == typeof(BoxCollider2D))
                {
                    rayCastPoints.AddRange(get2DBoxPoints((BoxCollider2D)collision));
                }
                // PolygonCollider2D
                else if (collision.GetType() == typeof(PolygonCollider2D))
                {
                    rayCastPoints.AddRange(getPolygonPoints((PolygonCollider2D)collision));
                }
            }
        }
        // Send ray casts to each corner
        List<Vector3> vertices = new List<Vector3>();
        foreach (Vector2 point in rayCastPoints)
        {
            rayCastToPoint(point, ref vertices);
        }
        // Order Raycasts in clockwise order
        vertices.Sort(sortAngleClockwise);
        // Push in the center
        vertices.Add(center);

        // build light mesh
        Mesh lightMesh = createLightMesh(vertices);

        // Set materials's properties
        MeshRenderer rend = GetComponent<MeshRenderer>();
        rend.material.SetColor("_Color", LightColor);
        rend.material.SetFloat("_Radius", lightRadius - 2);
        rend.material.SetVector("_LightPosition", center);
        rend.material.SetFloat("_Falloff", lightFalloff);
    }

    // Return the corners of a BoxCollider2D in World Coordinates
    List<Vector2> get2DBoxPoints(BoxCollider2D box) {
        Transform parent = box.gameObject.GetComponent<Transform>();
        List<Vector2> results = new List<Vector2>();
        Vector2 offset = new Vector2(parent.localScale.x * box.size.x, parent.localScale.y * box.size.y) / 2;
        Vector2 boxCenter = box.transform.position;
        // Find the sin and cos of the rotation
        float sinAngle = Mathf.Sin((box.transform.eulerAngles.z * Mathf.Deg2Rad));
        float cosAngle = Mathf.Cos((box.transform.eulerAngles.z * Mathf.Deg2Rad));

        // Find Local Corners
        results.Add(new Vector2(offset.x, offset.y));
        results.Add(new Vector2(offset.x, -offset.y));
        results.Add(new Vector2(-offset.x, offset.y));
        results.Add(new Vector2(-offset.x, -offset.y));
        // Rotate about the center
        results[0] = new Vector2(results[0].x * cosAngle - results[0].y * sinAngle, results[0].x * sinAngle + results[0].y * cosAngle);
        results[1] = new Vector2(results[1].x * cosAngle - results[1].y * sinAngle, results[1].x * sinAngle + results[1].y * cosAngle);
        results[2] = new Vector2(results[2].x * cosAngle - results[2].y * sinAngle, results[2].x * sinAngle + results[2].y * cosAngle);
        results[3] = new Vector2(results[3].x * cosAngle - results[3].y * sinAngle, results[3].x * sinAngle + results[3].y * cosAngle);

        // Transform to world positions.
        results[0] = new Vector2(results[0].x + boxCenter.x, results[0].y + boxCenter.y);
        results[1] = new Vector2(results[1].x + boxCenter.x, results[1].y + boxCenter.y);
        results[2] = new Vector2(results[2].x + boxCenter.x, results[2].y + boxCenter.y);
        results[3] = new Vector2(results[3].x + boxCenter.x, results[3].y + boxCenter.y);

        return results;
    }

    // Return the corners of a PolygonCollider2D in World Coordinates
    List<Vector2> getPolygonPoints(PolygonCollider2D polygon)
    {
        Transform parent = polygon.gameObject.GetComponent<Transform>();
        Vector2[] points = polygon.points;
        List<Vector2> results = new List<Vector2>();
        // Convert points from local to world coordinates.
        foreach (Vector2 point in points)
        {
            results.Add(parent.TransformPoint(point));
        }

        return results;
    }

    // Sorts points based on angle in a clockwise direction
    int sortAngleClockwise(Vector3 a, Vector3 b)
    {
        Vector2 lineA = new Vector2(center.x - a.x, center.y - a.y);
        Vector2 lineB = new Vector2(center.x - b.x, center.y - b.y);

        float angleA = Mathf.Rad2Deg * Mathf.Atan(lineA.y / lineA.x);
        float angleB = Mathf.Rad2Deg * Mathf.Atan(lineB.y / lineB.x);

        if (lineA.x < 0 && lineA.y >= 0) angleA += 180;
        if (lineA.y < 0 && lineA.x <= 0) angleA += 180;
        if (lineA.x > 0 && lineA.y < 0) angleA += 360;

        if (lineB.x < 0 && lineB.y >= 0) angleB += 180;
        if (lineB.y < 0 && lineB.x <= 0) angleB += 180;
        if (lineB.x > 0 && lineB.y < 0) angleB += 360;

        if (angleA > angleB) return -1;
        else if (angleB > angleA) return 1;
        else return 0;
    }

    // Build light mesh from a list of vertices in World Coordinates.
    Mesh createLightMesh(List<Vector3> verts)
    {
        // Create new mesh and set Meshfilter to it
        Mesh result = new Mesh();
        GetComponent<MeshFilter>().mesh = result;

        // Push in vertices, converting them from world to local coordinates.
        Vector3[] vertices = new Vector3[verts.Count];
        for (int i = 0; i < verts.Count; i++)
        {
            vertices[i] = transform.InverseTransformPoint(verts[i]); 
        }
        // Create triangle array
        int[] triangles = new int[(verts.Count - 1) * 3];
        for (int i = 0; i < verts.Count - 1; i++)
        {
            triangles[i * 3] = i;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = verts.Count - 1;
        }
        // Wrap the end to the beginning
        triangles[(verts.Count - 1) * 3 - 3] = verts.Count - 2;
        triangles[(verts.Count - 1) * 3 - 2] = 0;
        triangles[(verts.Count - 1) * 3 - 1] = verts.Count - 1;

        result.vertices = vertices;
        result.triangles = triangles;

        return result;
    }

    // Rotate a point around the origin, by radians
    Vector2 rotatePointbyRadians(Vector2 point, float radians)
    {
        Vector2 result = new Vector2();
        result.x = point.x * Mathf.Cos(radians) - point.y * Mathf.Sin(radians);
        result.y = point.x * Mathf.Sin(radians) + point.y * Mathf.Cos(radians);
        return result;
    }

    // Casts three rays to a point, returns all the hits or misses
    void rayCastToPoint(Vector2 point, ref List<Vector3> vertices)
    {
        RaycastHit2D[] target = new RaycastHit2D[1];
        // Raycast fro the center to the point, and to the left and to the right
        Vector2 direction = point - new Vector2(center.x, center.y);
        Vector2 left = rotatePointbyRadians(direction, 0.000001f);
        Vector2 right = rotatePointbyRadians(direction, -0.000001f);
        // Aim for point
        if (colliderCircle.Raycast(direction, layerfilter, target, lightRadius) == 0)
        {
            // if no collision, push in a point on the radius
            direction.Normalize();
            vertices.Add(new Vector3((direction.x * lightRadius) + center.x, (direction.y * lightRadius) + center.y, center.z));
        }
        else
        {
            // if collision, push in the collision point
            vertices.Add(new Vector3(target[0].point.x, target[0].point.y, center.z));
        }
        // Aim left
        if (colliderCircle.Raycast(left, layerfilter, target, lightRadius) == 0)
        {
            left.Normalize();
            vertices.Add(new Vector3((left.x * lightRadius) + center.x, (left.y * lightRadius) + center.y, center.z));
        }
        else
        {
            vertices.Add(new Vector3(target[0].point.x, target[0].point.y, center.z));
        }
        // Aim Right
        if (colliderCircle.Raycast(right, layerfilter, target, lightRadius) == 0)
        {
            right.Normalize();
            vertices.Add(new Vector3((right.x * lightRadius) + center.x, (right.y * lightRadius) + center.y, center.z));
        }
        else
        {
            vertices.Add(new Vector3(target[0].point.x, target[0].point.y, center.z));
        }
    }
}