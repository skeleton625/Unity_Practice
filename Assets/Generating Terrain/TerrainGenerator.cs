using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private float Scale;
    [SerializeField]
    private TerrainInfo Info;
    [SerializeField]
    private RiverGenerator River;

    private Terrain FieldTerrain;


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
        _data.heightmapResolution = Info.Width + 1;
        _data.size = new Vector3(Info.Width, Info.Depth, Info.Height);

        float[,] _field = new float[Info.Width, Info.Height];
        GenerateDefaultHeights(ref _field);
        River.GenerateStraightRiver(ref _field);
        _data.SetHeights(0, 0, _field);

        return _data;
    }

    private void GenerateDefaultHeights(ref float[,] _field)
    {
        int _width = Info.Width, _height = Info.Height;
        float _depth, _limit = Info.HeightLimit;

        for(int x = 0; x < _width; x++)
            for(int z = 0; z < _height; z++)
            {
                _depth = CalculateRandomHeight(x, z);
                if (_depth < _limit)
                    _field[z, x] = _limit;
                else
                    _field[z, x] = _depth;
            }
                
    }

    private float CalculateRandomHeight(float _x, float _z)
    {
        float _xCoord = _x / Info.Width * Scale;
        float _zCoord = _z / Info.Height * Scale;

        return Mathf.PerlinNoise(_xCoord, _zCoord);
    }
}
