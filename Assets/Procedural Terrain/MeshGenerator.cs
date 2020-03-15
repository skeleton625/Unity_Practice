using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 해당 스크립트는 MeshFilter 컴포넌트가 필요함을 알리는 코드
[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    // Terrain 크기 변수들
    [SerializeField]
    private int Xsize, Zsize;
    // Noise 정도 변수
    [SerializeField]
    private float NoiseForce;

    // Debuging 변수들
    [SerializeField]
    private bool IsNoise;
    private WaitForSeconds DebugTimer;

    // Mesh 객체
    private Mesh TerrainMesh;
    // triangle, vertex 변수들
    private int[] Mtriangles;
    private Vector3[] Mvertices;

    private void Start()
    {
        TerrainMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = this.TerrainMesh;

        CreateShape();
        UpdateTerrainMesh();
    }

    private void CreateShape()
    {
        // 점 생성
        Mvertices = new Vector3[(Xsize + 1) * (Zsize + 1)];

        for (int c = 0, z = 0; z <= Zsize; z++)
        {
            for (int x = 0; x <= Xsize; x++)
            {
                // Terrain 랜덤 높낮이 정의
                float y = 0;
                if(IsNoise)
                    y = Mathf.PerlinNoise(x * .3f, z * 3f) * NoiseForce;

                Mvertices[c] = new Vector3(x, y, z);
                ++c;
            }
        }

        // 점을 기준으로 TerrainMesh 생성
        Mtriangles = new int[Xsize * Zsize * 6];

        for(int tris = 0, vert = 0, z = 0; z < Zsize; z++)
        {
            for (int x = 0; x < Xsize; x++)
            {
                Mtriangles[tris + 0] = vert + 0;
                Mtriangles[tris + 1] = vert + Xsize + 1;
                Mtriangles[tris + 2] = vert + 1;
                Mtriangles[tris + 3] = vert + 1;
                Mtriangles[tris + 4] = vert + Xsize + 1;
                Mtriangles[tris + 5] = vert + Xsize + 2;

                ++vert;
                tris += 6;

            }
            ++vert;
        }
    }

    private void UpdateTerrainMesh()
    {
        // TerrainMesh 생성되는 동안의 모습을 모여줌
        TerrainMesh.Clear();

        TerrainMesh.vertices = this.Mvertices;
        TerrainMesh.triangles = this.Mtriangles;

        TerrainMesh.RecalculateNormals();

        Debug.Log("Generating is Completed");
    }
}
