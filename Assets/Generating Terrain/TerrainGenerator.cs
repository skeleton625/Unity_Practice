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
    private Transform Sobject, Eobject;
    [SerializeField]
    private int RF;


    private Vector3 Spoint, Epoint;
    private Terrain FieldTerrain;
    private float A, B;

    private void Start()
    {
        Spoint = Sobject.position;
        Epoint = Eobject.position;
        FieldTerrain = GetComponent<Terrain>();
        FieldTerrain.terrainData = GenerateTerrain(FieldTerrain.terrainData, 0, 0, Width, Height);
    }

    private void Update()
    {
        if(Sobject.position != Spoint || Eobject.position != Epoint)
        {
            Spoint = Sobject.position;
            Epoint = Eobject.position;
            FieldTerrain.terrainData = GenerateTerrain(FieldTerrain.terrainData, 0, 0, Width, Height);
        }
    }

    private TerrainData GenerateTerrain(TerrainData _data, int _sx, int _sz, int _ex, int _ez)
    {
        CalculateAandB();
        _data.heightmapResolution = Width + 1;
        _data.size = new Vector3(Width, Depth, Height);
        _data.SetHeights(_sx, _sz, GenerateHeights(_ex-_sx, _ez - _sz, (int)Spoint.x, (int)Epoint.x, (int)Epoint.z));

        return _data;
    }

    private float[,] GenerateHeights(int _width, int _height, int _sx, int _ex, int _ez)
    {
        float[,] _heights = new float[_width, _height];

        for(int x = 0; x < _width; x++)
            for(int z = 0; z < _height; z++)
                _heights[x, z] = CalculateRandomHeight(x, z);

        for (int x = _sx; x < _ex; x++)
        {
            int z = CalculateHeightSpot(x);
            for(int i = x-RF; i < x + RF; i++)
            {
                for(int j = z-RF; j < z +RF; j++)
                {
                    if (i < 0 || i >= _width || j < 0 || j >= _height)
                        continue;
                    _heights[j, i] = 0;
                }
            }
        }

        return _heights;
    }

    private float CalculateRandomHeight(float _x, float _z)
    {
        float _xCoord = _x / Width * Scale;
        float _zCoord = _z / Height * Scale;

        return Mathf.PerlinNoise(_xCoord, _zCoord);
    }

    private void CalculateAandB()
    {
        float x1 = Spoint.x, x2 = Epoint.x;
        float z1 = Spoint.z, z2 = Epoint.z;

        A = (z2 - z1) / (x2 - x1);
        B = (x2 * z1 - x1 * z2) / (x2 - x1);
    }

    private int CalculateHeightSpot(int x)
    {
        return (int)(A * x + B);
    }
}
