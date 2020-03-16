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
    private float HeightForce;

    private Terrain FieldTerrain;
    private float A, B;

    private void Start()
    {
        FieldTerrain = GetComponent<Terrain>();
        FieldTerrain.terrainData = GenerateTerrain(FieldTerrain.terrainData);
    }

    private TerrainData GenerateTerrain(TerrainData _data)
    {
        _data.heightmapResolution = Width + 1;
        _data.size = new Vector3(Width, Depth, Height);

        float[,] Heights = new float[Width, Height];
        GenerateHeights(ref Heights);
        GenerateRiver(ref Heights);
        _data.SetHeights(0, 0, Heights);

        return _data;
    }

    private void GenerateHeights(ref float[,] _field)
    {
        float _height;
        for(int x = 0; x < Width; x++)
            for(int z = 0; z < Height; z++)
            {
                _height = CalculateRandomHeight(x, z);
                if (_height < 0.2)
                    _field[x, z] = 0.2f;
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
                int RF = Random.Range(1, 10);
                int z = CalculateHeightSpot(x);
                for (int i = x - RF; i < x + RF; i++)
                {
                    for (int j = z - RF; j < z + RF; j++)
                    {
                        if (i < 0 || i >= Width || j < 0 || j >= Height)
                            continue;
                        _field[j, i] = -10;
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
}
