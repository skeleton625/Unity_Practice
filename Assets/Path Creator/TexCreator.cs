using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexCreator : MonoBehaviour
{
    [SerializeField]
    private GameObject NewRiver;
    [SerializeField]
    private float texWidth;
    private TerrainInfo FieldInfo;

    public void UpdateTexture()
    {
        FieldInfo = TerrainInfo.instance;
        Path path = GetComponent<PathCreator>().path;
        GameObject River = Instantiate(NewRiver, Vector3.zero, Quaternion.identity);
        River.transform.position = new Vector3(0, -1f, 0);

        Mesh RiverMesh = CreateTexMesh();
        River.GetComponent<MeshFilter>().mesh = RiverMesh;
        River.GetComponent<MeshCollider>().sharedMesh = RiverMesh;

        gameObject.SetActive(false);
    }

    private Mesh CreateTexMesh()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector3[] verts = new Vector3[path.NumPoints * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int numTris = 2 * (path.NumPoints - 1);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < path.NumPoints; i+=3)
        {
            Vector3 forward = Vector3.zero;
            if (i < path.NumPoints - 1)
                forward += path[(i + 1) % path.NumPoints] - path[i];
            if (i > 0)
                forward += path[i] - path[(i - 1 + path.NumPoints) % path.NumPoints];

            forward.Normalize();
            Vector3 left = new Vector3(forward.z, forward.y, -forward.x);

            verts[vertIndex] = SetMeshHeights(path[i] - left * texWidth);
            verts[vertIndex + 1] = SetMeshHeights(path[i] + left * texWidth);

            float completionPercent = i / (float)(path.NumPoints - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector2(1, v);
            uvs[vertIndex + 1] = new Vector2(0, v);

            if (i < path.NumPoints - 1)
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
        float h = FieldInfo.Depth * FieldInfo[z, x];

        if (h <= FieldInfo.HeightLimit)
            h = point.y;
        Vector3 pos = new Vector3(x, h, z);

        /*
        GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        g.transform.position = pos;
        g.transform.localScale = new Vector3(2f, 2f, 2f);
        */

        return pos;
    }
}
