using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainInfo : MonoBehaviour
{
    [SerializeField]
    private int width, height, depth, scale;
    [SerializeField]
    private Terrain FieldTerrain;
    [SerializeField]
    private float heightLimit;
    private float[,] hArray;
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

    public int Width
    { get => width; }
    public int Height
    { get => height; }
    public int Depth
    { get => depth; }
    public int Scale
    { get => scale; }
    public float HeightLimit
    { get => heightLimit; }

    private void Awake()
    {
        hArray = new float[width, height];
    }

    public void ApplyPreTerrainHeights()
    {
        TerrainData data = FieldTerrain.terrainData;
        data.heightmapResolution = width + 1;
        data.size = new Vector3(width, depth, height);

        data.SetHeights(0, 0, hArray);
        FieldTerrain.terrainData = data;
    }

    public float[,] GetPreHeights()
    {
        return hArray;
    }
}
