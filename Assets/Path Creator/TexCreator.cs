﻿using System.Collections;
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
    public bool autoUpdate;

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

            verts[vertIndex] = points[i] - left * texWidth * .5f;
            verts[vertIndex + 1] = points[i] + left * texWidth * .5f;

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
        mesh.uv = uvs;
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
