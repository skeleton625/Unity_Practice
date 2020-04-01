using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexCreator : MonoBehaviour
{
    private enum PathSpace { xyz, xy, xz };

    [SerializeField]
    private GameObject NewMesh;
    [SerializeField]
    private float texWidth;
    [SerializeField, Range(.05f, 1.5f)]
    private float spacing;
    [SerializeField]
    private TerrainInfo FieldInfo;

    public void UpdateTexture()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector3[] points = path.CalculateEvenlySpacedPoitns(spacing);
        GameObject RiverMesh = Instantiate(NewMesh, Vector3.zero, Quaternion.identity);
        RiverMesh.transform.position = new Vector3(0, -1.1f, 0);
        RiverMesh.GetComponent<MeshFilter>().mesh = CreateTexMesh(points);
    }

    private Mesh CreateTexMesh(Vector3[] points)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int numTris = 2 * (points.Length - 1);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < points.Length - 1)
            {
                forward += points[(i + 1) % points.Length] - points[i];
            }
            if (i > 0)
            {
                forward += points[i] - points[(i - 1 + points.Length) % points.Length];
            }

            forward.Normalize();
            Vector3 left = new Vector3(forward.z, forward.y, -forward.x);

            verts[vertIndex] = SetMeshHeights(points[i] - left * texWidth);
            verts[vertIndex + 1] = SetMeshHeights(points[i] + left * texWidth);

            float completionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector2(1, v);
            uvs[vertIndex + 1] = new Vector2(0, v);

            if (i < points.Length - 1)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.uv = uvs;
        return mesh;
    }

    private Vector3 SetMeshHeights(Vector3 point)
    {
        int x = (int)point.x;
        int z = (int)point.z;
        Vector3 pos = new Vector3(x, FieldInfo.Depth * FieldInfo[z, x], z);

        /*
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.transform.position = pos;
        g.transform.localScale = new Vector3(2f, 2f, 2f);
        */

        return pos;
    }
}
