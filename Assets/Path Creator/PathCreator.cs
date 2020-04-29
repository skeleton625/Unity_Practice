using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    // Inspector 창을 통해 조절할 수 있는 변수들
    /* Path UI의 색, 크기, 회전 점의 표시여부 */
    [SerializeField]
    private RiverData datas;
    [HideInInspector]
    public RiverPath path;

    public bool displayControlPoints = true;
    public bool AutoRiver;
    [SerializeField]
    private TerrainGenerator generator;
    [SerializeField]
    public GameObject NewRiver;


    // Inspector 창에서 스크립트 컴포넌트를 Reset할 경우 실행
    private void Reset() { CreatePath(); }

    // 새 경로 생성 함수
    public void CreatePath()
    {
        path = new RiverPath(generator.SetRealHeight(transform.position));
    }

    public void CreateRandomRiver(int DepthLevel, float WaterLevel, float Space, float Strength)
    {
        datas.spacing = Space;
        datas.WaterLevel = WaterLevel;
        datas.DepthLevel = DepthLevel;
        datas.RealStrength = Strength / 1500;

        int width = datas.RiverWidth;
        int height = datas.RiverHeight;
        int interval = datas.IntervalSize;
        CreatePath();
        // 랜덤 경로 지정
        for(int x = interval; x <= width; x += interval)
        {
            Vector3 riverPos = new Vector3(x, 0, Random.Range(-height, height));
            /* Vector.zero를 기준으로 transfrom.rotation 만큼 회전 */
            riverPos = transform.rotation * riverPos;
            /* 회전 후, 점 위치를 원래 위치로 이동 */
            riverPos.x = transform.position.x + riverPos.x;
            riverPos.z = transform.position.z + riverPos.z;

            path.AddSegment(generator.SetRealHeight(riverPos));
        }

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

            if (x >= 1024)
                break;

            newPath.AddSegment(generator.SetDownTerrain(x, z, DepthLevel, datas.RealStrength));
        }
        /* 강 끝 점을 경로에 추가 */
        newPath.AutoSetControlPoints = true;

        /* 경로에 혼선을 주는 시작 점 좌표들 제거 */
        newPath.DeleteSegment(0);
        newPath.DeleteSegment(0);
        /* 자연스러운 강 곡선 수정 */
        path = newPath;

        UpdateTexture(newPath);
    }

    /* Terrain의 상대적 높낮이(0~1)가 아닌 실제 높낮이를 계산하는 함수 */
    public Vector3 SetHeights(float x, float y, float z)
    {
        Vector3 pos = generator.SetRealHeight(new Vector3(x, 0, z));
        if (pos.y < y)
            pos.y = y;
        return pos;
    }

    private void UpdateTexture(RiverPath path)
    {
        GameObject River = Instantiate(NewRiver, Vector3.zero, Quaternion.identity);
        River.transform.position = new Vector3(0, -1f, 0);

        Mesh RiverMesh = CreateTexMesh(path);
        River.GetComponent<MeshFilter>().mesh = RiverMesh;
        River.GetComponent<MeshCollider>().sharedMesh = RiverMesh;

        gameObject.SetActive(false);
        River.transform.parent = GameObject.Find("MainRiver").transform;
    }

    private Mesh CreateTexMesh(RiverPath path)
    {
        Vector3[] verts = new Vector3[path.NumPoints * 2];
        Vector2[] uvs = new Vector2[verts.Length];
        int[] tris = new int[2 * (path.NumPoints - 1) * 3];
        int vertIndex = 0, triIndex = 0;
        int[] riverWidth = new int[3] { 5, 15, 30 };

        for (int i = 0; i < path.NumPoints; i += 3)
        {
            Vector3 forward = Vector3.zero;
            if (i < path.NumPoints - 1)
                forward += path[(i + 1) % path.NumPoints] - path[i];
            if (i > 0)
                forward += path[i] - path[(i - 1 + path.NumPoints) % path.NumPoints];
            forward.Normalize();
            Vector3 left = new Vector3(forward.z, forward.y, -forward.x);
            verts[vertIndex] = generator.SetRealHeight(path[i] - left * riverWidth[datas.DepthLevel]);
            verts[vertIndex + 1] = generator.SetRealHeight(path[i] + left * riverWidth[datas.DepthLevel]);

            for(int j = 0; j < 2; j++)
                verts[vertIndex + j].y -= (datas.WaterLevel + 0.05f);

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
}
