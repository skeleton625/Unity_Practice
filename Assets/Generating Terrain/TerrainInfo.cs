using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainInfo : MonoBehaviour
{
    [System.Serializable]
    private struct SplatHeights
    {
        public int textureIndex;
        public int startingHeight;
    }
    // Terrain의 가로, 세로, 깊이, 높낮이 규모 정의
    /* Terrain의 가로, 세로 정의의 경우, 2의 제곱 수로 정의해야 함 */
    [SerializeField]
    private int width, height, depth, scale;
    [SerializeField]
    private Terrain FieldTerrain;
    [SerializeField]
    private float heightLimit;
    [SerializeField]
    private Texture2D[] DepthBrush;
    [SerializeField]
    private float Strength;
    private float realStrength;
    [SerializeField]
    private SplatHeights[] splatHeights;
    [SerializeField]
    private GameObject Grass;


    // Terrain의 높낮이 배열
    private float[,] hArray;
    private float[,] pArray;
    // Terrain 텍스처 배열
    private float[,,] splatmapData;
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
            pArray[x, z] = value;
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
        TerrainData tData = FieldTerrain.terrainData;
        instance = this;
        hArray = new float[width, height];
        pArray = new float[width, height];
        splatmapData = new float[tData.alphamapWidth, 
                                 tData.alphamapHeight,
                                 tData.alphamapLayers];
        realStrength = Strength / 1500.0f;
    }

    //  현재 정의된 높낮이 배열로 Terrain의 전체 높낮이를 설정하는 함수
    public void ApplyPreTerrainHeights()
    {
        TerrainData data = FieldTerrain.terrainData;
        data.heightmapResolution = width + 1;
        data.size = new Vector3(width, depth, height);

        data.SetHeights(0, 0, pArray);
        data.SetAlphamaps(0, 0, splatmapData);
        FieldTerrain.terrainData = data;
        
        for(int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
                hArray[x, z] = pArray[x, z];
        }
    }

    public Vector3 SetRealHeight(Vector3 pos)
    {
        if (pos.x < 0 || pos.x >= height || pos.z < 0 || pos.z >= width)
            return pos;

        int px = (int)pos.x;
        int pz = (int)pos.z;

        pos.y = depth * hArray[pz, px];
        return pos;
    }

    public Vector3 SetDownTerrain(int px, int pz, int bSize)
    {
        int dx = DepthBrush[bSize].height / 2;
        int dz = DepthBrush[bSize].width / 2;

        int bx, bz;
        float nh;

        Color[] brushData = DepthBrush[bSize].GetPixels();
        for(int x = 0; x < DepthBrush[bSize].height; x++)
        {
            for (int z = 0; z < DepthBrush[bSize].width; z++)
            {
                bx = px + x;
                bz = pz + z;
                if (bx < 0 || bx >= height || bz < 0 || bz >= width)
                    continue;
                nh = pArray[bz, bx] - brushData[x * DepthBrush[bSize].width + z].a * realStrength;
                if (nh == pArray[bz, bx])
                {
                    splatmapData[bz, bx, 0] = 0.4f;
                    if (splatmapData[bz, bx, 1] == 1)
                        splatmapData[bz, bx, 1] = 0.6f;
                    else
                        splatmapData[bz, bx, 2] = 0.6f;
                }
                else
                {
                    splatmapData[bz, bx, 0] = 1;
                    splatmapData[bz, bx, 1] = 0;
                    splatmapData[bz, bx, 2] = 0;
                }
                pArray[bz, bx] = nh;
            }
        }
        return SetRealHeight(new Vector3(px + dx, 0, pz + dz));
    }

    public void SetRandomTerrainTextures()
    {
        TerrainData terrainData = FieldTerrain.terrainData;

        float preHeight;
        for(int x = 0; x < terrainData.alphamapHeight; x++)
        {
            for (int z = 0; z < terrainData.alphamapWidth; z++)
            {
                preHeight = Depth * pArray[z, x];
                float[] splat = new float[splatHeights.Length];

                for(int i = 0; i < splatHeights.Length-1; i++)
                {
                    if (preHeight >= splatHeights[i].startingHeight && preHeight <= splatHeights[i + 1].startingHeight)
                        splat[splatHeights[i].textureIndex] = 1;
                }

                if (splat[0] == 0 && splat[1] == 0 && splat[2] == 0)
                    splat[splatHeights[1].textureIndex] = 1;


                for(int i = 0; i < splatHeights.Length; i++)
                    splatmapData[z, x, i] = splat[i];
            }
        }
    }
}
