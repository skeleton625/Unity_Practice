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
    private bool IsDebugging;
    [SerializeField]
    private bool IsNoise;
    private bool IsGenerating;
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

        if (IsDebugging)
            DebugTimer = new WaitForSeconds(.1f);
        else
            DebugTimer = null;

        StartCoroutine(CreateShape());
        StartCoroutine(UpdateTerrainMesh());
    }

    private IEnumerator CreateShape()
    {
        // TerrainMesh 생성 시작
        IsGenerating = true;

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

                yield return DebugTimer;
            }
            ++vert;
        }

        // TerrainMesh 생성 종료
        IsGenerating = false;
    }

    private IEnumerator UpdateTerrainMesh()
    {
        // TerrainMesh 생성되는 동안의 모습을 모여줌
        while (IsGenerating)
        {
            TerrainMesh.Clear();

            TerrainMesh.vertices = this.Mvertices;
            TerrainMesh.triangles = this.Mtriangles;

            TerrainMesh.RecalculateNormals();

            yield return null;
        }

        Debug.Log("Generating is Completed");
    }

    private void OnDrawGizmos()
    {
        // TerrainMesh 뼈대 표현
        if (Mvertices == null)
            return;

        for(int c = 0; c < Mvertices.Length; c++)
        {
            Gizmos.DrawSphere(Mvertices[c], 0.1f);
        }
    }
}
