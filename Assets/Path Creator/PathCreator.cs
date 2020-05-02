using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    // Inspector 창을 통해 조절할 수 있는 변수들
    /* Path UI의 색, 크기, 회전 점의 표시여부 */
    [SerializeField]
    private RiverData datas;
    [SerializeField]
    public GameObject NewRiver, SubRiver;
    [HideInInspector]
    public RiverPath path;
    public bool displayControlPoints = true;
    public bool AutoRiver;

    private TerrainGenerator generator;

    // Inspector 창에서 스크립트 컴포넌트를 Reset할 경우 실행
    private void Reset() { CreatePath(); }

    // 새 경로 생성 함수
    public void CreatePath()
    {
        path = new RiverPath(generator.SetRealHeight(transform.position));
    }

    public void CreateRandomRiver(int DepthLevel, float WaterLevel, float Strength, TerrainGenerator Generator)
    {
        datas.spacing = datas.spaces[DepthLevel];
        datas.WaterLevel = WaterLevel;
        datas.DepthLevel = DepthLevel;
        datas.RealStrength = Strength;
        generator = Generator;

        int width = datas.RiverWidth;
        int height = datas.RiverHeight;
        int interval = datas.IntervalSize;
        CreatePath();

        // 랜덤 경로 지정
        for (int x = interval; x <= width; x += interval)
        {
            Vector3 riverPos = new Vector3(x, 0, Random.Range(-height, height));
            /* Vector.zero를 기준으로 transfrom.rotation 만큼 회전 */
            riverPos = transform.rotation * riverPos;
            /* 회전 후, 점 위치를 원래 위치로 이동 */
            riverPos.x = transform.position.x + riverPos.x;
            riverPos.z = transform.position.z + riverPos.z;

            path.AddSegment(generator.SetRealHeight(riverPos));
        }
        /* 지류 강의 시작 부, 끊김을 방지하기 위함 */
        path.DeleteSegment(0);

        /* Terrain 깍기 시작 */
        CreateRiver(DepthLevel);
    }

    /* 현재 CreateRiver 함수는 CreateRandomRiver 함수에 종속적 (후에 병합 필요) */
    public void CreateRiver(int DepthLevel)
    {
        /* 강 생성을 위한 선 위의 점 좌표, 방문 여부 배열 정의 */
        Vector3[] points = path.CalculateEvenlySpacedPoitns(datas.spacing, datas.resolution);
        /* 강 생성 후, 해당 경로에 대한 새 경로 객체 정의 */
        RiverPath newPath = new RiverPath(points[0]);

        int i = 1;
        /* 강 시작 점을 경로에 추가 */
        newPath.AddSegment(generator.SetRealHeight(points[0]));
        /* 시작, 끝 점을 제외한 나머지 점들에 대해 Terrain 높낮이를 조절 */

        for (; i < points.Length - 1; i++)
        {
            int x = (int)points[i].x;
            int z = (int)points[i].z;

            if ((x - transform.position.x) >= 1024 || (z - transform.position.z) >= 1024)
                break;

            Vector3 pos = generator.SetDownTerrain(x, z, DepthLevel, datas.RealStrength);

            /* mesh 간격을 위해 짝수 좌표에 대해서만 경로에 입력 */
            newPath.AddSegment(pos);
        }

        newPath.AddSegment(generator.SetRealHeight(points[i]));
        /* 경로에 혼선을 주는 시작 점 좌표들 제거 */
        newPath.DeleteSegment(0);
        newPath.DeleteSegment(0);
        /* 자연스러운 강 곡선으로 수정 */
        newPath.AutoSetControlPoints = true;
        path = newPath;

        UpdateTexture();
    }

    /* Terrain의 상대적 높낮이(0~1)가 아닌 실제 높낮이를 계산하는 함수 */
    public Vector3 SetHeights(float x, float y, float z)
    {
        Vector3 pos = generator.SetRealHeight(new Vector3(x, 0, z));
        if (pos.y < y)
            pos.y = y;
        return pos;
    }

    private void UpdateTexture()
    {
        GameObject River = Instantiate(NewRiver, Vector3.zero, Quaternion.identity);
        River.transform.position = new Vector3(0, -1f, 0);

        Mesh RiverMesh = CreateTexMesh();
        River.GetComponent<MeshFilter>().mesh = RiverMesh;
        River.GetComponent<MeshCollider>().sharedMesh = RiverMesh;

        gameObject.SetActive(false);
        River.transform.parent = GameObject.Find("MainRiver").transform;
    }

    private Mesh CreateTexMesh()
    {
        Vector3[] verts = new Vector3[path.NumPoints * 3];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[4 * (path.NumPoints - 1) * 3];
        int[] riverWidth = new int[3] { 5, 10, 18 };
        int vertIndex = 0, triIndex = 0;

        for (int i = 0; i < path.NumPoints; i += 3)
        {
            Vector3 forward = Vector3.zero;
            if (i < path.NumPoints - 1)
                forward += path[(i + 1) % path.NumPoints] - path[i];
            if (i > 0)
                forward += path[i] - path[(i - 1 + path.NumPoints) % path.NumPoints];
            forward.Normalize();

            // calculate mesh vertex left, right position code
            Vector3 left = new Vector3(forward.z, forward.y, -forward.x);
            verts[vertIndex] = generator.SetRealHeight(path[i] - left * riverWidth[datas.DepthLevel]);
            verts[vertIndex + 1] = path[i];
            verts[vertIndex + 2] = generator.SetRealHeight(path[i] + left * riverWidth[datas.DepthLevel]);

            if(i % 90 == 0 && SubRiver != null)
            {
                int depths = datas.DepthLevel - 1 < 0 ? 0 : datas.DepthLevel - 1;
                GameObject clone = Instantiate(SubRiver, Vector3.zero, Quaternion.identity);
                clone.transform.position = path[i];
                if(Random.Range(0, 2) == 0)
                    clone.transform.LookAt(verts[vertIndex]);
                else
                    clone.transform.LookAt(verts[vertIndex + 2]);
                clone.transform.Rotate(0, -90, 0);
                clone.GetComponent<PathCreator>().CreateRandomRiver(depths, 0, datas.RealStrength, generator);
                clone.SetActive(false);
            }

            for(int j = 0; j < 3; j++)
                verts[vertIndex + j].y -= (datas.WaterLevel + 0.02f);

            // i / path.NumPoints -> river completionPercent
            float v = 1 - i / (float)(path.NumPoints - 1); 
            // uv : Texture move value
            uvs[vertIndex] = new Vector2(1, v);
            uvs[vertIndex + 1] = new Vector2(.5f, v);
            uvs[vertIndex + 2] = new Vector2(0, v);

            // mesh Generate Code
            if(i < path.NumPoints - 1)
            {
                for (int j = 0; j < 12; j += 6)
                {
                    tris[triIndex + j] = vertIndex;
                    tris[triIndex + 1 + j] = (vertIndex + 3) % verts.Length;
                    tris[triIndex + 2 + j] = vertIndex + 1;

                    tris[triIndex + 3 + j] = vertIndex + 1;
                    tris[triIndex + 4 + j] = (vertIndex + 3) % verts.Length;
                    tris[triIndex + 5 + j] = (vertIndex + 4) % verts.Length;
                    ++vertIndex;
                }
            }
            vertIndex += 1;
            triIndex += 12;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.uv = uvs;
        return mesh;
    }
}
