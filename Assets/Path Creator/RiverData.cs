﻿using UnityEngine;

[CreateAssetMenu(fileName = "New River Data", menuName = "FieldData/River")]
public class RiverData : ScriptableObject
{
    // Inspector 창을 통해 조절할 수 있는 변수들
    /* Path UI의 색, 크기, 회전 점의 표시여부 */
    [HideInInspector]
    public RiverPath path;

    public int RiverWidth, RiverHeight;
    public int IntervalSize = 50;
    public float spacing = .1f;
    public float resolution = 1f;
}
