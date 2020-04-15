using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField]
    private TerrainInfo Info;
    [SerializeField]
    private RiverGenerator River;
    [SerializeField]
    private float OffsetX, OffsetZ;

    private void Awake()
    {
        // 기존 PerlinNoize에 변칙성을 추가
        OffsetX = Random.Range(0, 9999f);
        OffsetZ = Random.Range(0, 9999f);
        // PerlinNoize가 들어간 Terrain 생성
        GenerateTerrain();
    }

    // 전체 Terrain의 형태를 구현하는 함수 
    private void GenerateTerrain()
    {
        //GenerateDefaultHeights(Info.Scale);
        GenerateSlopHeights(Info.Scale);
        if(River != null)
            River.GenerateStraightRiver();
        Info.ApplyPreTerrainHeights();
    }

    // 랜덤으로 Terrain의 각 위치를 조정해 Terrain의 형태를 구현하는 함수
    private void GenerateDefaultHeights(float Scale)
    {
        float _depth, _limit = Info.HeightLimit;

        for(int x = 0; x < Info.Height; x++)
            for(int z = 0; z < Info.Width; z++)
            {
                /* Terrain 내 모든 부분에 대해 랜덤 높낮이를 정의 */
                _depth = CalculateRandomHeight(x, z, Scale);
                /* 랜덤 값이 기준 높이보다 낮을 경우, 기준 높이로 통일 */
                if (_depth < _limit)
                    Info[z, x] = _limit;
                else
                    Info[z, x] = _depth;
            }
    }

    private void GenerateSlopHeights(float Scale)
    {
        float hVal = 0.5f, depth;
        for(int x = 0; x < Info.Height; x++)
        {
            for(int z = 0; z < Info.Width; z++)
            {
                depth = CalculateRandomHeight(x, z, Scale);
                Info[z, x] = depth + hVal;
            }
            hVal -= 0.0005f;
        }
    }

    public void GenerateRestrictHeights(float Scale)
    {
        int _width = Info.Width, _height = Info.Height;
        float _depth, _limit = Info.HeightLimit;

        for (int x = 0; x < _width; x++)
            for (int z = 0; z < _height; z++)
            {
                /* Terrain 내 모든 부분에 대해 랜덤 높낮이를 정의 */
                _depth = CalculateRandomHeight(x, z, Scale);
                /* 랜덤 값이 기준 높이보다 낮을 경우, 기준 높이로 통일 */
                if (Info[z, x] == _limit)
                    Info[z, x] = _depth < _limit ? _limit : _depth;
            }

        Info.ApplyPreTerrainHeights();
    }

    // PerlinNoise 알고리즘을 통한 Terrain 높낮이 정의 함수
    private float CalculateRandomHeight(float _x, float _z, float Scale)
    {
        float _xCoord = _x / Info.Width * Scale + OffsetX;
        float _zCoord = _z / Info.Height * Scale + OffsetZ;

        return Mathf.PerlinNoise(_xCoord, _zCoord) - 0.2f;
    }
}
