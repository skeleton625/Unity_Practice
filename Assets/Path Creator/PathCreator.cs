﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator : MonoBehaviour
{
    // 경로 좌표 객체
    [HideInInspector]
    public Path path;

    // Inspector 창을 통해 조절할 수 있는 변수들
    /* Path UI의 색, 크기, 회전 점의 표시여부 */
    public Color anchorCol = Color.red;
    public Color controlCol = Color.white;
    public Color segmentCol = Color.green;
    public Color selectedSegmentCol = Color.yellow;
    public float anchorDiameter = .1f;
    public float controlDiameter = .075f;
    public bool displayControlPoints = true;
    public bool AutoRiver;

    [SerializeField]
    private Texture2D[] DepthTex;

    [SerializeField]
    private int RandomSize;
    [SerializeField]
    private float spacing = .1f;
    [SerializeField]
    private float resolution = 1f;
    // 강 깊이 설정 변수
    [SerializeField]
    private float strength;
    private float realStrength;
    // 주류의 비주류 Object
    [SerializeField]
    private GameObject SubRiver;
    // Path를 통한 강 생성 관련 변수들
    private TerrainInfo FieldInfo;

    // Inspector 창에서 스크립트 컴포넌트를 Reset할 경우 실행
    private void Reset() { CreatePath(); }

    // 새 경로 생성 함수
    public void CreatePath()
    {
        Vector3 startPos = new Vector3(-transform.localScale.x / 2, 0, -transform.localScale.z / 2);
        startPos = transform.rotation * startPos;

        int x = (int)(transform.position.x + startPos.x);
        int z = (int)(transform.position.z + startPos.z);

        startPos.x = x;
        startPos.z = z;
        if (FieldInfo == null)
            FieldInfo = TerrainInfo.instance;
        path = new Path(FieldInfo.SetRealHeight(startPos));
    }

    public void CreateRandomRiver(int BrushSize)
    {
        CreatePath();
        // 랜덤 경로를 위한 최소, 최대 범위 지정
        float minX = - transform.localScale.x / 2;
        float maxX = transform.localScale.x / 2;
        int minZ = (int) - transform.localScale.z / 2;
        int maxZ = (int) transform.localScale.z / 2;

        // 랜덤 경로 지정
        int z;
        for(int x = (int)minX + RandomSize; x < maxX; x += RandomSize)
        {
            z = Random.Range(minZ, maxZ);
            Vector3 riverPos = new Vector3(x, 0, z);
            /* Vector.zero를 기준으로 transfrom.rotation 만큼 회전 */
            riverPos = transform.rotation * riverPos;

            riverPos.x = transform.position.x + riverPos.x;
            riverPos.z = transform.position.z + riverPos.z;

            path.AddSegment(FieldInfo.SetRealHeight(riverPos));
            path.AutoSetControlPoints = true;
        }

        CreateRiver(BrushSize);
    }

    private void CreateSubRiver(Vector3 start, Vector3 next)
    {
        start.y = 0;
        GameObject clone = Instantiate(SubRiver, start, Quaternion.identity);

        Vector3 forward = next - start;
        forward.Normalize();
        Vector3 left = new Vector3(forward.z, forward.y, -forward.x);

        start += left * 5;
        clone.transform.rotation = Quaternion.LookRotation(start);
        clone.transform.Translate(clone.transform.localScale.x/2, 0, clone.transform.localScale.z/2);
        clone.GetComponent<PathCreator>().CreateRandomRiver(0);
    }

    public void CreateRiver(int BrushSize)
    {
        /* 강 생성을 위한 선 위의 점 좌표, 방문 여부 배열 정의 */
        Vector3[] points = path.CalculateEvenlySpacedPoitns(spacing, resolution);
        bool[,] visited = new bool[FieldInfo.Width, FieldInfo.Height];
        /* 강 생성 후, 해당 경로에 대한 새 경로 객체 정의 */
        Path newPath = new Path(points[0]);
        realStrength = strength / 1500.0f;

        /* 강 시작 점을 경로에 추가 */
        int _x, _z;
        _x = (int)points[0].x;
        _z = (int)points[0].z;
        newPath.AddSegment(SetHeights(_x, _z));

        int defTime = 30;
        /* 시작, 끝 점을 제외한 나머지 점들에 대해 Terrain 높낮이를 조절 */
        for (int i = 1; i < points.Length - 1; i++)
        {
            _x = (int)points[i].x;
            _z = (int)points[i].z;
            newPath.AddSegment(SetHeights(_x + DepthTex[BrushSize].height / 2, _z + DepthTex[BrushSize].width / 2));
            _x += Random.Range(-1, 1);
            _z += Random.Range(-1, 1);

            if (BrushSize != 0 && i % defTime == 0)
                CreateSubRiver(points[i], points[i + 1]);

            /* Brush 이미지 크기에 따라 지형 변환을 진행 */
            float y;
            int bx, bz;

            Color[] brushData = DepthTex[BrushSize].GetPixels();
            for (int j = 0; j < DepthTex[BrushSize].height; j++)
            {
                for (int k = 0; k < DepthTex[BrushSize].width; k++)
                {
                    bx = _x + k;
                    bz = _z + j;
                    y = FieldInfo[bz, bx] - brushData[j * DepthTex[BrushSize].width + k].a * realStrength;
                    if(y < FieldInfo[bz, bx])
                        FieldInfo[bz, bx] = y;
                }
            }
        }

        /* 강 끝 점을 경로에 추가 */
        _x = (int)points[points.Length - 1].x + 10;
        _z = (int)points[points.Length - 1].z;
        newPath.AddSegment(new Vector3(_x, points[points.Length - 2].y, _z));

        /* 현재 정의된 높낮이를 Terrain의 높낮이로 정의 */
        FieldInfo.ApplyPreTerrainHeights();
        /* 경로에 혼선을 주는 시작 점 좌표들 제거 및 새 경로로 정의 */
        newPath.DeleteSegment(0);
        newPath.DeleteSegment(0);
        path = newPath;
        path.AutoSetControlPoints = true;

        gameObject.GetComponent<TexCreator>().UpdateTexture();
    }

    /* Terrain의 상대적 높낮이(0~1)가 아닌 실제 높낮이를 계산하는 함수 */
    public Vector3 SetHeights(float x, float z)
    {
        return FieldInfo.SetRealHeight(new Vector3(x, 0, z));
    }
}
