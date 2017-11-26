using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControl : MonoBehaviour {

    public float lightRadius = 10;
    public ContactFilter2D layerfilter;

    private Vector3 center;
    private CircleCollider2D colliderCircle;

	// Use this for initialization
	void Start () {
        center = transform.position;
        colliderCircle = gameObject.GetComponent<CircleCollider2D>();
	}

	// Update is called once per frame
	void Update () {
        // remove this later, it is for debug purposes
        //transform.Translate(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        //center = transform.position;


        // Detect Collisions with objects around a radius
        Collider2D[] results = new Collider2D[10];
        int collisions = Physics2D.OverlapCircle(center, lightRadius, layerfilter, results);
        // Create list of points to send ray casts to
        List<Vector2> rayCastPoints = new List<Vector2>();
        // Add points at edge of light radius
        rayCastPoints.Add(new Vector2(center.x + lightRadius, center.y + lightRadius));
        rayCastPoints.Add(new Vector2(center.x - lightRadius, center.y + lightRadius));
        rayCastPoints.Add(new Vector2(center.x + lightRadius, center.y - lightRadius));
        rayCastPoints.Add(new Vector2(center.x - lightRadius, center.y - lightRadius));
        foreach (Collider2D collision in results)
        {
            if (collision != null)
            {
                if (collision.GetType() == typeof(BoxCollider2D))
                {
                    rayCastPoints.AddRange(get2DBoxPoints((BoxCollider2D)collision));
                }
            }
        }
        // Send ray casts to each edge
        List<Vector3> vertices = new List<Vector3>();
        foreach (Vector2 point in rayCastPoints)
        {
            rayCastToPoint(point, ref vertices);
        }
        // Order Raycasts in counter-clockwise order
        vertices.Sort(sortAngleClockwise);
        vertices.Add(center);

        Color[] colors = { Color.red, Color.yellow, Color.green, Color.blue, Color.magenta, Color.cyan, Color.white, Color.magenta };
        // draw polygon
        Mesh lightMesh = createLightMesh(vertices);
        /*for (int i = 0; i < vertices.Count; i++)
        {
            Debug.DrawLine(center, vertices[i], colors[i % 8], 0.1f, false);
        }*/

        // Set polygon's properties

        // Light scene
	}

    List<Vector2> get2DBoxPoints(BoxCollider2D box) {
        List<Vector2> results = new List<Vector2>();
        Vector2 offset = box.size / 2;
        Vector2 center = box.offset;
        float sinAngle = Mathf.Sin(box.transform.rotation.z);
        float cosAngle = Mathf.Cos(box.transform.rotation.z);

        // Find Local Corners
        results.Add(new Vector2(center.x + offset.x, center.y + offset.y));
        results.Add(new Vector2(center.x - offset.x, center.y + offset.y));
        results.Add(new Vector2(center.x - offset.x, center.y - offset.y));
        results.Add(new Vector2(center.x + offset.x, center.y - offset.y));
        // Rotate about the center
        results[0] = new Vector2(results[0].x * cosAngle - results[0].y * sinAngle, results[0].x * sinAngle + results[0].y * cosAngle);
        results[1] = new Vector2(results[1].x * cosAngle - results[1].y * sinAngle, results[1].x * sinAngle + results[1].y * cosAngle);
        results[2] = new Vector2(results[2].x * cosAngle - results[2].y * sinAngle, results[2].x * sinAngle + results[2].y * cosAngle);
        results[3] = new Vector2(results[3].x * cosAngle - results[3].y * sinAngle, results[3].x * sinAngle + results[3].y * cosAngle);
        // Transform to world positions.
        results[0] = box.transform.TransformPoint(results[0]);
        results[1] = box.transform.TransformPoint(results[1]);
        results[2] = box.transform.TransformPoint(results[2]);
        results[3] = box.transform.TransformPoint(results[3]);

        return results;
    }

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

    Mesh createLightMesh(List<Vector3> verts)
    {
        Mesh result = new Mesh();
        GetComponent<MeshFilter>().mesh = result;

        Vector3[] vertices = new Vector3[verts.Count];
        Color[] colors = new Color[verts.Count];
        for (int i = 0; i < verts.Count; i++)
        {
            Color color = new Color();
            color = Color.clear;
            color.a = 1 - verts[i].magnitude / lightRadius;
            vertices[i] = verts[i];
            colors[i] = color;
        }
        colors[verts.Count - 1] = Color.blue; // set this so that it can be picked

        int[] triangles = new int[(verts.Count - 1) * 3];
        for (int i = 0; i < verts.Count - 1; i++)
        {
            triangles[i * 3] = i;
            triangles[i * 3 + 1] = i + 1;
            triangles[i * 3 + 2] = verts.Count - 1;
        }
        triangles[(verts.Count - 1) * 3 - 3] = verts.Count - 2;
        triangles[(verts.Count - 1) * 3 - 2] = 0;
        triangles[(verts.Count - 1) * 3 - 1] = verts.Count - 1;

        result.vertices = vertices;
        result.triangles = triangles;
        result.colors = colors;

        return result;
    }

    Vector2 rotatePointbyRadians(Vector2 point, float radians)
    {
        Vector2 result = new Vector2();
        result.x = point.x * Mathf.Cos(radians) - point.y * Mathf.Sin(radians);
        result.y = point.x * Mathf.Sin(radians) + point.y * Mathf.Cos(radians);
        return result;
    }

    void rayCastToPoint(Vector2 point, ref List<Vector3> vertices)
    {
        // Ray cast from center to point
        RaycastHit2D[] target = new RaycastHit2D[1];
        Vector2 direction = point - new Vector2(center.x, center.y);
        Vector2 left = rotatePointbyRadians(direction, 0.00001f);
        Vector2 right = rotatePointbyRadians(direction, -0.00001f);
        // Push in whatever I'm going towards, unless I hit something
        // Aim for point
        if (colliderCircle.Raycast(direction, layerfilter, target, lightRadius) == 0)
        {
            point.Normalize();
            vertices.Add(new Vector3(point.x * lightRadius, point.y * lightRadius, center.z));
        }
        else
        {
            vertices.Add(new Vector3(target[0].point.x, target[0].point.y, center.z));
        }
        // Aim left
        if (colliderCircle.Raycast(left, layerfilter, target, lightRadius) == 0)
        {
            left.Normalize();
            vertices.Add(new Vector3(left.x * lightRadius, left.y * lightRadius, center.z));
        }
        else
        {
            vertices.Add(new Vector3(target[0].point.x, target[0].point.y, center.z));
        }
        // Aim Right
        if (colliderCircle.Raycast(right, layerfilter, target, lightRadius) == 0)
        {
            right.Normalize();
            vertices.Add(new Vector3(right.x * lightRadius, right.y * lightRadius, center.z));
        }
        else
        {
            vertices.Add(new Vector3(target[0].point.x, target[0].point.y, center.z));
        }
    }
}