using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    // Inspector 창을 통해 조절할 수 있는 변수들
    /* Path UI의 색, 크기, 회전 점의 표시여부 */
    [HideInInspector]
    public Color anchorCol = Color.red;
    [HideInInspector]
    public Color controlCol = Color.white;
    [HideInInspector]
    public Color segmentCol = Color.green;
    [HideInInspector]
    public Color selectedSegmentCol = Color.yellow;
    [HideInInspector]
    public float anchorDiameter = 1f;
    [HideInInspector]
    public float controlDiameter = 0.5f;
    [HideInInspector]
    public Path path;

    public bool displayControlPoints = true;
    public bool AutoRiver;
    [SerializeField]
    private int RiverWidth, RiverHeight;
    [SerializeField]
    private int IntervalSize = 50;
    [SerializeField]
    private float spacing = .1f;
    [SerializeField]
    private float resolution = 1f;
    // Path를 통한 강 생성 관련 변수들
    private TerrainInfo FieldInfo;

    // Inspector 창에서 스크립트 컴포넌트를 Reset할 경우 실행
    private void Reset() { CreatePath(); }

    // 새 경로 생성 함수
    public void CreatePath()
    {
        if (FieldInfo == null)
            FieldInfo = TerrainInfo.instance;
        path = new Path(FieldInfo.SetRealHeight(transform.position));
    }

    public void CreateRandomRiver(int BrushSize)
    {
        CreatePath();
        // 랜덤 경로 지정
        for(int x = 0; x <= RiverWidth; x += IntervalSize)
        {
            Vector3 riverPos = new Vector3(x, 0, Random.Range(-RiverHeight, RiverHeight));
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

    public void CreateRiver(int BrushSize)
    {
        /* 강 생성을 위한 선 위의 점 좌표, 방문 여부 배열 정의 */
        Vector3[] points = path.CalculateEvenlySpacedPoitns(spacing, resolution);
        /* 강 생성 후, 해당 경로에 대한 새 경로 객체 정의 */
        Path newPath = new Path(points[0]);

        /* 강 시작 점을 경로에 추가 */
        newPath.AddSegment(FieldInfo.SetRealHeight(points[0]));

        /* 시작, 끝 점을 제외한 나머지 점들에 대해 Terrain 높낮이를 조절 */
        for (int i = 1; i < points.Length - 1; i++)
            newPath.AddSegment(FieldInfo.SetDownTerrain((int)points[i].x, (int)points[i].z, BrushSize));

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
        path = newPath;
        path.AutoSetControlPoints = true;

        gameObject.GetComponent<TexCreator>().UpdateTexture(newPath);
        GameObject.Find("Terrain").GetComponent<TerrainGenerator>().GenerateRestrictHeights(10);
    }

    /* Terrain의 상대적 높낮이(0~1)가 아닌 실제 높낮이를 계산하는 함수 */
    public Vector3 SetHeights(float x, float y, float z)
    {
        Vector3 pos = FieldInfo.SetRealHeight(new Vector3(x, 0, z));
        if (pos.y < y)
            pos.y = y;
        return pos;
    }
}
