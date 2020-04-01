using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;
    private TexCreator texture;

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
    private int RandomSize;
    // Path를 통한 강 생성 관련 변수들
    [SerializeField]
    private TerrainInfo FieldInfo;
    [SerializeField]
    private float DefaultDepth;
    [SerializeField]
    private int RiverWidth;
    [SerializeField]
    private float spacing = .1f;
    [SerializeField]
    private float resolution = 1f;

    // 강 너비를 위한 전방향 배열
    private int[,] dir = new int[8, 2]
     { {-1, -1 }, {-1, 0 }, {-1, 1 }, {0, -1 }, {0, 1 }, {1, -1 }, {1, 0 }, {1, 1 } };

    // Inspector 창에서 스크립트 컴포넌트를 Reset할 경우 실행
    private void Reset() { CreatePath(); }

    private void Start() {
        texture = GetComponent<TexCreator>();
        CreatePath();
    }

    public void CreateRandomPath()
    {
        
    }

    // 새 경로 생성 함수
    public void CreatePath()
    {
        int x = (int)(transform.position.x - transform.localScale.x / 2);
        int z = (int)(transform.position.z - transform.localScale.z / 2);
        float y = FieldInfo.SetRealHeight(x, z);
        path = new Path(new Vector3(x, y, z));
    }

    public void CreateRandomRiver()
    {
        int minX = (int)(transform.position.x - transform.localScale.x / 2);
        int maxX = (int)(transform.position.x + transform.localScale.x / 2);
        int minZ = (int)(transform.position.z - transform.localScale.z / 2) ;
        int maxZ = (int)(transform.position.z + transform.localScale.z / 2);
        int z;
        float h;
        for(int x = minX + RandomSize; x < maxX; x += RandomSize)
        {
            z = Random.Range(minZ, maxZ);
            h = FieldInfo.SetRealHeight(x, z);
            path.AddSegment(new Vector3(x, h, z));
        }

        CreateRiver();
    }

    public void CreateRiver()
    {
        /* 강 생성을 위한 선 위의 점 좌표, 방문 여부 배열 정의 */
        Vector3[] points = path.CalculateEvenlySpacedPoitns(spacing, resolution);
        bool[,] visited = new bool[FieldInfo.Width, FieldInfo.Height];
        /* 강 생성 후, 해당 경로에 대한 새 경로 객체 정의 */
        Path newPath = new Path(points[0]);

        /* 강 시작 점을 경로에 추가 */
        int _x, _z;
        _x = (int)points[0].x;
        _z = (int)points[0].z;
        newPath.AddSegment(SetHeights(_x, _z));

        /* 시작, 끝 점을 제외한 나머지 점들에 대해 Terrain 높낮이를 조절 */
        for(int i = 1; i < points.Length - 1; i++)
        {
            _x = (int)points[i].x;
            _z = (int)points[i].z;

            /* 방문하지 않은 좌표에 대해서만 조절 가능 */
            if (!visited[_z, _x])
            {
                visited[_z, _x] = true;
                newPath.AddSegment(SetHeights(_x, _z));
                FieldInfo[_z, _x] -= DefaultDepth;
                /* 강의 퍼짐을 표현하기 위한 좌표 주위의 좌표들에 대해 높낮이를 정의 */
                SpreadRiver(ref visited, _x, _z, 0);
            }
            else
                /* 강의 퍼짐을 표현하기 위한 좌표 주위의 좌표들에 대해 높낮이를 정의 */
                SpreadRiver(ref visited, _x, _z, 0);
        }

        /* 강 끝 점을 경로에 추가 */
        _x = (int)points[points.Length - 1].x;
        _z = (int)points[points.Length - 1].z;
        newPath.AddSegment(new Vector3(_x, points[points.Length - 2].y, _z));

        /* 현재 정의된 높낮이를 Terrain의 높낮이로 정의 */
        FieldInfo.ApplyPreTerrainHeights();
        /* 경로에 혼선을 주는 시작 점 좌표들 제거 및 새 경로로 정의 */
        newPath.DeleteSegment(0);
        newPath.DeleteSegment(1);
        path = newPath;
        path.AutoSetControlPoints = true;

        /* 새 경로에 대한 Mesh 계산 및 생성 */
        if(texture != null)
            texture.UpdateTexture();
    }

    /* 주어진 x, z 좌표를 기준으로 주위 좌표를 같은 높이로 정의하는 함수 */
    private void SpreadRiver(ref bool[,] visited, int x, int z, int cnt)
    {
        if (cnt > RiverWidth)
            return;

        int nx, nz;
        /* 기준 좌표에서 팔방으로 존재하는 좌표에 같은 높이를 적용 */
        for(int i = 0; i < 8; i++)
        {
            nx = x + dir[i, 0];
            nz = z + dir[i, 1];
            if (nz < 0 || nz >= FieldInfo.Width || nx < 0 || nx >= FieldInfo.Height)
                continue;
            if(!visited[nz, nx])
            {
                visited[nz, nx] = true;
                /*현재 높낮이를 표현할 수 있도록 정의된 깊이에서 현재 높낮이를 뺌 */
                FieldInfo[nz, nx] -= DefaultDepth;
                SpreadRiver(ref visited, nx, nz, cnt + 1);
            }
            else
                SpreadRiver(ref visited, nx, nz, cnt + 1);
        }
    }

    /* Terrain의 상대적 높낮이(0~1)가 아닌 실제 높낮이를 계산하는 함수 */
    public Vector3 SetHeights(float x, float z)
    {
        float y = FieldInfo.SetRealHeight((int)x, (int)z);
        return new Vector3(x, y, z);
    }
}
