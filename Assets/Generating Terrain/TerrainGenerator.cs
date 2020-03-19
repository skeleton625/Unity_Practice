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

    private Terrain FieldTerrain;
    private float A, B;

    private void Start()
    {
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
        float[,] Heights = new float[Width, Height];

        GenerateDefaultHeights(ref Heights);
        GenerateRiver(ref Heights);

        _data.heightmapResolution = Width + 1;
        _data.size = new Vector3(Width, Depth, Height);
        _data.SetHeights(0, 0, Heights);

        return _data;
    }

    private void GenerateDefaultHeights(ref float[,] _field)
    {
        float _height;
        for(int x = 0; x < Width; x++)
            for(int z = 0; z < Height; z++)
            {
                _height = CalculateRandomHeight(x, z);
                if (_height < HeightLimit)
                    _field[x, z] = HeightLimit;
                else
                    _field[x, z] = _height;
            }
                
    }

    private void GenerateRiver(ref float[,] _field)
    {
        for(int c = 0; c < Vertices.Length-1; c++)
        {
            Vector3 _start = Vertices[c].position, _end = Vertices[c + 1].position;
            
            CalculateAandB(_start, _end);
            for (int x = (int)_start.x; x < _end.x; x++)
            {
                int _bound = Random.Range(1, RiverBound);
                int _nz = CalculateHeightSpot(x);
                for (int i = x - _bound; i < x + _bound; i++)
                {
                    for (int j = _nz - _bound; j < _nz + _bound; j++)
                    {
                        if (i < 0 || i >= Width || j < 0 || j >= Height)
                            continue;
                        _field[j, i] = 0.3f;
                    }
                }
            }

            for (int z = (int)_start.z; z < _end.z; z++)
            {
                int _bound = Random.Range(1, RiverBound);
                int _nx = CalculateWidthSpot(z);
                for (int i = z - _bound; i < z + _bound; i++)
                {
                    for (int j = _nx - _bound; j < _nx + _bound; j++)
                    {
                        if (i < 0 || i >= Height || j < 0 || j >= Width)
                            continue;
                        _field[i, j] = 0.3f;
                    }
                }
            }
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
