using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleColliderController : MonoBehaviour {

    static float circleSAG = 0.05f;

	void Awake ()
    {
        GameObject particleCollider = new GameObject("ParticleCollider");
        particleCollider.transform.position = Vector3.zero;
        particleCollider.transform.rotation = Quaternion.identity;
        particleCollider.transform.SetParent(gameObject.transform, false);

        List<BoxCollider2D> box2Ds = new List<BoxCollider2D>();
        GetComponents<BoxCollider2D>(box2Ds);
        foreach (BoxCollider2D box2D in box2Ds)
        {
            if (!box2D.isTrigger)
            {
                MakeCollider(particleCollider, MakeBoundary(box2D));
            }        
        }

        List<CircleCollider2D> circle2Ds = new List<CircleCollider2D>();
        GetComponents<CircleCollider2D>(circle2Ds);
        foreach (CircleCollider2D circle2D in circle2Ds)
        {
            if (!circle2D.isTrigger)
            {
                CapsuleCollider capsule3D = particleCollider.AddComponent<CapsuleCollider>();
                capsule3D.center = new Vector3(circle2D.offset.x, circle2D.offset.y, 0f);
                capsule3D.radius = circle2D.radius;
                capsule3D.height = 1f + 2 * circle2D.radius;
                capsule3D.direction = 2;
                capsule3D.isTrigger = false;
            }
        }

        List<PolygonCollider2D> polygon2Ds = new List<PolygonCollider2D>();
        GetComponents<PolygonCollider2D>(polygon2Ds);
        foreach (PolygonCollider2D polygon2D in polygon2Ds)
        {
            if (!polygon2D.isTrigger)
            {
                for (int pathIdx = 0; pathIdx < polygon2D.pathCount; ++pathIdx)
                {
                    MakeCollider(particleCollider, polygon2D.GetPath(pathIdx));
                }
            }
        }
    }

    Vector2[] MakeBoundary(BoxCollider2D box)
    {
        Vector2[] boundary = new Vector2[4];
        boundary[0] = box.offset + box.size.x * 0.5f * Vector2.right + box.size.y * 0.5f * Vector2.up;
        boundary[1] = box.offset - box.size.x * 0.5f * Vector2.right + box.size.y * 0.5f * Vector2.up;
        boundary[2] = box.offset - box.size.x * 0.5f * Vector2.right - box.size.y * 0.5f * Vector2.up;
        boundary[3] = box.offset + box.size.x * 0.5f * Vector2.right - box.size.y * 0.5f * Vector2.up;
        return boundary;
    }

    Vector2[] MakeBoundary(CircleCollider2D circle)
    {
        int pointCount = (circleSAG < circle.radius) ?  Mathf.CeilToInt(Mathf.PI / Mathf.Acos(1f - circleSAG / circle.radius)) : 3;
        Vector2[] boundary = new Vector2[pointCount];
        for (int pointIdx = 0; pointIdx < pointCount; ++pointIdx)
        {
            float idx = pointIdx;
            boundary[pointIdx] = circle.offset + circle.radius * Vector2.right.Rotate(2f * Mathf.PI * idx / pointCount);
        }
        return boundary;
    }

    void MakeCollider(GameObject obj, Vector2[] points)
    {
        MeshCollider mesh3D = obj.AddComponent<MeshCollider>();
        Mesh geometry = new Mesh();
        int pointCount = points.Length;
        Vector3[] vertices = new Vector3[2 * pointCount];
        int triangleCount = 2 * pointCount;
        int[] triangles = new int[3 * triangleCount];

        int vertex2DIdx, vertex3DIdx, triangle3DIdx;
        for (vertex2DIdx = 0; vertex2DIdx < pointCount - 1; ++vertex2DIdx)
        {
            vertex3DIdx = 2 * vertex2DIdx;
            vertices[vertex3DIdx] = new Vector3(points[vertex2DIdx].x, points[vertex2DIdx].y, 0.5f);
            vertices[vertex3DIdx + 1] = new Vector3(points[vertex2DIdx].x, points[vertex2DIdx].y, -0.5f);
            triangle3DIdx = 3 * vertex3DIdx;
            triangles[triangle3DIdx] = vertex3DIdx;
            triangles[triangle3DIdx + 1] = vertex3DIdx + 1;
            triangles[triangle3DIdx + 2] = vertex3DIdx + 2;
            triangles[triangle3DIdx + 3] = vertex3DIdx + 1;
            triangles[triangle3DIdx + 4] = vertex3DIdx + 3;
            triangles[triangle3DIdx + 5] = vertex3DIdx + 2;
        }
        vertex3DIdx = 2 * vertex2DIdx;
        vertices[vertex3DIdx] = new Vector3(points[vertex2DIdx].x, points[vertex2DIdx].y, 0.5f);
        vertices[vertex3DIdx + 1] = new Vector3(points[vertex2DIdx].x, points[vertex2DIdx].y, -0.5f);
        triangle3DIdx = 3 * vertex3DIdx;
        triangles[triangle3DIdx] = vertex3DIdx;
        triangles[triangle3DIdx + 1] = vertex3DIdx + 1;
        triangles[triangle3DIdx + 2] = 0;
        triangles[triangle3DIdx + 3] = vertex3DIdx + 1;
        triangles[triangle3DIdx + 4] = 1;
        triangles[triangle3DIdx + 5] = 0;

        geometry.vertices = vertices;
        geometry.triangles = triangles;
        mesh3D.sharedMesh = geometry;
        mesh3D.isTrigger = false;
    }
}
