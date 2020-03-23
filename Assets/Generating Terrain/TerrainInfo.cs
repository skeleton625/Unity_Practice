using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainInfo : MonoBehaviour
{
    [SerializeField]
    private int width, height, depth;
    public int Width
    { get => width; }
    public int Height
    { get => height; }
    public int Depth
    { get => depth; }
    [SerializeField]
    private float heightLimit;
    public float HeightLimit
    { get => heightLimit; }

    public static TerrainInfo Instance;

    private void Awake()
    {
        Instance = this;
    }
}
