using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{
    public static Vector3 EvaluateQuadratic(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        // a, b 지점 사이의 t * 100 % 에 위치한 좌표
        Vector3 p0 = Vector3.Lerp(a, b, t);
        // b, c 지점 사이의 t * 100 % 에 위치한 좌표
        Vector3 p1 = Vector3.Lerp(b, c, t);
        // p0, p1 지점 사이의 t * 100 % 에 위치한 좌표
        return Vector3.Lerp(p0, p1, t);
    }

    public static Vector3 EvaluateCubic(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
    {
        Vector3 p0 = EvaluateQuadratic(a, b, c, t);
        Vector3 p1 = EvaluateQuadratic(b, c, d, t);
        return Vector3.Lerp(p0, p1, t);
    }
}
