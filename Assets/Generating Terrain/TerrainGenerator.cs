using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private int Width, Height, Depth;
    [SerializeField]
    private float Scale;
    [SerializeField]
    private Transform[] Vertices;
    [SerializeField]
    private int RiverBound;
    [SerializeField]
    private float HeightLimit;
    [SerializeField]
    private float RiverDepth;

    private float[,] TerrainArray;
    private int[,] ArrayDir;
    private Terrain FieldTerrain;
    private float A, B;

    private void Start()
    {
        ArrayDir = new int[8, 2]
        {
            {-1, -1 }, {-1, 0 },{-1, 1 },
            {0, -1 }, {0, 1 },
            {1, -1 }, {1, 0 }, {1, 1 }
        };
        TerrainArray = new float[Width, Height];
        FieldTerrain = GetComponent<Terrain>();
        FieldTerrain.terrainData = GenerateTerrain(FieldTerrain.terrainData);
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
            FieldTerrain.terrainData = GenerateTerrain(FieldTerrain.terrainData);
    }

    private TerrainData GenerateTerrain(TerrainData _data)
    {
        _data.heightmapResolution = Width + 1;
        _data.size = new Vector3(Width, Depth, Height);
        GenerateDefaultHeights();
        GenerateRiver();
        _data.SetHeights(0, 0, TerrainArray);

        return _data;
    }

    private void GenerateDefaultHeights()
    {
        float _height;
        for(int x = 0; x < Width; x++)
            for(int z = 0; z < Height; z++)
            {
                _height = CalculateRandomHeight(x, z);
                if (_height < HeightLimit)
                    TerrainArray[z, x] = HeightLimit;
                else
                    TerrainArray[z, x] = _height;
            }
                
    }

    private void GenerateRiver()
    {
        for(int c = 0; c < Vertices.Length-1; c++)
        {
            Vector3 _start = Vertices[c].position, _end = Vertices[c + 1].position;
            float value = HeightLimit - RiverBound * RiverDepth;
            
            CalculateAandB(_start, _end);
            for (int x = (int)_start.x; x < _end.x; x++)
            {
                int _bound = Random.Range(1, RiverBound);
                int _nz = CalculateHeightSpot(x);
                if (_nz < 0 || _nz >= Width)
                    continue;

                TerrainArray[_nz, x] = value;
                DownValueInArray(x, _nz, value, 0);
            }

            for (int z = (int)_start.z; z < _end.z; z++)
            {
                int _bound = Random.Range(1, RiverBound);
                int _nx = CalculateWidthSpot(z);
                if(_nx < 0 || _nx >= Height)
                    continue;

                TerrainArray[z, _nx] = value;
                DownValueInArray(_nx, z, value, 0);
            }
        }
    }

    private void DownValueInArray(int x, int z, float val, int cnt)
    {
        if (cnt > RiverBound)
            return;

        int nx, nz;
        for (int i = 0; i < 8; i++)
        {
            nx = x + ArrayDir[i, 0];
            nz = z + ArrayDir[i, 1];

            if (nx < 0 || nx >= Width || nz < 0 || nz >= Height)
                continue;
            else if (TerrainArray[nz, nx] < val)
                continue;

            TerrainArray[nz, nx] = val;
            DownValueInArray(nx, nz, val + RiverDepth, cnt + 1);
        }
    }

    private float CalculateRandomHeight(float _x, float _z)
    {
        float _xCoord = _x / Width * Scale;
        float _zCoord = _z / Height * Scale;

        return Mathf.PerlinNoise(_xCoord, _zCoord);
    }

    private void CalculateAandB(Vector3 _s, Vector3 _e)
    {
        float x1 = _s.x, x2 = _e.x;
        float z1 = _s.z, z2 = _e.z;

        A = (z2 - z1) / (x2 - x1);
        B = (x2 * z1 - x1 * z2) / (x2 - x1);
    }

    private int CalculateHeightSpot(int x)
    {
        return (int)(A * x + B);
    }

    private int CalculateWidthSpot(int z)
    {
        return (int)((z - B) / A);
    }
}
