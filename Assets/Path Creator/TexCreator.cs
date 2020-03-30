using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TexCreator : MonoBehaviour
{
    private enum PathSpace { xyz, xy, xz };

    [SerializeField]
    private float texWidth;
    [SerializeField, Range(.05f, 1.5f)]
    private float spacing;
    public bool autoUpdate;

    public void UpdateTexture()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector3[] points = path.CalculateEvenlySpacedPoitns(spacing);
        GetComponent<MeshFilter>().mesh = CreateTexMesh(points);
    }

    Mesh CreateTexMesh(Vector3[] points)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        int[] tris = new int[2 * (points.Length - 1) * 3];
        int vertIndex = 0, triIndex = 0;

        for(int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if(i < points.Length - 1)
            {
                forward += points[i + 1] - points[i];
            }
            if(i > 0)
            {
                forward -= points[i] - points[i - 1];
            }
            forward.Normalize();

            Vector3 left = TransformDirection(points[i], transform, PathSpace.xyz);
            verts[vertIndex] = points[i] + left * texWidth * .5f;
            verts[vertIndex + 1] = points[i] - left * texWidth * .5f;
            Debug.Log(verts[vertIndex] + " " + verts[vertIndex + 1]);

            if (i < points.Length - 1)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = vertIndex + 2;
                tris[triIndex + 2] = vertIndex + 1;
                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = vertIndex + 2;
                tris[triIndex + 5] = vertIndex + 3;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        return mesh;
    }

    private Vector3 TransformDirection(Vector3 p, Transform t, PathSpace space)
    {
        Quaternion constrainedRot = t.rotation;
        ConstrainRot(ref constrainedRot, space);
        return constrainedRot * p;
    }

    private void ConstrainRot(ref Quaternion rot, PathSpace space)
    {
        if (space == PathSpace.xy)
        {
            var eulerAngles = rot.eulerAngles;
            if (eulerAngles.x != 0 || eulerAngles.y != 0)
            {
                rot = Quaternion.AngleAxis(eulerAngles.z, Vector3.forward);
            }
        }
        else if (space == PathSpace.xz)
        {
            var eulerAngles = rot.eulerAngles;
            if (eulerAngles.x != 0 || eulerAngles.z != 0)
            {
                rot = Quaternion.AngleAxis(eulerAngles.y, Vector3.up);
            }
        }
    }
}
