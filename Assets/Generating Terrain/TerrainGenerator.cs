using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private FieldMapData datas;
    [SerializeField]
    private float OffsetX, OffsetZ;
    [SerializeField]
    private Texture2D[] DepthBrush;
    private int depthCount;
    public int DepthCount { get => depthCount; }

    // Terrain의 높낮이 배열
    private float[,] hArray;
    public float[,] HArray
    { get => hArray; }
    private float[,] pArray;
    public float[,] PArray
    { get => pArray; }
    // Terrain 텍스처 배열
    private float[,,] SplatmapData;

    private void Awake()
    {
        depthCount = DepthBrush.Length;
        // 기존 PerlinNoize에 변칙성을 추가
        OffsetX = Random.Range(0, 9999f);
        OffsetZ = Random.Range(0, 9999f);
        // PerlinNoize가 들어간 Terrain 생성
        hArray = new float[datas.Width, datas.Height];
        pArray = new float[datas.Width, datas.Height];

        datas.FieldTerrain = GetComponent<Terrain>();
        TerrainData tData = datas.FieldTerrain.terrainData;
        SplatmapData = new float[tData.alphamapWidth,
                                 tData.alphamapHeight,
                                 tData.alphamapLayers];
        GenerateTerrain();
    }

    // 전체 Terrain의 형태를 구현하는 함수 
    private void GenerateTerrain()
    {
        GenerateSlopHeights();

        SetRandomTerrainTextures();
        ApplyPreTerrainHeights();
    }

    private void GenerateSlopHeights()
    {
        float hVal = 0.5f, depth;
        float limit = datas.HeightLimit;
        int height = datas.Height, width = datas.Width;

        for(int x = 0; x < height; x++)
        {
            for(int z = 0; z < width; z++)
            {
                depth = CalculateRandomHeight(x, z) + hVal;
                depth = depth > 1 ? 1 : depth;
                if (depth < limit)
                    pArray[z, x] = limit;
                else
                    pArray[z, x] = depth;
            }
            hVal -= 0.0003f;
        }
    }

    private void SetRandomTerrainTextures()
    {
        TerrainData terrainData = datas.FieldTerrain.terrainData;

        float preHeight;
        int TBlen = datas.TexBound.Length;
        for (int x = 0; x < terrainData.alphamapHeight; x++)
        {
            for (int z = 0; z < terrainData.alphamapWidth; z++)
            {
                preHeight = CalculateRandomHeight(x, z);
                float[] splat = new float[TBlen];

                for (int i = 0; i < TBlen - 1; i++)
                {
                    if (preHeight >= datas.TexBound[i].startingHeight &&
                        preHeight <= datas.TexBound[i + 1].startingHeight)
                        splat[datas.TexBound[i].textureIndex] = 1;
                }

                if (splat[0] == 0 && splat[1] == 0 && splat[2] == 0)
                    splat[datas.TexBound[1].textureIndex] = 1;

                for (int i = 0; i < TBlen; i++)
                    SplatmapData[z, x, i] = splat[i];
            }
        }
    }

    // PerlinNoise 알고리즘을 통한 Terrain 높낮이 정의 함수
    private float CalculateRandomHeight(float _x, float _z)
    {
        float _xCoord = _x / datas.Width * datas.Scale + OffsetX;
        float _zCoord = _z / datas.Height * datas.Scale + OffsetZ;

        return Mathf.PerlinNoise(_xCoord, _zCoord);
    }

    public void GenerateRestrictHeights()
    {
        int width = datas.Width;
        int height = datas.Height;
        float depth, limit = datas.HeightLimit;

        for (int x = 0; x < width; x++)
            for (int z = 0; z < height; z++)
            {
                /* Terrain 내 모든 부분에 대해 랜덤 높낮이를 정의 */
                depth = CalculateRandomHeight(x, z) - 0.2f;
                /* 랜덤 값이 기준 높이보다 낮을 경우, 기준 높이로 통일 */
                if (pArray[z, x] == limit)
                    pArray[z, x] = depth < limit ? limit : depth;
            }

        ApplyPreTerrainHeights();
    }

    public Vector3 SetDownTerrain(int px, int pz, int bSize, float strength)
    {
        int dx = DepthBrush[bSize].height / 2;
        int dz = DepthBrush[bSize].width / 2;

        int bx, bz;
        float nh;

        Color[] brushData = DepthBrush[bSize].GetPixels();
        for (int x = 0; x < DepthBrush[bSize].height; x++)
        {
            for (int z = 0; z < DepthBrush[bSize].width; z++)
            {
                bx = px + x;
                bz = pz + z;
                if (bx < 0 || bx >= datas.Height || bz < 0 || bz >= datas.Width)
                    continue;

                pArray[bz, bx] -= brushData[x * DepthBrush[bSize].width + z].a * strength;
                if (hArray[bz, bx] - pArray[bz, bx] < 0.005)
                {
                    SplatmapData[bz, bx, 0] = 0.2f;
                    if (SplatmapData[bz, bx, 1] == 1)
                        SplatmapData[bz, bx, 1] = 0.8f;
                    else
                        SplatmapData[bz, bx, 2] = 0.8f;
                }
                else
                {
                    SplatmapData[bz, bx, 0] = 1;
                    SplatmapData[bz, bx, 1] = 0;
                    SplatmapData[bz, bx, 2] = 0;
                }
            }
        }
        return SetRealHeight(new Vector3(px + dx, 0, pz + dz));
    }

    //  현재 정의된 높낮이 배열로 Terrain의 전체 높낮이를 설정하는 함수
    public void ApplyPreTerrainHeights()
    {
        Terrain terrain = datas.FieldTerrain;
        TerrainData data = terrain.terrainData;
        int width = datas.Width, height = datas.Height;

        data.heightmapResolution = width + 1;
        data.size = new Vector3(width, datas.Depth, height);

        data.SetHeights(0, 0, pArray);
        data.SetAlphamaps(0, 0, SplatmapData);
        terrain.terrainData = data;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
                hArray[x, z] = pArray[x, z];
        }
    }

    public Vector3 SetRealHeight(Vector3 pos)
    {
        int px = (int)pos.x;
        int pz = (int)pos.z;

        if (px < 0) px = 0;
        else if (px >= datas.Height) px = datas.Height - 1;
        if (pz < 0) pz = 0;
        else if (pz >= datas.Width) pz = datas.Width - 1;

        pos.y = datas.Depth * hArray[pz, px] - 1f;
        return pos;
    }
}
