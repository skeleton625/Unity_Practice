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
    private GameObject MainRiver;

    private void Awake()
    {
        MainRiver = GameObject.Find("River");
    }

    public void UpdateTexture(Path path)
    {
        FieldInfo = TerrainInfo.instance;
        GameObject River = Instantiate(NewRiver, Vector3.zero, Quaternion.identity);
        River.transform.position = new Vector3(0, -1f, 0);

        Mesh RiverMesh = CreateTexMesh(path);
        River.GetComponent<MeshFilter>().mesh = RiverMesh;
        River.GetComponent<MeshCollider>().sharedMesh = RiverMesh;

        gameObject.SetActive(false);
        River.transform.parent = MainRiver.transform;
    }

    private Mesh CreateTexMesh(Path path)
    {
        Vector3[] verts = new Vector3[path.NumPoints * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[2 * (path.NumPoints - 1) * 3];
        int vertIndex = 0, triIndex = 0;

        for (int i = 0; i < path.NumPoints; i+=3)
        {
            Vector3 forward = Vector3.zero;
            if (i < path.NumPoints - 1)
                forward += path[(i + 1) % path.NumPoints] - path[i];
            if (i > 0)
                forward += path[i] - path[(i - 1 + path.NumPoints) % path.NumPoints];

            forward.Normalize();
            Vector3 left = new Vector3(forward.z, forward.y, -forward.x);

            verts[vertIndex] = FieldInfo.SetRealHeight(path[i] - left * texWidth);
            verts[vertIndex + 1] = FieldInfo.SetRealHeight(path[i] + left * texWidth);

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

    public void SetCombineAllRiverMesh()
    {
        MeshFilter[] meshes = MainRiver.transform.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshes.Length];

        for(int i = 0; i < meshes.Length; i++)
        {
            combine[i].mesh = meshes[i].sharedMesh;
            combine[i].transform = meshes[i].transform.localToWorldMatrix;
            meshes[i].gameObject.SetActive(false);
        }

        MeshFilter RiverMesh = MainRiver.GetComponent<MeshFilter>();
        RiverMesh.mesh = new Mesh();
        RiverMesh.mesh.CombineMeshes(combine);
        MainRiver.SetActive(true);
    }
}
