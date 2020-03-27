using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path
{

    [SerializeField, HideInInspector]
    private List<Vector3> points;
    [SerializeField, HideInInspector]
    private bool isClosed;
    [SerializeField, HideInInspector]
    private bool autoSetControlPoints;

    // center 좌표를 기준으로 4개의 점을 정의
    public Path(Vector3 center)
    {
        points = new List<Vector3>
        {
            center + Vector3.left,                          /* (-1, 0, 0) 시작 점 */
            center + (Vector3.left + Vector3.forward)*.5f,  /* (-1, 0, 1) 시작 점의 회전 점 */
            center + (Vector3.right + Vector3.back)*.5f,    /* (1, 0, -1) 끝 점의 회전 점*/
            center + Vector3.right                          /* (1, 0, 0) 끝 점*/
        };
    }

    // 경로 위의 점 위치
    public Vector3 this[int i]
    { get => points[i]; }

    // 경로 자동 조정 여부
    public bool AutoSetControlPoints
    {
        get => autoSetControlPoints;
        set
        {
            if (autoSetControlPoints != value)
            {
                autoSetControlPoints = value;
                // 경로 자동 조정 값이 true일 경우 자동 조정 실행
                if (autoSetControlPoints)
                    AutoSetAllControlPoints();
            }
        }
    }

    // 모든 기준 점이 연결되도록 최초 시작 점과 마지막 끝 점을 연결하거나 끊는 함수
    public bool IsClosed
    {
        get => isClosed;
        set
        {
            isClosed = value;

            /* 연결되지 않을 상태였을 경우 */
            if (isClosed)
            {
                /* 시작 점의 회전 점 추가 */
                points.Add(points[0] * 2 - points[1]);
                /* 끝 점의 회전 점 추가 */
                points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                /* 자동 조정이 true일 경우 자동 조정 진행 */
                if (autoSetControlPoints)
                {
                    AutoSetAnchorControlPoints(0);
                    AutoSetAnchorControlPoints(points.Count - 3);
                }
            }
            /* 연결되어 있는 상태였을 경우 */
            else
            {
                points.RemoveRange(points.Count - 2, 2);
                if (autoSetControlPoints)
                {
                    AutoSetStartAndEndControls();
                }
            }
        }
    }

    // 경로 위의 모든 점 개수
    public int NumPoints
    { get => points.Count; }

    // 경로 위의 기준 점 개수
    public int NumSegments
    { get => points.Count / 3; }

    // 새 기준 점을 anchorPos 좌표에 추가하는 함수
    public void AddSegment(Vector3 anchorPos)
    {
        anchorPos.y = 0;
        /* 이전 점에 대한 회전 점 추가 */
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        /* 새 기준 점 anchorPos의 회전 점 추가 */
        points.Add((points[points.Count - 1] + anchorPos) * .5f);
        /* anchorPos 기준 점 추가 */
        points.Add(anchorPos);

        // 자동 조정이 true일 경우 새 기준 점을 추가한 것에 대해 새 조정 진행
        if (autoSetControlPoints)
            AutoSetAllAffectedControlPoints(points.Count - 1);
    }

    public void SplitSegment(Vector3 anchorPos, int segmentIndex)
    {
        points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, anchorPos, Vector3.zero });
        if (autoSetControlPoints)
            AutoSetAllAffectedControlPoints(segmentIndex * 3 + 2);
        else
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
    }

    // 선택한 기준 점을 삭제하는 함수
    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments > 2 || !isClosed && NumSegments > 1)
        {
            if (anchorIndex.Equals(0))
            {
                if (isClosed)
                    points[points.Count - 1] = points[2];
                points.RemoveRange(0, 3);
            }
            else if (anchorIndex.Equals(points.Count - 1) && !isClosed)
                points.RemoveRange(anchorIndex - 2, 3);
            else
                points.RemoveRange(anchorIndex - 1, 3);
        }
    }

    // i 번째 시작 점에서 i 번째 끝 점까지의 좌표 배열 반환하는 함수
    public Vector3[] GetPointsInSegment(int i)
    {
        int num = i * 3;
        return new Vector3[] { points[num], points[num + 1], points[num + 2], points[LoopIndex(num + 3)] };
    }

    // 기준 점 및 회전 점에 위치를 이동시키는 함수 (자동 조정 또한 포함)
    public void MovePoint(int i, Vector3 pos)
    {
        Vector3 deltaMove = pos - points[i];
        deltaMove.y = 0;
        /* 
         * autoSetControlPoints -> 자동 조정 여부
         * i % 3 -> 기준 점
         */
        if (i % 3 == 0 || !autoSetControlPoints)
        {
            pos.y = 0;
            points[i] = pos;

            if (autoSetControlPoints)
            {
                AutoSetAllAffectedControlPoints(i);
            }
            else
            {
                /* 현재 점이 기준 점인 경우 */
                if (i % 3 == 0)
                {
                    /* 기준 점 i의 이동에 따른 앞, 뒤 회전 점의 위치 조정 */
                    if (i + 1 < points.Count || isClosed)
                        points[LoopIndex(i + 1)] += deltaMove;
                    if (i - 1 >= 0 || isClosed)
                        points[LoopIndex(i - 1)] += deltaMove;
                }
                /* 현재 점이 회전 점인 경우 */
                else
                {
                    bool nextPointIsAnchor = (i + 1) % 3 == 0;
                    int anchorIndex = (nextPointIsAnchor) ? i + 1 : i - 1;
                    int correspondingControlIndex = (nextPointIsAnchor) ? i + 2 : i - 2;

                    /* 회전 점이 속한 선의 다른 회전점에 위치를 조정 */
                    if (correspondingControlIndex >= 0 && correspondingControlIndex < points.Count || isClosed)
                    {
                        Vector3 dir = (points[LoopIndex(anchorIndex)] - pos).normalized;
                        float dst = (points[LoopIndex(anchorIndex)] - points[LoopIndex(correspondingControlIndex)]).magnitude;
                        points[LoopIndex(correspondingControlIndex)] = points[LoopIndex(anchorIndex)] + dir * dst;
                    }
                }
            }
        }
    }

    /*** 자동 조정 관련 함수들 ***/

    // 점들의 좌표 이동 시, 자동 조정을 갱신하는 함수
    void AutoSetAllAffectedControlPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {
            if (i >= 0 && i < points.Count || isClosed)
                AutoSetAnchorControlPoints(LoopIndex(i));
        }

        AutoSetStartAndEndControls();
    }

    // 모든 점에 대해 자동 조정을 진행하는 함수
    void AutoSetAllControlPoints()
    {
        for (int i = 0; i < points.Count; i += 3)
            AutoSetAnchorControlPoints(i);

        AutoSetStartAndEndControls();
    }

    // 시작 점, 끝 점을 제외한 중간 기준 점(anchorIndex)들에 대해 자동 조정을 진행하는 함수
    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Vector3 anchorPos = points[anchorIndex];
        Vector3 dir = Vector3.zero;
        float[] neighbourDistances = new float[2];

        /* 각 기준점에 대해 이웃 점 간의 적정 거리를 계산 */
        if (anchorIndex - 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }
        if (anchorIndex + 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        /* 계산한 거리를 이웃 점에게 정의 */
        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (controlIndex >= 0 && controlIndex < points.Count || isClosed)
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * .5f;
        }
    }

    // 시작 점과 끝 점에 자동 조정을 진행하는 함수
    void AutoSetStartAndEndControls()
    {
        if (!isClosed)
        {
            points[1] = (points[0] + points[2]) * .5f;
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * .5f;
        }
    }

    // 번호 i가 점 개수보다 많아지면 시작부터 다시 증가하도록 하는 함수
    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }
}