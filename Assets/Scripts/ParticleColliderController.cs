using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleColliderController : MonoBehaviour {

	void Awake ()
    {
        GameObject particleCollider = new GameObject();
        particleCollider.transform.position = Vector3.zero;
        particleCollider.transform.rotation = Quaternion.identity;
        particleCollider.transform.SetParent(gameObject.transform, false);

        List<BoxCollider2D> box2Ds = new List<BoxCollider2D>();
        GetComponents<BoxCollider2D>(box2Ds);
        foreach (BoxCollider2D box2D in box2Ds)
        {
            if (!box2D.isTrigger)
            {
                BoxCollider box3D = particleCollider.AddComponent<BoxCollider>();
                box3D.center = new Vector3(box2D.offset.x, box2D.offset.y, 0f);
                box3D.size = new Vector3(box2D.size.x, box2D.size.y, 1f);
                box3D.isTrigger = false;
            }        
        }

        List<CircleCollider2D> circle2Ds = new List<CircleCollider2D>();
        GetComponents<CircleCollider2D>(circle2Ds);
        foreach (CircleCollider2D circle2D in circle2Ds)
        {
            if (!circle2D.isTrigger)
            {
                SphereCollider sphere3D = particleCollider.AddComponent<SphereCollider>();
                sphere3D.center = new Vector3(circle2D.offset.x, circle2D.offset.y, 0f);
                sphere3D.radius = circle2D.radius;
                sphere3D.isTrigger = false;
            }
        }

        List<PolygonCollider2D> polygon2Ds = new List<PolygonCollider2D>();
        GetComponents<PolygonCollider2D>(polygon2Ds);
        foreach (PolygonCollider2D polygon2D in polygon2Ds)
        {
            if (!polygon2D.isTrigger)
            {

                MeshCollider mesh3D = particleCollider.AddComponent<MeshCollider>();
                Mesh geometry = new Mesh();
                int pointCount = polygon2D.GetTotalPointCount();
                Vector3[] vertices = new Vector3[2 * pointCount];
                int triangleCount = 2 * pointCount;
                int[] triangles = new int[3 * triangleCount];

                int vertexCount = 0;
                for (int pathIdx = 0; pathIdx < polygon2D.pathCount; ++pathIdx)
                {
                    Vector2[] path = polygon2D.GetPath(pathIdx);
                    int vertex2DIdx, vertex3DIdx, triangle3DIdx;
                    for (vertex2DIdx = 0; vertex2DIdx < path.Length - 1; ++vertex2DIdx)
                    {
                        vertex3DIdx = vertexCount + 2 * vertex2DIdx;
                        vertices[vertex3DIdx] = new Vector3(path[vertex2DIdx].x, path[vertex2DIdx].y, 0.5f);
                        vertices[vertex3DIdx + 1] = new Vector3(path[vertex2DIdx].x, path[vertex2DIdx].y, -0.5f);
                        triangle3DIdx = 3 * vertex3DIdx;
                        triangles[triangle3DIdx] = vertex3DIdx;
                        triangles[triangle3DIdx + 1] = vertex3DIdx + 1;
                        triangles[triangle3DIdx + 2] = vertex3DIdx + 2;
                        triangles[triangle3DIdx + 3] = vertex3DIdx + 1;
                        triangles[triangle3DIdx + 3] = vertex3DIdx + 3;
                        triangles[triangle3DIdx + 3] = vertex3DIdx + 2;
                    }
                    ++vertex2DIdx;
                    vertex3DIdx = vertexCount + 2 * vertex2DIdx;
                    vertices[vertex3DIdx] = new Vector3(path[vertex2DIdx].x, path[vertex2DIdx].y, 0.5f);
                    vertices[vertex3DIdx + 1] = new Vector3(path[vertex2DIdx].x, path[vertex2DIdx].y, -0.5f);
                    triangle3DIdx = 3 * vertex3DIdx;
                    triangles[triangle3DIdx] = vertex3DIdx;
                    triangles[triangle3DIdx + 1] = vertex3DIdx + 1;
                    triangles[triangle3DIdx + 2] = vertexCount;
                    triangles[triangle3DIdx + 3] = vertex3DIdx + 1;
                    triangles[triangle3DIdx + 3] = vertexCount + 1;
                    triangles[triangle3DIdx + 3] = vertexCount;

                    vertexCount += 2 * path.Length;
                }
                geometry.vertices = vertices;
                geometry.triangles = triangles;
                mesh3D.sharedMesh = geometry;
                mesh3D.isTrigger = false;
            }
        }
    }
}
