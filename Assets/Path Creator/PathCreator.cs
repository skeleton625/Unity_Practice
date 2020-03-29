using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;
    private Path newPath;

    // Inspector 창을 통해 조절할 수 있는 변수들
    public Color anchorCol = Color.red;
    public Color controlCol = Color.white;
    public Color segmentCol = Color.green;
    public Color selectedSegmentCol = Color.yellow;
    public float anchorDiameter = .1f;
    public float controlDiameter = .075f;
    public bool displayControlPoints = true;
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

    private int[,] dir = new int[8, 2]
     { {-1, -1 }, {-1, 0 }, {-1, 1 }, {0, -1 }, {0, 1 }, {1, -1 }, {1, 0 }, {1, 1 } };

    // Inspector 창에서 스크립트 컴포넌트를 Reset할 경우 실행
    private void Reset() { CreatePath(); }

    private void Start() { CreatePath(); }

    public void CreatePath()
    {
        Vector3 _pos = transform.position;
        _pos.y = FieldInfo.Depth * FieldInfo[(int)_pos.x, (int)_pos.z];
        path = new Path(_pos);
    }

    public void CreateRiver()
    {
        Vector3[] points = path.CalculateEvenlySpacedPoitns(spacing, resolution);

        bool[,] visited = new bool[FieldInfo.Width, FieldInfo.Height];
        newPath = new Path(points[0]);

        int _x, _z;
        foreach(Vector3 p in points)
        {
            _x = (int)p.x;
            _z = (int)p.z;

            if (!visited[_z, _x])
            {
                newPath.AddSegment(SetHeights(_x, _z));
                FieldInfo[_z, _x] -= DefaultDepth;
                SpreadRiver(ref visited, _x, _z, 0);
            }
            else
                SpreadRiver(ref visited, _x, _z, 0);
        }

       
        FieldInfo.ApplyPreTerrainHeights();
        newPath.DeleteSegment(0);
        newPath.DeleteSegment(1);
        path = newPath;
    }

    private void SpreadRiver(ref bool[,] visited, int x, int z, int cnt)
    {
        if (cnt > RiverWidth)
            return;

        int nx, nz;
        for(int i = 0; i < 8; i++)
        {
            nx = x + dir[i, 0];
            nz = z + dir[i, 1];
            if (nz < 0 || nz >= FieldInfo.Width || nx < 0 || nx >= FieldInfo.Height)
                continue;
            if(!visited[nz, nx])
            {
                visited[nz, nx] = true;
                FieldInfo[nz, nx] -= DefaultDepth;
                SpreadRiver(ref visited, nx, nz, cnt + 1);
            }
            else
                SpreadRiver(ref visited, nx, nz, cnt + 1);
        }
    }

    public Vector3 SetHeights(float x, float z)
    {
        float y = FieldInfo.Depth * FieldInfo[(int)z, (int)x];
        return new Vector3(x, y, z);
    }
}
