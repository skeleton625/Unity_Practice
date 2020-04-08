using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    // 경로 좌표 객체
    [HideInInspector]
    public Path path;

    // Inspector 창을 통해 조절할 수 있는 변수들
    /* Path UI의 색, 크기, 회전 점의 표시여부 */
    public Color anchorCol = Color.red;
    public Color controlCol = Color.white;
    public Color segmentCol = Color.green;
    public Color selectedSegmentCol = Color.yellow;
    public float anchorDiameter = .1f;
    public float controlDiameter = .075f;
    public bool displayControlPoints = true;
    public bool AutoRiver;

    [SerializeField]
    private Texture2D[] DepthTex;

    [SerializeField]
    private int IntervalSize = 50;
    [SerializeField]
    private float spacing = .1f;
    [SerializeField]
    private float resolution = 1f;
    // 강 깊이 설정 변수
    [SerializeField]
    private float strength;
    private float realStrength;
    // 주류의 비주류 Object
    [SerializeField]
    private GameObject SubRiver;
    // Path를 통한 강 생성 관련 변수들
    private TerrainInfo FieldInfo;

    // Inspector 창에서 스크립트 컴포넌트를 Reset할 경우 실행
    private void Reset() { CreatePath(); }

    // 새 경로 생성 함수
    public void CreatePath()
    {
        Vector3 startPos = new Vector3(-transform.localScale.x / 2, 0, -transform.localScale.z / 2);
        startPos = transform.rotation * startPos;

        startPos.x = (int)(transform.position.x + startPos.x);
        startPos.z = (int)(transform.position.z + startPos.z);

        if (FieldInfo == null)
            FieldInfo = TerrainInfo.instance;
        path = new Path(FieldInfo.SetRealHeight(startPos));
    }

    public void CreateRandomRiver(int BrushSize)
    {
        CreatePath();
        // 랜덤 경로를 위한 최소, 최대 범위 지정
        float minX = - transform.localScale.x / 2;
        float maxX = transform.localScale.x / 2;
        int minZ = (int) - transform.localScale.z / 2;
        int maxZ = (int) transform.localScale.z / 2;

        // 랜덤 경로 지정
        for(int x = (int)minX + IntervalSize; x <= maxX; x += IntervalSize)
        {
            Vector3 riverPos = new Vector3(x, 0, Random.Range(minZ, maxZ));
            /* Vector.zero를 기준으로 transfrom.rotation 만큼 회전 */
            riverPos = transform.rotation * riverPos;
            /* 회전 후, 점 위치를 원래 위치로 이동 */
            riverPos.x = transform.position.x + riverPos.x;
            riverPos.z = transform.position.z + riverPos.z;

            path.AddSegment(FieldInfo.SetRealHeight(riverPos));
        }

        /* Terrain 깍기 시작 */
        CreateRiver(BrushSize);
    }

    private void CreateSubRiver(Vector3 start, Vector3 next, bool isLeft)
    {
        start.y = 0;
        GameObject clone = Instantiate(SubRiver, start, Quaternion.identity);

        /* 비주류 강의 각도 정의 */
        Vector3 forward = next - start;
        forward.Normalize();
        Vector3 left = new Vector3(forward.z, forward.y, -forward.x);

        start += left * 5;
        clone.transform.rotation = Quaternion.LookRotation(start);

        /* 주류 강을 기준으로 왼쪽, 오른쪽 위치인지에 따라 이동 및 회전 */
        if (isLeft)
            clone.transform.Translate(clone.transform.localScale.x / 2, 0, clone.transform.localScale.z / 2);
        else
        {
            clone.transform.Translate(-clone.transform.localScale.x / 2, 0, -clone.transform.localScale.z / 2);
            clone.transform.Rotate(0, 180, 0);
        }
        clone.GetComponent<PathCreator>().CreateRandomRiver(0);
    }

    public void CreateRiver(int BrushSize)
    {
        /* 강 생성을 위한 선 위의 점 좌표, 방문 여부 배열 정의 */
        Vector3[] points = path.CalculateEvenlySpacedPoitns(spacing, resolution);
        bool[,] visited = new bool[FieldInfo.Width, FieldInfo.Height];
        /* 강 생성 후, 해당 경로에 대한 새 경로 객체 정의 */
        Path newPath = new Path(points[0]);
        realStrength = strength / 1500.0f;

        /* 강 시작 점을 경로에 추가 */
        newPath.AddSegment(SetHeights((int)points[0].x, (int)points[0].z));

        Color[] brushData = DepthTex[BrushSize].GetPixels();
        /* 시작, 끝 점을 제외한 나머지 점들에 대해 Terrain 높낮이를 조절 */
        for (int i = 1; i < points.Length - 1; i++)
        {
            int px = (int)points[i].x + Random.Range(-1, 1);
            int pz = (int)points[i].z + Random.Range(-1, 1);
            newPath.AddSegment(SetHeights(px + DepthTex[BrushSize].height / 2, pz + DepthTex[BrushSize].width / 2));

            /* Brush 이미지 크기에 따라 지형 변환을 진행 */
            float y;
            int bx, bz;
            for (int j = 0; j < DepthTex[BrushSize].height; j++)
            {
                for (int k = 0; k < DepthTex[BrushSize].width; k++)
                {
                    bx = px + k;
                    bz = pz + j;
                    y = FieldInfo[bz, bx] - brushData[j * DepthTex[BrushSize].width + k].a * realStrength;
                    if(y < FieldInfo[bz, bx])
                        FieldInfo[bz, bx] = y;
                }
            }
        }

        /* 강 끝 점을 경로에 추가 */
        newPath.AddSegment(new Vector3((int)points[points.Length - 1].x + 10, 
                                            points[points.Length - 2].y,
                                       (int)points[points.Length - 1].z));

        /* 현재 정의된 높낮이를 Terrain의 높낮이로 정의 */
        FieldInfo.ApplyPreTerrainHeights();
        /* 경로에 혼선을 주는 시작 점 좌표들 제거 */
        newPath.DeleteSegment(0);
        newPath.DeleteSegment(0);
        /* 자연스러운 강 곡선 수정 */
        newPath.AutoSetControlPoints = true;

        gameObject.GetComponent<TexCreator>().UpdateTexture(newPath);
        if(BrushSize != 0)
        {
            for (int i = 6; i < path.NumPoints - 3; i+=3)
                CreateSubRiver(path[i], path[i + 3], i % 2 == 0 ? true : false);
            gameObject.GetComponent<TexCreator>().SetCombineAllRiverMesh();
        }
                
    }

    /* Terrain의 상대적 높낮이(0~1)가 아닌 실제 높낮이를 계산하는 함수 */
    public Vector3 SetHeights(float x, float z)
    {
        return FieldInfo.SetRealHeight(new Vector3(x, 0, z));
    }
}
