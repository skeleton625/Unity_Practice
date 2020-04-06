using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainInfo : MonoBehaviour
{
    // Terrain의 가로, 세로, 깊이, 높낮이 규모 정의
    /* Terrain의 가로, 세로 정의의 경우, 2의 제곱 수로 정의해야 함 */
    [SerializeField]
    private int width, height, depth, scale;
    [SerializeField]
    private Terrain FieldTerrain;
    [SerializeField]
    private float heightLimit;

    // Terrain의 높낮이 배열
    private float[,] hArray;
    // TerrainInfo 객체를 통해 높낮이 배열에 접근할 수 있음
    public float this[int x, int z]
    {
        get
        {
            if (x < 0 || z < 0 || x >= height || z >= width)
                return 0;
            return hArray[x, z];
        }
        set
        {
            if (x < 0 || z < 0 || x >= height || z >= width)
                return;
            hArray[x, z] = value;
        }
    }

    public int Width { get => width; }
    public int Height { get => height; }
    public int Depth { get => depth; }
    public int Scale { get => scale; }
    public float HeightLimit { get => heightLimit; }
    public static TerrainInfo instance;

    private void Awake()
    {
        hArray = new float[width, height];
        instance = this;
    }

    //  현재 정의된 높낮이 배열로 Terrain의 전체 높낮이를 설정하는 함수
    public void ApplyPreTerrainHeights()
    {
        TerrainData data = FieldTerrain.terrainData;
        data.heightmapResolution = width + 1;
        data.size = new Vector3(width, depth, height);

        data.SetHeights(0, 0, hArray);
        FieldTerrain.terrainData = data;
    }

    public float SetRealHeight(int x, int z)
    {
        if (x < 0 || x >= height || z < 0 || z >= width)
            return 0;
        return depth * hArray[z, x];
    }

    public Vector3 SetRealHeight(Vector3 pos)
    {
        if (pos.x < 0 || pos.x >= height || pos.z < 0 || pos.z >= width)
            return pos;

        pos.y = depth * hArray[(int)pos.z, (int)pos.x];
        return pos;
    }
}
