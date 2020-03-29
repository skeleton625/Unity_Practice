using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private TerrainInfo Info;
    [SerializeField]
    private RiverGenerator River;

    private void Awake()
    {
        GenerateTerrain();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(1))
            GenerateTerrain();
    }

    private void GenerateTerrain()
    {
        GenerateDefaultHeights();
        if(River != null)
            River.GenerateStraightRiver();
        Info.ApplyPreTerrainHeights();
    }

    private void GenerateDefaultHeights()
    {
        int _width = Info.Width, _height = Info.Height;
        float _depth, _limit = Info.HeightLimit;

        for(int x = 0; x < _width; x++)
            for(int z = 0; z < _height; z++)
            {
                _depth = CalculateRandomHeight(x, z);
                if (_depth < _limit)
                    Info[z, x] = _limit;
                else
                    Info[z, x] = _depth;
            }
                
    }

    private float CalculateRandomHeight(float _x, float _z)
    {
        float _xCoord = _x / Info.Width * Info.Scale;
        float _zCoord = _z / Info.Height * Info.Scale;

        return Mathf.PerlinNoise(_xCoord, _zCoord);
    }
}
