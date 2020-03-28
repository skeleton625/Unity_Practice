using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    [HideInInspector]
    public Path path;

    // Inspector 창을 통해 조절할 수 있는 변수들
    public Color anchorCol = Color.red;
    public Color controlCol = Color.white;
    public Color segmentCol = Color.green;
    public Color selectedSegmentCol = Color.yellow;
    public float anchorDiameter = .1f;
    public float controlDiameter = .075f;
    public bool displayControlPoints = true;
    [SerializeField]
    private Terrain FieldTerrain;
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
    private float[,] TerrainArray;
    private bool[,] visited;

    // Inspector 창에서 스크립트 컴포넌트를 Reset할 경우 실행
    private void Reset()
    {
        CreatePath();
    }

    public void CreatePath()
    {
        Vector3 _pos = transform.position;
        TerrainArray = FieldTerrain.terrainData.GetHeights(0, 0, FieldInfo.Width, FieldInfo.Height);
        _pos.y = FieldInfo.Depth * TerrainArray[(int)_pos.x, (int)_pos.z];
        path = new Path(_pos);
    }

    public void CreateRiver()
    {
        Vector3[] points = path.CalculateEvenlySpacedPoitns(spacing, resolution);

        FieldTerrain.terrainData.heightmapResolution = FieldInfo.Width + 1;
        FieldTerrain.terrainData.size = new Vector3(FieldInfo.Width, FieldInfo.Depth, FieldInfo.Height);

        if(TerrainArray == null)
        {
            TerrainArray = FieldTerrain.terrainData.GetHeights(0, 0, FieldInfo.Width, FieldInfo.Height);
        }

        visited = new bool[FieldInfo.Width, FieldInfo.Height];

        int _x, _z;
        foreach(Vector3 p in points)
        {
            _x = (int)p.x;
            _z = (int)p.z;
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = p;
            g.transform.localScale = Vector3.one * spacing * .5f;

            if (!visited[_z, _x])
            {
                TerrainArray[_z, _x] -= DefaultDepth;
                SpreadRiver(_x, _z, 0, DefaultDepth);
            }
            else
                SpreadRiver(_x, _z, 0, DefaultDepth);
        }

        FieldTerrain.terrainData.SetHeights(0, 0, TerrainArray);
    }

    private void SpreadRiver(int x, int z, int cnt, float val)
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
                TerrainArray[nz, nx] -= val;
                SpreadRiver(nx, nz, cnt + 1, DefaultDepth);
            }
            else
                SpreadRiver(nx, nz, cnt + 1, DefaultDepth);
        }
    }

    public Vector3 SetHeights(Vector3 pos)
    {
        pos.y = FieldInfo.Depth * TerrainArray[(int)pos.x, (int)pos.z];
        return pos;
    }

    private void OnEnable()
    {
        TerrainArray = FieldTerrain.terrainData.GetHeights(0, 0, FieldInfo.Width, FieldInfo.Height);
    }
}
